using System.Drawing.Imaging;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;

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
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new (configurationRootMock.Object);
        _storageService = new (_userConfigurationService);
    }

    [Test]
    public void GetSubDirectories_ValidDirectory_ReturnsListOfSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        List<DirectoryInfo> subDirectories = _storageService!.GetSubDirectories(directoryPath);

        Assert.That(subDirectories, Is.Not.Null);
        Assert.That(subDirectories, Has.Exactly(3).Items);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(subDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(subDirectories[0].Name, Is.EqualTo(Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(subDirectories[1].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_1));
        Assert.That(subDirectories[2].Name, Is.EqualTo(Directories.TEST_SUB_FOLDER_2));
    }

    [Test]
    public void GetSubDirectories_InvalidDirectory_ThrowsDirectoryNotFoundException()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsListOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        List<DirectoryInfo> recursiveSubDirectories = _storageService!.GetRecursiveSubDirectories(directoryPath);

        Assert.That(recursiveSubDirectories, Has.Count.EqualTo(4));
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_HIDDEN_SUB_FOLDER), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_1), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_2), Is.True);
        Assert.That(recursiveSubDirectories.Any(dir => dir.Name == Directories.TEST_SUB_FOLDER_3), Is.True);
        Assert.That(recursiveSubDirectories[0].FullName, Does.EndWith("\\" + Directories.TEST_HIDDEN_SUB_FOLDER));
        Assert.That(recursiveSubDirectories[1].FullName, Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_1));
        Assert.That(recursiveSubDirectories[2].FullName, Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_2));
        Assert.That(recursiveSubDirectories[3].FullName, Does.EndWith("\\" + Directories.TEST_SUB_FOLDER_2 + "\\" + Directories.TEST_SUB_FOLDER_3));
    }

    [Test]
    public void GetRecursiveSubDirectories_InvalidDirectory_ReturnsEmptyList()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetRecursiveSubDirectories(directoryPath));

        Assert.That(exception?.Message, Is.EqualTo($"Could not find a part of the path '{directoryPath}'."));
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(_dataDirectory!, Directories.TEST_DIRECTORY);

        _storageService!.CreateDirectory(testDirectory);

        Assert.That(Directory.Exists(testDirectory), Is.True);

        Directory.Delete(testDirectory);
    }

    [Test]
    public void DeleteFile_FileExists_DeletesFile()
    {
        const string fileName = FileNames.IMAGE_1_JPG;
        const string newFileName = FileNames.IMAGE_TO_DELETE_JPG;
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
        const string testFileName = FileNames.NON_EXISTENT_IMAGE_JPG;
        string testFilePath = Path.Combine(_dataDirectory!, testFileName);

        Assert.That(File.Exists(testFilePath), Is.False);

        Assert.DoesNotThrow(() => _storageService!.DeleteFile(_dataDirectory!, testFileName));
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _storageService!.GetFileNames(_dataDirectory!);

        Assert.That(fileNames, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_2_JPG));
        Assert.That(fileNames, Does.Contain(FileNames.IMAGE_1_JPG));
    }

    [Test]
    public void GetFileBytes_FileExists_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        byte[] actualBytes = _storageService!.GetFileBytes(testFilePath);

        Assert.That(actualBytes, Is.Not.Null);
        Assert.That(actualBytes, Is.Not.Empty);
    }

    [Test]
    public void GetFileBytes_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_FILE_JPG);

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
    [TestCase(FileNames.IMAGE_1_JPG, 1)]
    [TestCase(FileNames.IMAGE_1_90_DEG_JPG, 6)]
    [TestCase(FileNames.IMAGE_1_180_DEG_JPG, 3)]
    [TestCase(FileNames.IMAGE_1_270_DEG_JPG, 8)]
    [TestCase(FileNames.IMAGE_8_JPEG, 1)]
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
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase(FileNames.HOMER_GIF)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase(FileNames.IMAGE_11_HEIC)] // Error on BitmapFrame.Create(stream)
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
    [TestCase(FileNames.IMAGE_11_HEIC, 1)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, 6)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, 3)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, 8)]
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
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
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
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
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
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
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
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
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
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
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
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(_storageService.IsValidGDIPlusImage(imageBuffer), Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
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
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
            FileName = FileNames.NON_EXISTENT_IMAGE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
            },
            Hash = string.Empty
        };

        bool exists = _storageService!.FileExists(folder, asset);

        Assert.That(exists, Is.False);
    }

    [Test]
    [TestCase("toto", null!, "path2")]
    [TestCase(null!, FileNames.IMAGE_1_JPG, "path1")]
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
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty
        };

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.FileExists(folder, asset));

        Assert.That(exception?.Message, Is.EqualTo($"Value cannot be null. (Parameter '{exceptionParameter}')"));
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        bool exists = _storageService!.FileExists(fullPath);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void FileExistsFullPath_FileDoesNotExist_ReturnsFalse()
    {
        string fullPath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);

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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

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
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
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
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
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

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(FileSize.HOMER_GIF));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
            Assert.That(asset2.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset2.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset3.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_9_PNG));
            Assert.That(asset3.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset3.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset4.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_11_HEIC));
            Assert.That(asset4.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset4.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset5.FileProperties.Size, Is.EqualTo(FileSize.NON_EXISTENT_IMAGE_JPG));
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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

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
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
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
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName1 = FileNames.HOMER_GIF;
            const string fileName2 = FileNames.IMAGE_1_JPG;
            const string fileName3 = FileNames.IMAGE_9_PNG;
            const string fileName4 = FileNames.IMAGE_11_HEIC;
            const string fileName5 = FileNames.NON_EXISTENT_IMAGE_JPG;

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
                    Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
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
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
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

            Assert.That(asset1.FileProperties.Size, Is.EqualTo(FileSize.HOMER_GIF));
            Assert.That(asset1.FileProperties.Creation.Date, Is.EqualTo(creationTime.Date));
            Assert.That(asset1.FileProperties.Modification.Date, Is.EqualTo(oldDateTime.Date));
            Assert.That(asset2.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.IMAGE_1_JPG;

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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
                },
                Hash = string.Empty
            };

            Assert.That(asset.FileProperties.Size, Is.EqualTo(0));
            Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(DateTime.MinValue));
            Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(DateTime.MinValue));

            _storageService!.UpdateAssetFileProperties(asset);

            Assert.That(asset.FileProperties.Size, Is.EqualTo(FileSize.IMAGE_1_JPG));
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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.NON_EXISTENT_IMAGE_JPG;

            Folder folder = new() { Id = Guid.NewGuid(), Path = destinationPath };
            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
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
        string destinationPath = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        try
        {
            Directory.CreateDirectory(destinationPath);

            const string fileName = FileNames.IMAGE_1_JPG;

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
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
    [TestCase(FileNames.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF)]
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
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
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
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

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
