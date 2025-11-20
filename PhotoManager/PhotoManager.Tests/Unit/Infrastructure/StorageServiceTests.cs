using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;

namespace PhotoManager.Tests.Unit.Infrastructure;

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

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
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
    // [TestCase(null, 100, 100)]
    // [TestCase(Rotation.Rotate90, null, 100)]
    // [TestCase(Rotation.Rotate90, 100, null)]
    // [TestCase(Rotation.Rotate90, null, null)]
    [TestCase(Rotation.Rotate90, -100, 100)]
    [TestCase(Rotation.Rotate90, 100, -100)]
    [TestCase(Rotation.Rotate90, -100, -100)]
    [TestCase(Rotation.Rotate0, 1000000, 100)]
    [TestCase(Rotation.Rotate0, 100, 1000000)]
    // [TestCase(null, 100, null)]
    // [TestCase(null, null, 100)]
    // [TestCase(null, null, null)]
    public void LoadBitmapThumbnailImage_ValidBufferAndRotationAndWidthAndHeight_ReturnsBitmapImage(Rotation rotation, int width, int height)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
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
    // [TestCase(100, null, 100, 56)]
    // [TestCase(null, 100, 177, 100)]
    // [TestCase(null, null, 1280, 720)]
    public void LoadBitmapThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsBitmapImage(int width, int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapThumbnailImage(buffer, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        OverflowException? exception = Assert.Throws<OverflowException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapThumbnailImage(buffer!, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedPixelHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation, int expectedWith, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWith));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWith));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultBitmapImage()
    {
        string? filePath = null;
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _storageService!.LoadBitmapImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_HEIC, Rotation.Rotate0, Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, Rotation.Rotate90, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, Rotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, Rotation.Rotate270, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    // [TestCase(FileNames.IMAGE_11_HEIC, null, Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(string fileName, Rotation rotation, Rotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedPixelHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
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
    // [TestCase(null, 100, 100, Rotation.Rotate0, 75, 100)]
    // [TestCase(Rotation.Rotate90, null, 100, Rotation.Rotate90, 100, 133)]
    // [TestCase(Rotation.Rotate90, 100, null, Rotation.Rotate90, 75, 100)]
    // [TestCase(Rotation.Rotate90, null, null, Rotation.Rotate90, 1, 1)]
    [TestCase(Rotation.Rotate0, 1000000, 100, Rotation.Rotate0, 75, 100)]
    [TestCase(Rotation.Rotate0, 100, 1000000, Rotation.Rotate0, 100, 133)]
    // [TestCase(null, 100, null, Rotation.Rotate0, 100, 133)]
    // [TestCase(null, null, 100, Rotation.Rotate0, 75, 100)]
    // [TestCase(null, null, null, Rotation.Rotate0, 1, 1)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, 100, 100, Rotation.Rotate90, 100, 75)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, 100, 100, Rotation.Rotate180, 75, 100)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, 100, 100, Rotation.Rotate270, 100, 75)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100, 100, 133)]
    [TestCase(100, -100, 75, 100)]
    public void LoadBitmapHeicThumbnailImage_InvalidWidthOrHeightOrBoth_ThrowsArgumentException(int width, int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate90, -100, -100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate270, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
        Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
        Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsBitmapImage()
    {
        string? filePath = null;
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.StreamSource, Is.Null);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _storageService!.LoadBitmapHeicImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
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

        Assert.That(rotation, Is.EqualTo(expectedRotation));
    }

    [Test]
    public void GetImageRotation_InvalidExifOrientation_ReturnsCorrectRotationValue()
    {
        int exifOrientation = -10;
        Rotation rotation = _storageService!.GetImageRotation((ushort)exifOrientation);

        Assert.That(rotation, Is.EqualTo(Rotation.Rotate0));
    }

    [Test]
    public void GetImageRotation_NullExifOrientation_ThrowsInvalidOperationException()
    {
        ushort? exifOrientation = null;

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _storageService!.GetImageRotation((ushort)exifOrientation!));

        Assert.That(exception?.Message, Is.EqualTo("Nullable object must have a value."));
    }
}
