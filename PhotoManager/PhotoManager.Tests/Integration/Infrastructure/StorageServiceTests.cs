using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class StorageServiceTests
{
    private string? dataDirectory;
    private IStorageService? _storageService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock.MockGetValue("appsettings:CatalogBatchSize", "100");

        _storageService = new StorageService(new UserConfigurationService(configurationMock.Object));
    }

    [Test]
    public void GetSubDirectories_ValidDirectory_ReturnsListOfSubDirectories()
    {
        string directoryPath = Path.Combine(dataDirectory!, "TestFolder");

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
        string directoryPath = Path.Combine(dataDirectory!, "NonExistentDirectory");

        Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetSubDirectories(directoryPath));
    }

    [Test]
    public void GetRecursiveSubDirectories_ValidDirectory_ReturnsListOfRecursiveSubDirectories()
    {
        string directoryPath = Path.Combine(dataDirectory!, "TestFolder");

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
        string directoryPath = Path.Combine(dataDirectory!, "NonExistentDirectory");

        Assert.Throws<DirectoryNotFoundException>(() => _storageService!.GetRecursiveSubDirectories(directoryPath));
    }

    [Test]
    public void CreateDirectory_ValidDirectory_CreatesDirectory()
    {
        string testDirectory = Path.Combine(dataDirectory!, "TestDirectory");

        _storageService!.CreateDirectory(testDirectory);

        Assert.IsTrue(Directory.Exists(testDirectory));

        Directory.Delete(testDirectory);
    }

    [Test]
    public void DeleteFile_FileExists_DeletesFile()
    {
        string testFileName = "TestFile.txt";
        string testFilePath = Path.Combine(dataDirectory!, testFileName);
        File.WriteAllText(testFilePath, "Test content");

        _storageService!.DeleteFile(dataDirectory!, testFileName);

        Assert.IsFalse(File.Exists(testFilePath));
    }

    [Test]
    public void DeleteFile_FileDoesNotExist_NoActionTaken()
    {
        string testFileName = "NonExistentFile.txt";
        string testFilePath = Path.Combine(dataDirectory!, testFileName);

        Assert.DoesNotThrow(() => _storageService!.DeleteFile(dataDirectory!, testFileName));
    }

    [Test]
    public void GetFileNames_ReturnsFileNames()
    {
        string[] fileNames = _storageService!.GetFileNames(dataDirectory!);

        Assert.GreaterOrEqual(fileNames.Length, 2);
        Assert.Contains("Image 2.jpg", fileNames);
        Assert.Contains("Image 1.jpg", fileNames);
    }

    [Test]
    public void GetFileBytes_ExistentFile_ReturnsFileBytes()
    {
        string testFilePath = Path.Combine(dataDirectory!, "Image 1.jpg");

        byte[] actualBytes = _storageService!.GetFileBytes(testFilePath);

        Assert.IsNotNull(actualBytes);
        Assert.Greater(actualBytes.Length, 0);
    }

    [Test]
    public void GetFileBytes_NonExistentFile_ThrowsFileNotFoundException()
    {
        string nonExistentFilePath = Path.Combine(dataDirectory!, "NonExistentFile.txt");

        Assert.Throws<FileNotFoundException>(() => _storageService!.GetFileBytes(nonExistentFilePath));
    }

    [Test]
    [TestCase("Image 1.jpg", 1)]
    [TestCase("Image 1_90_deg.jpg", 6)]
    [TestCase("Image 1_180_deg.jpg", 3)]
    [TestCase("Image 1_270_deg.jpg", 8)]
    [TestCase("Image 8.jpeg", 1)]
    public void GetExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _storageService!.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    [TestCase("Image 10 portrait.png", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Homer.gif", AssetConstants.OrientationCorruptedImage)] // Error on bitmapMetadata.GetQuery("System.Photo.Orientation")
    [TestCase("Image_11.heic", AssetConstants.OrientationCorruptedImage)] // Error on BitmapFrame.Create(stream)
    public void GetExifOrientation_FormatImageNotHandledBuffer_ReturnsOrientationCorruptedImage(string fileName, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        ushort orientation = _storageService!.GetExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    public void GetExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidImageBuffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        ushort orientation = _storageService!.GetExifOrientation(invalidImageBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetExifOrientation_NullBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[]? nullBuffer = null;

        ushort orientation = _storageService!.GetExifOrientation(nullBuffer!);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetExifOrientation_EmptyBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] emptyBuffer = Array.Empty<byte>();

        ushort orientation = _storageService!.GetExifOrientation(emptyBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    [TestCase("Image_11.heic", 0, 1)]
    //[TestCase("Image_11_90.heic", 90, 8)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    //[TestCase("Image_11_180.heic", 180, 3)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    //[TestCase("Image_11_270.heic", 270, 6)] // MagickImage always returns "TopLeft" it is not able to detect the right orientation for a heic file -_-
    public void GetHeicExifOrientation_ValidImageBuffer_ReturnsOrientationValue(string fileName, int degrees, int expectedOriention)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);
        //byte[] bufferRotated = GetHeicRotatedBuffer(buffer, degrees);

        ushort orientation = _storageService!.GetHeicExifOrientation(buffer);

        Assert.IsNotNull(orientation);
        Assert.AreEqual(expectedOriention, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_InvalidImageBuffer_ReturnsCorruptedOrientationValue()
    {
        byte[] invalidHeicBuffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        ushort orientation = _storageService!.GetHeicExifOrientation(invalidHeicBuffer);

        Assert.AreEqual(AssetConstants.OrientationCorruptedImage, orientation);
    }

    [Test]
    public void GetHeicExifOrientation_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? nullBuffer = null;

        Assert.Throws<ArgumentNullException>(() => _storageService!.GetHeicExifOrientation(nullBuffer!));
    }

    [Test]
    public void GetHeicExifOrientation_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] emptyBuffer = Array.Empty<byte>();

        Assert.Throws<ArgumentException>(() => _storageService!.GetHeicExifOrientation(emptyBuffer));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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

        Assert.Throws<InvalidOperationException>(() => _storageService!.GetJpegBitmapImage(image));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => _storageService!.GetJpegBitmapImage(invalidImage!));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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

        Assert.Throws<InvalidOperationException>(() => _storageService!.GetPngBitmapImage(image));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => _storageService!.GetPngBitmapImage(invalidImage!));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new(new Uri(filePath));

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = _storageService!.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(dataDirectory!, "ImageConverted");

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

        Assert.Throws<InvalidOperationException>(() => _storageService!.GetGifBitmapImage(image));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => _storageService!.GetGifBitmapImage(invalidImage!));
    }

    [Test]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        Asset asset = new() { FileName = "Image 1.jpg" };
        Folder folder = new() { Path = dataDirectory! };

        bool exists = _storageService!.FileExists(asset, folder);

        Assert.IsTrue(exists);
    }

    [Test]
    public void FileExists_NonExistentFile_ReturnsFalse()
    {
        Asset asset = new() { FileName = "nonexistent.txt" };
        Folder folder = new() { Path = dataDirectory! };

        bool exists = _storageService!.FileExists(asset, folder);

        Assert.IsFalse(exists);
    }

    [Test]
    [TestCase("Image 1.jpg", null)]
    [TestCase(null, "toto")]
    public void FileExists_NullFileNameOrNullPath_ThrowsArgumentNullException(string fileName, string path)
    {
        Asset asset = new() { FileName = fileName };
        Folder folder = new() { Path = path };

        Assert.Throws<ArgumentNullException>(() => _storageService!.FileExists(asset, folder));
    }

    [Test]
    public void FileExistsFullPath_ExistingFile_ReturnsTrue()
    {
        string fullPath = Path.Combine(dataDirectory!, "Image 1.jpg");

        bool exists = _storageService!.FileExists(fullPath);

        Assert.IsTrue(exists);
    }

    [Test]
    public void FileExistsFullPath_NonExistentFile_ReturnsFalse()
    {
        string fullPath = Path.Combine(dataDirectory!, "nonexistent.txt");

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
        bool exists = _storageService!.FolderExists(dataDirectory!);

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
    public void LoadFileInformation_FileExists_PopulatesAssetDates()
    {
        string destinationPath = Path.Combine(dataDirectory!, "DestinationToCopy");

        try
        {
            Directory.CreateDirectory(destinationPath);

            string fileName = "Image 1.jpg";

            string sourceFilePath = Path.Combine(dataDirectory!, fileName);
            string destinationFilePath = Path.Combine(destinationPath, fileName);

            File.Copy(sourceFilePath, destinationFilePath);

            Folder folder = new() { Path = destinationPath };

            DateTime creationTime = DateTime.Now;
            DateTime oldDateTime = DateTime.Now.AddDays(-1);

            File.SetLastWriteTime(destinationFilePath, oldDateTime);

            Asset asset = new() { Folder = folder, FileName = fileName };

            _storageService!.LoadFileInformation(asset);

            Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
            Assert.AreEqual(oldDateTime.Date, asset.FileModificationDateTime.Date);
        }
        finally
        {
            Directory.Delete(destinationPath, true);
        }
    }

    [Test]
    public void LoadFileInformation_FileDoesNotExist_DoesNotPopulateAssetDates()
    {
        string fileName = "nonexistent.jpg";
        Folder folder = new() { Path = dataDirectory! };
        Asset asset = new() { Folder = folder, FileName = fileName };
        DateTime creationTime = default;
        DateTime modificationTime = default;

        _storageService!.LoadFileInformation(asset);

        Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
        Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);
    }

    [Test]
    public void LoadFileInformation_NullFolder_DoesNotPopulateAssetDates()
    {
        string fileName = "Image 1.jpg";
        Folder? folder = null;
        Asset asset = new() { Folder = folder!, FileName = fileName };
        DateTime creationTime = default;
        DateTime modificationTime = default;

        _storageService!.LoadFileInformation(asset);

        Assert.AreEqual(creationTime.Date, asset.FileCreationDateTime.Date);
        Assert.AreEqual(modificationTime.Date, asset.FileModificationDateTime.Date);
    }

    [Test]
    public void LoadFileInformation_NullFilePath_ThrowsArgumentNullException()
    {
        string fileName = "Image 1.jpg";
        string? path = null;
        Folder folder = new() { Path = path! };
        Asset asset = new() { Folder = folder, FileName = fileName };

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadFileInformation(asset!));
    }

    [Test]
    public void LoadFileInformation_NullAsset_ThrowsNullReferenceException()
    {
        Asset? asset = null;

        Assert.Throws<NullReferenceException>(() => _storageService!.LoadFileInformation(asset!));
    }

    [Test]
    [TestCase("Image 1.jpg")]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 10 portrait.png")]
    [TestCase("Homer.gif")]
    public void IsValidGDIPlusImage_ValidImageData_ReturnsTrue(string fileName)
    {
        string filePath = Path.Combine(dataDirectory!, fileName);
        byte[] validImageData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidGDIPlusImage(validImageData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidGDIPlusImage_InvalidImageData_ReturnsFalse()
    {
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] invalidImageData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidGDIPlusImage(invalidImageData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidGDIPlusImage_EmptyImageData_ReturnsFalse()
    {
        byte[] emptyHeicData = Array.Empty<byte>();

        bool result = _storageService!.IsValidGDIPlusImage(emptyHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_ValidImageData_ReturnsTrue()
    {
        string filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] validHeicData = File.ReadAllBytes(filePath);

        bool result = _storageService!.IsValidHeic(validHeicData);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsValidHeic_InvalidImageData_ReturnsFalse()
    {
        byte[] invalidHeicData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        bool result = _storageService!.IsValidHeic(invalidHeicData);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsValidHeic_EmptyImageData_ThrowsArgumentException()
    {
        byte[] emptyHeicData = Array.Empty<byte>();

        Assert.Throws<ArgumentException>(() => _storageService!.IsValidHeic(emptyHeicData));
    }

    private static bool IsValidImage(string filePath)
    {
        try
        {
            using (var image = Image.FromFile(filePath))
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
