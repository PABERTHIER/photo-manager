using System.Drawing.Imaging;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class StorageServiceTests
{
    private string? _dataDirectory;

    private StorageService? _storageService;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new (configurationRootMock.Object);
        _storageService = new (_userConfigurationService);
    }

    [Test]
    public void GetSubDirectories_ValidDirectory_ReturnsListOfSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "TestFolder");

        List<DirectoryInfo> subDirectories = _storageService!.GetSubDirectories(directoryPath);

        Assert.That(subDirectories, Is.Not.Null);
        Assert.That(subDirectories, Has.Exactly(3).Items);
        Assert.That(subDirectories.Any(dir => dir.Name == "TestHiddenSubFolder"), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == "TestSubFolder1"), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == "TestSubFolder2"), Is.True);
        Assert.That(subDirectories[0].Name, Is.EqualTo("TestHiddenSubFolder"));
        Assert.That(subDirectories[1].Name, Is.EqualTo("TestSubFolder1"));
        Assert.That(subDirectories[2].Name, Is.EqualTo("TestSubFolder2"));
    }

    [Test]
    public void GetSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "NonExistentDirectory");

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsListOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "TestFolder");

        List<DirectoryInfo> recursiveSubDirectories = _storageService!.GetRecursiveSubDirectories(directoryPath);

        Assert.That(recursiveSubDirectories, Has.Count.EqualTo(4));
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == "TestHiddenSubFolder"), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder1"), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder2"), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder3"), Is.True);
        Assert.That(recursiveSubDirectories[0].FullName, Does.EndWith("\\TestHiddenSubFolder"));
        Assert.That(recursiveSubDirectories[1].FullName, Does.EndWith("\\TestSubFolder1"));
        Assert.That(recursiveSubDirectories[2].FullName, Does.EndWith("\\TestSubFolder2"));
        Assert.That(recursiveSubDirectories[3].FullName, Does.EndWith("\\TestSubFolder2\\TestSubFolder3"));
    }

    [Test]
    public void GetRecursiveSubDirectories_InvalidDirectory_ReturnsEmptyList()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "NonExistentDirectory");

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetRecursiveSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(_dataDirectory!, "TestDirectory");

        _storageService!.CreateDirectory(testDirectory);

        Assert.That(Directory.Exists(testDirectory), Is.True);

        Directory.Delete(testDirectory);
    }

    [Test]
    public void DeleteFile_FileExists_DeletesFile()
    {
        const string fileName = "Image 1.jpg";
        const string newFileName = "ImageToDelete.jpg";
        string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
        string destinationFilePath = Path.Combine(_dataDirectory!, newFileName);

        File.Copy(sourceFilePath, destinationFilePath);

        Assert.That(File.Exists(destinationFilePath), Is.True);

        _storageService!.DeleteFile(_dataDirectory!, newFileName);

        Assert.That(File.Exists(destinationFilePath), Is.False);
    }

    [Test]
    public void DeleteFile_FileDoesNotExist_NoActionTaken()
    {
        const string testFileName = "NonExistentImage.jpg";
        string testFilePath = Path.Combine(_dataDirectory!, testFileName);

        Assert.That(File.Exists(testFilePath), Is.False);

        Assert.DoesNotThrow(() => _storageService!.DeleteFile(_dataDirectory!, testFileName));
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _storageService!.GetFileNames(_dataDirectory!);

        Assert.That(fileNames, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(fileNames, Does.Contain("Image 2.jpg"));
        Assert.That(fileNames, Does.Contain("Image 1.jpg"));
    }

    [Test]
    public void GetFileBytes_FileExists_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");

        byte[] actualBytes = _storageService!.GetFileBytes(testFilePath);

        Assert.That(actualBytes, Is.Not.Null);
        Assert.That(actualBytes, Is.Not.Empty);
    }

    [Test]
    public void GetFileBytes_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(_dataDirectory!, "NonExistentFile.jpg");

        FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _storageService!.GetFileBytes(nonExistentFilePath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find file '{nonExistentFilePath}'."));
    }

    [Test]
    public void GetFileBytes_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() => _storageService!.GetFileBytes(_dataDirectory!));
        Assert.That(exception?.Message, Is.EqualTo($"Access to the path '{_dataDirectory!}' is denied."));
    }

    [Test]
    [TestCase("Image 1.jpg", 1)]
    [TestCase("Image 1_90_deg.jpg", 6)]
    [TestCase("Image 1_180_deg.jpg", 3)]
    [TestCase("Image 1_270_deg.jpg", 8)]
    [TestCase("Image 8.jpeg", 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _storageService!.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    [TestCase("Image 10 portrait.png")] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Homer.gif")] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Image_11.heic")] // Error on BitmapFrame.Create(stream)
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsCorruptedImageOrientation(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _storageService!.GetExifOrientation(
            buffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _storageService!.GetExifOrientation(
            invalidImageBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = _storageService!.GetExifOrientation(
            nullBuffer!,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = [];

        ushort orientation = _storageService!.GetExifOrientation(
            emptyBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetExifOrientation_InvalidFormat_ReturnsCorruptedOrientationValue()
    {
        Bitmap image = new (10, 10);

        using (MemoryStream ms = new())
        {
            image.Save(ms, ImageFormat.Bmp); // Save as BMP to create an invalid format for JPEG
            byte[] buffer = ms.ToArray(); // Buffer with invalid Exif Metadata (Metadata null)

            ushort orientation = _storageService!.GetExifOrientation(
                buffer,
                _userConfigurationService!.AssetSettings.DefaultExifOrientation,
                _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

            Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
        }
    }

    [Test]
    [TestCase("Image_11.heic", 1)]
    [TestCase("Image_11_90.heic", 6)]
    [TestCase("Image_11_180.heic", 3)]
    [TestCase("Image_11_270.heic", 8)]
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOrientation)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _storageService!.GetHeicExifOrientation(buffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(expectedOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _storageService!.GetHeicExifOrientation(invalidHeicBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.That(orientation, Is.EqualTo(_userConfigurationService!.AssetSettings.CorruptedImageOrientation));
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _storageService!.GetHeicExifOrientation(nullBuffer!, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            _storageService!.GetHeicExifOrientation(emptyBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.jpeg");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBitmapImage_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.jpeg");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetJpegBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _storageService!.GetJpegBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetJpegBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.png");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBitmapImage_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.png");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetPngBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _storageService!.GetPngBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetPngBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.gif");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBitmapImage_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.gif");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }
    }

    [Test]
    public void GetGifBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _storageService!.GetGifBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetGifBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Hash = string.Empty
        };

        bool exists = _storageService!.FileExists(folder, asset);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = "NonExistent.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Hash = string.Empty
        };

        bool exists = _storageService!.FileExists(folder, asset);

        Assert.That(exists, Is.False);
    }

    [Test]
    [TestCase("toto", null!, "path2")]
    [TestCase(null!, "Image 1.jpg", "path1")]
    public void FileExists_NullFileNameOrNullPath_ThrowsArgumentNullException(string path, string fileName, string exceptionParameter)
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = path };
        Asset asset = new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = fileName,
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Hash = string.Empty
        };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.FileExists(folder, asset));

        Assert.That(exception?.Message, Is.EqualTo($"Value cannot be null. (Parameter '{exceptionParameter}')"));
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(_dataDirectory!, "Image 1.jpg");

        bool exists = _storageService!.FileExists(fullPath);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FileExistsFullPath_FileDoesNotExist_ReturnsFalse()
    {
        string fullPath = Path.Combine(_dataDirectory!, "NonExistent.jpg");

        bool exists = _storageService!.FileExists(fullPath);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FileExistsFullPath_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _storageService!.FileExists(fullPath!);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FolderExists_ExistingFolder_ReturnsTrue()
    {
        bool exists = _storageService!.FolderExists(_dataDirectory!);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FolderExists_NonExistentFolder_ReturnsFalse()
    {
        bool exists = _storageService!.FolderExists("toto");

        Assert.That(exists, Is.False);
    }

    [Test]
    public void FolderExists_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _storageService!.FolderExists(fullPath!);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void UpdateAssetsFileProperties_SomeFilesExist_PopulatesAssetsFileProperties()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = "Homer.gif";
            const string fileName2 = "Image 1.jpg";
            const string fileName3 = "Image 9.png";
            const string fileName4 = "Image_11.heic";
            const string fileName5 = "nonexistent.jpg";

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName3,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            _storageService!.UpdateAssetsFileProperties([asset1, asset2, asset3, asset4, asset5]);

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(64123));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(29857));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(126277));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(1411940));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileProperties_AssetsIsEmpty_DoesNothing()
    {
        _storageService!.UpdateAssetsFileProperties([]);
    }

    [Test]
    public void UpdateAssetsFileProperties_AssetsIsNull_ThrowsNullReferenceException()
    {
        List<Asset>? assets = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetsFileProperties(assets!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void UpdateAssetsFileProperties_FilePathIsNull_ThrowsArgumentNullException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = "Homer.gif";
            const string fileName2 = "Image 1.jpg";
            const string fileName3 = "Image 9.png";
            const string fileName4 = "Image_11.heic";
            const string fileName5 = "nonexistent.jpg";

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            string? path = null;
            Folder folder = new() { Id = Guid.NewGuid(), Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset3 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName3,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.UpdateAssetsFileProperties([asset1, asset2, asset3, asset4, asset5]));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileProperties_OneAssetIsNull_ThrowsNullReferenceException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = "Homer.gif";
            const string fileName2 = "Image 1.jpg";
            const string fileName3 = "Image 9.png";
            const string fileName4 = "Image_11.heic";
            const string fileName5 = "nonexistent.jpg";

            string sourceFilePath1 = Path.Combine(_dataDirectory!, fileName1);
            string destinationFilePath1 = Path.Combine(destinationPath, fileName1);
            string sourceFilePath2 = Path.Combine(_dataDirectory!, fileName2);
            string destinationFilePath2 = Path.Combine(destinationPath, fileName2);
            string sourceFilePath3 = Path.Combine(_dataDirectory!, fileName3);
            string destinationFilePath3 = Path.Combine(destinationPath, fileName3);
            string sourceFilePath4 = Path.Combine(_dataDirectory!, fileName4);
            string destinationFilePath4 = Path.Combine(destinationPath, fileName4);

            File.Copy(sourceFilePath1, destinationFilePath1);
            File.Copy(sourceFilePath2, destinationFilePath2);
            File.Copy(sourceFilePath3, destinationFilePath3);
            File.Copy(sourceFilePath4, destinationFilePath4);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName1,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset2 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName2,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset? asset3 = null;
            Asset asset4 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName4,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };
            Asset asset5 = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName5,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetsFileProperties([asset1, asset2, asset3!, asset4, asset5]));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(64123));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(29857));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset5.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset5.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FileExists_PopulatesAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "Image 1.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            _storageService!.UpdateAssetFileProperties(asset);

            Assert.That(asset.FileProperties.Size, Is.EqualTo(29857));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FileDoesNotExist_DoesNotPopulateAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "nonexistent.jpg";

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };
            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            DateTime creationTime = default;
            DateTime modificationTime = default;

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            _storageService!.UpdateAssetFileProperties(asset);

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_FilePathIsNull_ThrowsArgumentNullException()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "Image 1.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            string? path = null;
            Folder folder = new() { Id = Guid.NewGuid(), Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty
            };

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.UpdateAssetFileProperties(asset));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(modificationTime.Date));
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileProperties_AssetIsNull_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetFileProperties(asset!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    [TestCase("Image 1.jpg")]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 10 portrait.png")]
    [TestCase("Homer.gif")]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidGDIPlusImage(validImageData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidGDIPlusImage(invalidImageData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = _storageService!.IsValidGDIPlusImage(emptyHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidHeic(validHeicData);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = _storageService!.IsValidHeic(invalidHeicData);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.IsValidHeic(emptyHeicData));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    public void GetTotalFilesCount_FilesInDirectory_ReturnsTotalFilesCount()
    {
        int totalFilesCount = _storageService!.GetTotalFilesCount();
        Assert.That(totalFilesCount, Is.EqualTo(67));
    }

    [Test]
    public void GetTotalFilesCount_EmptyDirectory_ReturnsTotalFilesCount()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            Mock<IConfigurationRoot> configurationRootMock = new();
            configurationRootMock.GetDefaultMockConfig();
            configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);

            UserConfigurationService userConfigurationService = new (configurationRootMock.Object);
            StorageService storageService = new (userConfigurationService);

            int totalFilesCount = storageService.GetTotalFilesCount();
            Assert.That(totalFilesCount, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }

    private static bool IsValidImage(string filePath)
    {
        try
        {
            using (Image.FromFile(filePath))
            {
                // The image is successfully loaded; consider it valid
                return true;
            }
        }
        catch (Exception)
        {
            // An exception occurred while loading the image; consider it invalid
            return false;
        }
    }
}
