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

        Assert.IsNotNull(subDirectories);
        Assert.That(subDirectories, Has.Exactly(3).Items);
        Assert.IsTrue(subDirectories.Any(dir => dir.Name == "TestHiddenSubFolder"));
        Assert.IsTrue(subDirectories.Any(dir => dir.Name == "TestSubFolder1"));
        Assert.IsTrue(subDirectories.Any(dir => dir.Name == "TestSubFolder2"));
        Assert.AreEqual("TestHiddenSubFolder", subDirectories[0].Name);
        Assert.AreEqual("TestSubFolder1", subDirectories[1].Name);
        Assert.AreEqual("TestSubFolder2", subDirectories[2].Name);
    }

    [Test]
    public void GetSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "NonExistentDirectory");

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetSubDirectories(directoryPath));

        Assert.AreEqual($"Could not find a part of the path '{directoryPath}'.", exception?.Message);
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsListOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "TestFolder");

        List<DirectoryInfo> recursiveSubDirectories = _storageService!.GetRecursiveSubDirectories(directoryPath);

        Assert.AreEqual(4, recursiveSubDirectories.Count);
        Assert.IsTrue(recursiveSubDirectories.Any(dir => dir.Name == "TestHiddenSubFolder"));
        Assert.IsTrue(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder1"));
        Assert.IsTrue(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder2"));
        Assert.IsTrue(recursiveSubDirectories.Any(dir => dir.Name == "TestSubFolder3"));
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

        Assert.AreEqual($"Could not find a part of the path '{directoryPath}'.", exception?.Message);
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(_dataDirectory!, "TestDirectory");

        _storageService!.CreateDirectory(testDirectory);

        Assert.IsTrue(Directory.Exists(testDirectory));

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

        Assert.IsTrue(File.Exists(destinationFilePath));

        _storageService!.DeleteFile(_dataDirectory!, newFileName);

        Assert.IsFalse(File.Exists(destinationFilePath));
    }

    [Test]
    public void DeleteFile_FileDoesNotExist_NoActionTaken()
    {
        const string testFileName = "NonExistentImage.jpg";
        string testFilePath = Path.Combine(_dataDirectory!, testFileName);

        Assert.IsFalse(File.Exists(testFilePath));

        Assert.DoesNotThrow(() => _storageService!.DeleteFile(_dataDirectory!, testFileName));
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _storageService!.GetFileNames(_dataDirectory!);

        Assert.GreaterOrEqual(fileNames.Length, 2);
        Assert.Contains("Image 2.jpg", fileNames);
        Assert.Contains("Image 1.jpg", fileNames);
    }

    [Test]
    public void GetFileBytes_FileExists_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(_dataDirectory!, "Image 1.jpg");

        byte[] actualBytes = _storageService!.GetFileBytes(testFilePath);

        Assert.IsNotNull(actualBytes);
        Assert.Greater(actualBytes.Length, 0);
    }

    [Test]
    public void GetFileBytes_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(_dataDirectory!, "NonExistentFile.jpg");

        FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => _storageService!.GetFileBytes(nonExistentFilePath));

        Assert.AreEqual($"Could not find file '{nonExistentFilePath}'.", exception?.Message);
    }

    [Test]
    public void GetFileBytes_FilePathIsInvalid_ThrowsUnauthorizedAccessException()
    {
        UnauthorizedAccessException? exception = Assert.Throws<UnauthorizedAccessException>(() => _storageService!.GetFileBytes(_dataDirectory!));
        Assert.AreEqual($"Access to the path '{_dataDirectory!}' is denied.", exception?.Message);
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

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOrientation, orientation);
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

        Assert.IsNotNull(orientation);
        Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _storageService!.GetExifOrientation(
            invalidImageBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = _storageService!.GetExifOrientation(
            nullBuffer!,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = [];

        ushort orientation = _storageService!.GetExifOrientation(
            emptyBuffer,
            _userConfigurationService!.AssetSettings.DefaultExifOrientation,
            _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
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

            Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
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

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOrientation, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = [0x00, 0x01, 0x02, 0x03];

        ushort orientation = _storageService!.GetHeicExifOrientation(invalidHeicBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);

        Assert.AreEqual(_userConfigurationService!.AssetSettings.CorruptedImageOrientation, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _storageService!.GetHeicExifOrientation(nullBuffer!, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });
        
        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
        {
            _storageService!.GetHeicExifOrientation(emptyBuffer, _userConfigurationService!.AssetSettings.CorruptedImageOrientation);
        });

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.jpeg");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.jpeg");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetJpegBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.png");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.png");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetPngBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.gif");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(_storageService.IsValidGDIPlusImage(imageBuffer));
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, "image_converted.gif");
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.IsTrue(IsValidImage(destinationNewFilePath));
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

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.GetGifBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        Folder folder = new() { Path = _dataDirectory! };
        Asset asset = new() { FileName = "Image 1.jpg" };

        bool exists = _storageService!.FileExists(folder, asset);

        Assert.IsTrue(exists);
    }

    [Test]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        Folder folder = new() { Path = _dataDirectory! };
        Asset asset = new() { FileName = "NonExistent.jpg" };

        bool exists = _storageService!.FileExists(folder, asset);

        Assert.IsFalse(exists);
    }

    [Test]
    [TestCase("toto", null, "path2")]
    [TestCase(null, "Image 1.jpg", "path1")]
    public void FileExists_NullFileNameOrNullPath_ThrowsArgumentNullException(string path, string fileName, string exceptionParameter)
    {
        Folder folder = new() { Path = path };
        Asset asset = new() { FileName = fileName };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.FileExists(folder, asset));

        Assert.AreEqual($"Value cannot be null. (Parameter '{exceptionParameter}')", exception?.Message);
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(_dataDirectory!, "Image 1.jpg");

        bool exists = _storageService!.FileExists(fullPath);

        Assert.IsTrue(exists);
    }

    [Test]
    public void FileExistsFullPath_FileDoesNotExist_ReturnsFalse()
    {
        string fullPath = Path.Combine(_dataDirectory!, "NonExistent.jpg");

        bool exists = _storageService!.FileExists(fullPath);

        Assert.IsFalse(exists);
    }

    [Test]
    public void FileExistsFullPath_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _storageService!.FileExists(fullPath!);

        Assert.IsFalse(exists);
    }

    [Test]
    public void FolderExists_ExistingFolder_ReturnsTrue()
    {
        bool exists = _storageService!.FolderExists(_dataDirectory!);

        Assert.IsTrue(exists);
    }

    [Test]
    public void FolderExists_NonExistentFolder_ReturnsFalse()
    {
        bool exists = _storageService!.FolderExists("toto");

        Assert.IsFalse(exists);
    }

    [Test]
    public void FolderExists_NullPath_ReturnsFalse()
    {
        string? fullPath = null;

        bool exists = _storageService!.FolderExists(fullPath!);

        Assert.IsFalse(exists);
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_SomeFilesExist_PopulatesAssetsDates()
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

            Folder folder = new() { Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new() { Folder = folder, FileName = fileName1 };
            Asset asset2 = new() { Folder = folder, FileName = fileName2 };
            Asset asset3 = new() { Folder = folder, FileName = fileName3 };
            Asset asset4 = new() { Folder = folder, FileName = fileName4 };
            Asset asset5 = new() { Folder = folder, FileName = fileName5 };

            Assert.AreEqual(DateTime.MinValue, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileModificationDateTime.Date);

            _storageService!.UpdateAssetsFileDateTimeProperties([asset1, asset2, asset3, asset4, asset5]);

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_AssetsIsEmpty_DoesNothing()
    {
        _storageService!.UpdateAssetsFileDateTimeProperties([]);
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_AssetsIsNull_ThrowsNullReferenceException()
    {
        List<Asset>? assets = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetsFileDateTimeProperties(assets!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_FolderIsNull_DoesNotPopulateAssetsDates()
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

            Folder? folder = null;

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new() { Folder = folder!, FileName = fileName1 };
            Asset asset2 = new() { Folder = folder!, FileName = fileName2 };
            Asset asset3 = new() { Folder = folder!, FileName = fileName3 };
            Asset asset4 = new() { Folder = folder!, FileName = fileName4 };
            Asset asset5 = new() { Folder = folder!, FileName = fileName5 };

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset5.FileModificationDateTime.Date);

            _storageService!.UpdateAssetsFileDateTimeProperties([asset1, asset2, asset3, asset4, asset5]);

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset5.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_FilePathIsNull_ThrowsArgumentNullException()
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
            Folder folder = new() { Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new() { Folder = folder, FileName = fileName1 };
            Asset asset2 = new() { Folder = folder, FileName = fileName2 };
            Asset asset3 = new() { Folder = folder, FileName = fileName3 };
            Asset asset4 = new() { Folder = folder, FileName = fileName4 };
            Asset asset5 = new() { Folder = folder, FileName = fileName5 };

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset5.FileModificationDateTime.Date);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.UpdateAssetsFileDateTimeProperties([asset1, asset2, asset3, asset4, asset5]));

            Assert.AreEqual("Value cannot be null. (Parameter 'path1')", exception?.Message);

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset3.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset3.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset5.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetsFileDateTimeProperties_OneAssetIsNull_ThrowsNullReferenceException()
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

            Folder folder = new() { Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath1, oldDateTime);
            File.SetLastWriteTime(destinationFilePath2, oldDateTime);
            File.SetLastWriteTime(destinationFilePath3, oldDateTime);
            File.SetLastWriteTime(destinationFilePath4, oldDateTime);

            Asset asset1 = new() { Folder = folder, FileName = fileName1 };
            Asset asset2 = new() { Folder = folder, FileName = fileName2 };
            Asset? asset3 = null;
            Asset asset4 = new() { Folder = folder, FileName = fileName4 };
            Asset asset5 = new() { Folder = folder, FileName = fileName5 };

            Assert.AreEqual(DateTime.MinValue, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileModificationDateTime.Date);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetsFileDateTimeProperties([asset1, asset2, asset3!, asset4, asset5]));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);

            Assert.AreEqual(creationTime.Date, asset1.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset1.FileModificationDateTime.Date);
            Assert.AreEqual(creationTime.Date, asset2.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset2.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset4.FileModificationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset5.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileDateTimeProperties_FileExists_PopulatesAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "Image 1.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            Folder folder = new() { Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new() { Folder = folder, FileName = fileName };

            Assert.AreEqual(DateTime.MinValue, asset.FileCreationDateTime.Date);
            Assert.AreEqual(DateTime.MinValue, asset.FileModificationDateTime.Date);

            _storageService!.UpdateAssetFileDateTimeProperties(asset);

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileDateTimeProperties_FileDoesNotExist_DoesNotPopulateAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "nonexistent.jpg";

            Folder folder = new() { Path = destinationPath };
            Asset asset = new() { Folder = folder, FileName = fileName };

            DateTime creationTime = default;
            DateTime modificationTime = default;

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);

            _storageService!.UpdateAssetFileDateTimeProperties(asset);

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileDateTimeProperties_FolderIsNull_DoesNotPopulateAssetDates()
    {
        string destinationPath = Path.Combine(_dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = "Image 1.jpg";

            string sourceFilePath = Path.Combine(_dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            Folder? folder = null;

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new() { Folder = folder!, FileName = fileName };

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);

            _storageService!.UpdateAssetFileDateTimeProperties(asset);

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileDateTimeProperties_FilePathIsNull_ThrowsArgumentNullException()
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
            Folder folder = new() { Path = path! };

            DateTime creationTime = default;
            DateTime modificationTime = default;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new() { Folder = folder, FileName = fileName };

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.UpdateAssetFileDateTimeProperties(asset));

            Assert.AreEqual("Value cannot be null. (Parameter 'path1')", exception?.Message);

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void UpdateAssetFileDateTimeProperties_AssetIsNull_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _storageService!.UpdateAssetFileDateTimeProperties(asset!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
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

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidGDIPlusImage(invalidImageData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = [];

        bool result = _storageService!.IsValidGDIPlusImage(emptyHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidHeic(validHeicData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = [0x00, 0x01, 0x02, 0x03];

        bool result = _storageService!.IsValidHeic(invalidHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = [];

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.IsValidHeic(emptyHeicData));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    public void GetTotalFilesCount_FilesInDirectory_ReturnsTotalFilesCount()
    {
        int totalFilesCount = _storageService!.GetTotalFilesCount();
        Assert.AreEqual(67, totalFilesCount);
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
            Assert.AreEqual(0, totalFilesCount);
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
