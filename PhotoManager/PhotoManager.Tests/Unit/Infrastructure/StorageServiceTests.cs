using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class StorageServiceTests
{
    private string? dataDirectory;
    private IStorageService? _storageService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var directoryName = Path.GetDirectoryName(typeof(StorageServiceTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");

        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock.MockGetValue("appsettings:CatalogBatchSize", "100");

        _storageService = new StorageService(new UserConfigurationService(configurationMock.Object));
    }

    [Test]
    [TestCase(1.0, "v1.0")]
    [TestCase(1.1, "v1.1")]
    [TestCase(2.0, "v2.0")]
    public void ResolveDataDirectory_ValidStorageVersion_ReturnsCorrectPath(double storageVersion, string storageVersionPath)
    {
        string expected = Path.Combine(PathConstants.PathBackup, storageVersionPath);

        string result = _storageService!.ResolveDataDirectory(storageVersion);

        Assert.False(string.IsNullOrWhiteSpace(result));
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
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, rotation, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsOverflowException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    [TestCase(100, 100)]
    [TestCase(10000, 100)]
    [TestCase(100, 10000)]
    [TestCase(0, 10000)]
    [TestCase(100, 0)]
    [TestCase(0, 0)]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(-100, -100)]
    [TestCase(1000000, 100)]
    [TestCase(100, 1000000)]
    [TestCase(100, null)]
    [TestCase(null, 100)]
    [TestCase(null, null)]
    public void LoadBitmapThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsBitmapImage(int width, int height)
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));
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
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
        Assert.AreEqual(image.PixelWidth, expectedPixelWidth);
        Assert.AreEqual(image.PixelHeight, expectedPixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(Rotation.Rotate0)]
    [TestCase(Rotation.Rotate90)]
    [TestCase(Rotation.Rotate180)]
    [TestCase(Rotation.Rotate270)]
    [TestCase(null)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation)
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FileNotExists_ReturnsDefaultBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Invalid.jpg");
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_NullFilePath_ReturnsDefaultBitmapImage()
    {
        string? filePath = null;
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(Rotation.Rotate0, 3024, 4032)]
    [TestCase(Rotation.Rotate90, 4032, 3024)]
    [TestCase(Rotation.Rotate180, 3024, 4032)]
    [TestCase(Rotation.Rotate270, 4032, 3024)]
    [TestCase(null, 3024, 4032)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0)); // Rotate0 because the BitmapImage (default rotation 0) is created from a MagickImage (containing the right rotation)
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
        Assert.AreEqual(image.PixelWidth, expectedPixelWidth);
        Assert.AreEqual(image.PixelHeight, expectedPixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ReturnsPartialBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(Rotation.Rotate0, 100, 100)]
    [TestCase(Rotation.Rotate90, 100, 100)]
    [TestCase(Rotation.Rotate180, 100, 100)]
    [TestCase(Rotation.Rotate270, 100, 100)]
    [TestCase(null, 100, 100)]
    [TestCase(Rotation.Rotate90, 10000, 100)]
    [TestCase(Rotation.Rotate90, 100, 10000)]
    [TestCase(Rotation.Rotate90, 0, 10000)]
    [TestCase(Rotation.Rotate90, 100, 0)]
    [TestCase(Rotation.Rotate90, 0, 0)]
    [TestCase(null, 100, 100)]
    [TestCase(Rotation.Rotate90, null, 100)]
    [TestCase(Rotation.Rotate90, 100, null)]
    [TestCase(Rotation.Rotate90, null, null)]
    [TestCase(Rotation.Rotate0, 1000000, 100)]
    [TestCase(Rotation.Rotate0, 100, 1000000)]
    [TestCase(null, 100, null)]
    [TestCase(null, null, 100)]
    [TestCase(null, null, null)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int width, int height)
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0)); // Rotate0 because the BitmapImage (default rotation 0) is created from a MagickImage (containing the right rotation)
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100)]
    [TestCase(100, -100)]
    [TestCase(-100, -100)]
    public void LoadBitmapHeicThumbnailImage_InvalidWidthOrHeightOrBoth_ThrowsArgumentException(int width, int height)
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, width, height));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        var image = _storageService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, 1000000, 1000000);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ReturnsPartialBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(Rotation.Rotate0)]
    [TestCase(Rotation.Rotate90)]
    [TestCase(Rotation.Rotate180)]
    [TestCase(Rotation.Rotate270)]
    [TestCase(null)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotation_ReturnsBitmapImage(Rotation rotation)
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_NullFilePath_ReturnsBitmapImage()
    {
        string? filePath = null;
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath!, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FileNotExists_ReturnsDefaultBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "invalid_path.heic");
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ReturnsPartialBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation));
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
        var exifOrientation = -10;
        Rotation rotation = _storageService!.GetImageRotation((ushort)exifOrientation);

        Assert.AreEqual(Rotation.Rotate0, rotation);
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        Assert.Throws<InvalidOperationException>(() => _storageService!.GetImageRotation((ushort)exifOrientation!));
    }

    //[Test]
    //[Category("Supplemental")]
    //public void WriteReadJsonTestAndReadObjectFromJsonFile_Tests()
    //{
    //    List<string> writtenList = new() { "Value 1", "Value 2" };
    //    string jsonPath = Path.Combine(dataDirectory!, "test.json");

    //    WriteObjectToJsonFile(writtenList, jsonPath);
    //    List<string> readList = ReadObjectFromJsonFile<List<string>>(jsonPath) ?? new List<string>();

    //    Assert.AreEqual(writtenList.Count(), readList.Count());
    //    Assert.AreEqual(writtenList[0], readList[0]);
    //    Assert.AreEqual(writtenList[1], readList[1]);
    //}

    //private static T? ReadObjectFromJsonFile<T>(string jsonFilePath)
    //{
    //    return FileHelper.ReadObjectFromJsonFile<T>(jsonFilePath);
    //}

    //private static void WriteObjectToJsonFile(object anObject, string jsonFilePath)
    //{
    //    FileHelper.WriteObjectToJsonFile(anObject, jsonFilePath);
    //}
}
