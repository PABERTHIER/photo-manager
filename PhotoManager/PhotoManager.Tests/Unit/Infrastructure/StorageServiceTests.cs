﻿namespace PhotoManager.Tests.Unit.Infrastructure;

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

        _userConfigurationService = new (configurationRootMock.Object);
        _storageService = new (_userConfigurationService);
    }

    [Test]
    [TestCase("1.0", "v1.0")]
    [TestCase("1.1", "v1.1")]
    [TestCase("2.0", "v2.0")]
    public void ResolveDataDirectory_ValidStorageVersion_ReturnsCorrectPath(string storageVersion, string storageVersionPath)
    {
        string expected = Path.Combine(_userConfigurationService!.PathSettings.BackupPath, storageVersionPath);

        string result = _storageService!.ResolveDataDirectory(storageVersion);

        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        Assert.AreEqual(expected, result);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    [TestCase(Rotation.Rotate0, 100, 100)]
    [TestCase(Rotation.Rotate90, 100, 100)]
    [TestCase(Rotation.Rotate180, 100, 100)]
    [TestCase(Rotation.Rotate270, 100, 100)]
    [TestCase(Rotation.Rotate90, 10000, 100)]
    [TestCase(Rotation.Rotate90, 100, 10000)]
    [TestCase(Rotation.Rotate90, 0, 10000)]
    [TestCase(Rotation.Rotate90, 100, 0)]
    [TestCase(Rotation.Rotate90, 0, 0)]
    [TestCase(null, 100, 100)]
    [TestCase(Rotation.Rotate90, null, 100)]
    [TestCase(Rotation.Rotate90, 100, null)]
    [TestCase(Rotation.Rotate90, null, null)]
    [TestCase(Rotation.Rotate90, -100, 100)]
    [TestCase(Rotation.Rotate90, 100, -100)]
    [TestCase(Rotation.Rotate90, -100, -100)]
    [TestCase(Rotation.Rotate0, 1000000, 100)]
    [TestCase(Rotation.Rotate0, 100, 1000000)]
    [TestCase(null, 100, null)]
    [TestCase(null, null, 100)]
    [TestCase(null, null, null)]
    public void LoadBitmapThumbnailImage_ValidBufferAndRotationAndWidthAndHeight_ReturnsBitmapImage(Rotation rotation, int width, int height)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, rotation, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(rotation, image.Rotation);
        Assert.AreEqual(width, image.DecodePixelWidth);
        Assert.AreEqual(height, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));

        Assert.AreEqual("The image data generated an overflow during processing.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    [TestCase(100, 100, 100, 100)]
    [TestCase(10000, 100, 10000, 100)]
    [TestCase(100, 10000, 100, 10000)]
    [TestCase(0, 10000, 17777, 10000)]
    [TestCase(100, 0, 100, 56)]
    [TestCase(0, 0, 1280, 720)]
    [TestCase(-100, 100, 100, 100)]
    [TestCase(100, -100, 100, 100)]
    [TestCase(-100, -100, 100, 100)]
    [TestCase(1000000, 100, 1000000, 100)]
    [TestCase(100, 1000000, 100, 1000000)]
    [TestCase(100, null, 100, 56)]
    [TestCase(null, 100, 177, 100)]
    [TestCase(null, null, 1280, 720)]
    public void LoadBitmapThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsBitmapImage(int width, int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(width, image.DecodePixelWidth);
        Assert.AreEqual(height, image.DecodePixelHeight);
        Assert.AreEqual(expectedWidth, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(expectedWidth, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));

        Assert.AreEqual("The image data generated an overflow during processing.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(Rotation.Rotate0, 1280, 720)]
    [TestCase(Rotation.Rotate90, 720, 1280)]
    [TestCase(Rotation.Rotate180, 1280, 720)]
    [TestCase(Rotation.Rotate270, 720, 1280)]
    [TestCase(null, 1280, 720)]
    public void LoadBitmapOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(rotation, image.Rotation);
        Assert.AreEqual(expectedPixelWidth, image.Width);
        Assert.AreEqual(expectedPixelHeight, image.Height);
        Assert.AreEqual(expectedPixelWidth, image.PixelWidth);
        Assert.AreEqual(expectedPixelHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapOriginalImage(buffer!, rotation));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(Rotation.Rotate0, 1280, 720)]
    [TestCase(Rotation.Rotate90, 720, 1280)]
    [TestCase(Rotation.Rotate180, 1280, 720)]
    [TestCase(Rotation.Rotate270, 720, 1280)]
    [TestCase(null, 1280, 720)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation, int expectedWith, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(rotation, image.Rotation);
        Assert.AreEqual(expectedWith, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
        Assert.AreEqual(expectedWith, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.jpg");
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultBitmapImage()
    {
        string? filePath = null;
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase("Image_11.heic", Rotation.Rotate0, Rotation.Rotate0, 3024, 4032)]
    [TestCase("Image_11_90.heic", Rotation.Rotate90, Rotation.Rotate90, 4032, 3024)]
    [TestCase("Image_11_180.heic", Rotation.Rotate180, Rotation.Rotate180, 3024, 4032)]
    [TestCase("Image_11_270.heic", Rotation.Rotate270, Rotation.Rotate270, 4032, 3024)]
    [TestCase("Image_11.heic", null, Rotation.Rotate0, 3024, 4032)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(string fileName, Rotation rotation, Rotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(expectedRotation, image.Rotation);
        Assert.AreEqual(expectedPixelWidth, image.Width);
        Assert.AreEqual(expectedPixelHeight, image.Height);
        Assert.AreEqual(expectedPixelWidth, image.PixelWidth);
        Assert.AreEqual(expectedPixelHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer!, rotation));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(Rotation.Rotate0, 100, 100, Rotation.Rotate0, 75, 100)]
    [TestCase(Rotation.Rotate90, 100, 100, Rotation.Rotate90, 75, 100)]
    [TestCase(Rotation.Rotate180, 100, 100, Rotation.Rotate180, 75, 100)]
    [TestCase(Rotation.Rotate270, 100, 100, Rotation.Rotate270, 75, 100)]
    [TestCase(Rotation.Rotate90, 10000, 100, Rotation.Rotate90, 100, 133)]
    [TestCase(Rotation.Rotate90, 100, 10000, Rotation.Rotate90, 75, 100)]
    [TestCase(Rotation.Rotate90, 0, 10000, Rotation.Rotate90, 10000, 13333)]
    [TestCase(Rotation.Rotate90, 100, 0, Rotation.Rotate90, 75, 100)]
    [TestCase(Rotation.Rotate90, 0, 0, Rotation.Rotate90, 1, 1)]
    [TestCase(null, 100, 100, Rotation.Rotate0, 75, 100)]
    [TestCase(Rotation.Rotate90, null, 100, Rotation.Rotate90, 100, 133)]
    [TestCase(Rotation.Rotate90, 100, null, Rotation.Rotate90, 75, 100)]
    [TestCase(Rotation.Rotate90, null, null, Rotation.Rotate90, 1, 1)]
    [TestCase(Rotation.Rotate0, 1000000, 100, Rotation.Rotate0, 75, 100)]
    [TestCase(Rotation.Rotate0, 100, 1000000, Rotation.Rotate0, 100, 133)]
    [TestCase(null, 100, null, Rotation.Rotate0, 100, 133)]
    [TestCase(null, null, 100, Rotation.Rotate0, 75, 100)]
    [TestCase(null, null, null, Rotation.Rotate0, 1, 1)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(expectedRotation, image.Rotation);
        Assert.AreEqual(expectedWidth, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
        Assert.AreEqual(expectedWidth, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase("Image_11_90.heic", Rotation.Rotate90, 100, 100, Rotation.Rotate90, 100, 75)]
    [TestCase("Image_11_180.heic", Rotation.Rotate180, 100, 100, Rotation.Rotate180, 75, 100)]
    [TestCase("Image_11_270.heic", Rotation.Rotate270, 100, 100, Rotation.Rotate270, 100, 75)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(expectedRotation, image.Rotation);
        Assert.AreEqual(expectedWidth, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
        Assert.AreEqual(expectedWidth, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100, "width")]
    [TestCase(100, -100, "height")]
    [TestCase(-100, -100, "width")]
    public void LoadBitmapHeicThumbnailImage_InvalidWidthOrHeightOrBoth_ThrowsArgumentException(int width, int height, string exceptionParameter)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height));

        Assert.AreEqual($"Value should not be negative. (Parameter '{exceptionParameter}')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(Rotation.Rotate0, 3024, 4032)]
    [TestCase(Rotation.Rotate90, 3024, 4032)]
    [TestCase(Rotation.Rotate180, 3024, 4032)]
    [TestCase(Rotation.Rotate270, 3024, 4032)]
    [TestCase(null, 3024, 4032)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(rotation, image.Rotation);
        Assert.AreEqual(expectedWidth, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
        Assert.AreEqual(expectedWidth, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase("Image_11_90.heic", Rotation.Rotate90, 4032, 3024)]
    [TestCase("Image_11_180.heic", Rotation.Rotate180, 3024, 4032)]
    [TestCase("Image_11_270.heic", Rotation.Rotate270, 4032, 3024)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(rotation, image.Rotation);
        Assert.AreEqual(expectedWidth, image.Width);
        Assert.AreEqual(expectedHeight, image.Height);
        Assert.AreEqual(expectedWidth, image.PixelWidth);
        Assert.AreEqual(expectedHeight, image.PixelHeight);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsBitmapImage()
    {
        string? filePath = null;
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath!, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.heic");
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(Rotation.Rotate0, image.Rotation);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ReturnsPartialBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [TestCase((ushort)0, Rotation.Rotate0)]
    [TestCase((ushort)1, Rotation.Rotate0)]
    [TestCase((ushort)2, Rotation.Rotate0)]
    [TestCase((ushort)3, Rotation.Rotate180)]
    [TestCase((ushort)4, Rotation.Rotate180)]
    [TestCase((ushort)5, Rotation.Rotate90)]
    [TestCase((ushort)6, Rotation.Rotate90)]
    [TestCase((ushort)7, Rotation.Rotate270)]
    [TestCase((ushort)8, Rotation.Rotate270)]
    [TestCase((ushort)9, Rotation.Rotate0)]
    [TestCase((ushort)10, Rotation.Rotate0)]
    [TestCase((ushort)10000, Rotation.Rotate0)]
    [TestCase(ushort.MinValue, Rotation.Rotate0)]
    [TestCase(ushort.MaxValue, Rotation.Rotate0)]
    public void GetImageRotation_ValidExifOrientation_ReturnsCorrectRotationValue(ushort exifOrientation, Rotation expectedRotation)
    {
        Rotation rotation = _storageService!.GetImageRotation(exifOrientation);

        Assert.AreEqual(expectedRotation, rotation);
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        Rotation rotation = _storageService!.GetImageRotation((ushort)exifOrientation);

        Assert.AreEqual(Rotation.Rotate0, rotation);
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _storageService!.GetImageRotation((ushort)exifOrientation!));

        Assert.AreEqual("Nullable object must have a value.", exception?.Message);
    }
}
