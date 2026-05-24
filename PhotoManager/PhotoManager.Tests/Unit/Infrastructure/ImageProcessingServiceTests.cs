using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class ImageProcessingServiceTests
{
    private string? _assetsDirectory;

    private ImageProcessingService? _imageProcessingService;
    private TestLogger<ImageProcessingService>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _imageProcessingService = new(_testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    [TestCase(ImageRotation.Rotate0, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate180, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate270, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 10000, 100, 10000, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 10000, 100, 10000)]
    [TestCase(ImageRotation.Rotate90, 0, 10000, 5625, 10000)]
    [TestCase(ImageRotation.Rotate90, 100, 0, 100, 177)]
    [TestCase(ImageRotation.Rotate90, 0, 0, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, 100, 100)]
    // [TestCase(ImageRotation.Rotate90, null, 100)]
    // [TestCase(ImageRotation.Rotate90, 100, null)]
    // [TestCase(ImageRotation.Rotate90, null, null)]
    [TestCase(ImageRotation.Rotate90, -100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 100, -100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, -100, -100, 100, 100)]
    [TestCase(ImageRotation.Rotate0, 1000000, 100, 1000000, 100)]
    [TestCase(ImageRotation.Rotate0, 100, 1000000, 100, 1000000)]
    // [TestCase(null, 100, null)]
    // [TestCase(null, null, 100)]
    // [TestCase(null, null, null)]
    public void LoadBitmapThumbnailImage_ValidBufferAndRotationAndWidthAndHeight_ReturnsBitmapImage(
        ImageRotation rotation, int width, int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 1000000, 1000000));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ReturnsBitmapImageWithRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        IImageData image = _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, 100, 100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(100));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_HeicImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;
        const int width = 100;
        const int height = 100;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            _imageProcessingService!.LoadBitmapThumbnailImage(buffer, rotation, width, height));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    [TestCase(100, 100, 100, 100)]
    [TestCase(10000, 100, 10000, 100)]
    [TestCase(100, 10000, 100, 10000)]
    [TestCase(0, 10000, 17777, 10000)]
    [TestCase(100, 0, 100, 56)]
    [TestCase(0, 0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(-100, 100, 100, 100)]
    [TestCase(100, -100, 100, 100)]
    [TestCase(-100, -100, 100, 100)]
    [TestCase(1000000, 100, 1000000, 100)]
    [TestCase(100, 1000000, 100, 1000000)]
    // [TestCase(100, null, 100, 56)]
    // [TestCase(null, 100, 177, 100)]
    // [TestCase(null, null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapThumbnailImageAssetRepository_ValidBufferAndWidthAndHeight_ReturnsBitmapImage(int width,
        int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapThumbnailImage(buffer, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([new Exception("Unable to allocate pixels for the bitmap.")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.LoadBitmapThumbnailImage(buffer!, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const int width = 100;
        const int height = 100;

        IImageData image = _imageProcessingService!.LoadBitmapThumbnailImage(buffer, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(width));
        Assert.That(image.Height, Is.EqualTo(height));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(ImageRotation rotation,
        int expectedWith, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);

        IImageData image = _imageProcessingService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWith));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultBitmapImage()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ReturnsBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        const ImageRotation rotation = (ImageRotation)999;

        IImageData image = _imageProcessingService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_1_JPG));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_1_JPG));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate0;

        IImageData image = _imageProcessingService!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_HEIC, ImageRotation.Rotate0, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC,
        PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, ImageRotation.Rotate90,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, ImageRotation.Rotate180,
        PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, ImageRotation.Rotate270,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    // [TestCase(FileNames.IMAGE_11_HEIC, null, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(string fileName,
        ImageRotation rotation, ImageRotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.LoadBitmapHeicOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                _imageProcessingService!.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ReturnsBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        IImageData image = _imageProcessingService!.LoadBitmapHeicOriginalImage(buffer, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(ImageRotation.Rotate0, 100, 100, ImageRotation.Rotate0, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 100, ImageRotation.Rotate90, 75, 100)]
    [TestCase(ImageRotation.Rotate180, 100, 100, ImageRotation.Rotate180, 75, 100)]
    [TestCase(ImageRotation.Rotate270, 100, 100, ImageRotation.Rotate270, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 10000, 100, ImageRotation.Rotate90, 75, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 10000, ImageRotation.Rotate90, 100, 133)]
    [TestCase(ImageRotation.Rotate90, 0, 10000, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC,
        PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, 100, 0, ImageRotation.Rotate90, 100, 133)]
    [TestCase(ImageRotation.Rotate90, 0, 0, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC,
        PixelHeightAsset.IMAGE_11_HEIC)]
    // [TestCase(null, 100, 100, ImageRotation.Rotate0, 75, 100)]
    // [TestCase(ImageRotation.Rotate90, null, 100, ImageRotation.Rotate90, 100, 133)]
    // [TestCase(ImageRotation.Rotate90, 100, null, ImageRotation.Rotate90, 75, 100)]
    // [TestCase(ImageRotation.Rotate90, null, null, ImageRotation.Rotate90, 1, 1)]
    [TestCase(ImageRotation.Rotate0, 1000000, 100, ImageRotation.Rotate0, 75, 100)]
    [TestCase(ImageRotation.Rotate0, 100, 1000000, ImageRotation.Rotate0, 100, 133)]
    // [TestCase(null, 100, null, ImageRotation.Rotate0, 100, 133)]
    // [TestCase(null, null, 100, ImageRotation.Rotate0, 75, 100)]
    // [TestCase(null, null, null, ImageRotation.Rotate0, 1, 1)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotation_ReturnsBitmapImage(ImageRotation rotation,
        int width, int height, ImageRotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, 100, 100, ImageRotation.Rotate90, 100, 75)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, 100, 100, ImageRotation.Rotate180, 75, 100)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, 100, 100, ImageRotation.Rotate270, 100, 75)]
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName,
        ImageRotation rotation, int width, int height, ImageRotation expectedRotation, int expectedWidth,
        int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    [TestCase(-100, 100, 75, 100)]
    [TestCase(100, -100, 75, 100)]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthOrHeight_ReturnsBitmapImage(int width, int height,
        int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthAndHeight_ReturnsValidBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate90, -100,
            -100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate90));
        Assert.That(image.Width, Is.EqualTo(75));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([new Exception("The image is not valid or in an unsupported format")],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ReturnsBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        IImageData image = _imageProcessingService!.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(75));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);

        IImageData image = _imageProcessingService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    // HEIC decoding auto-orients these files before the stored rotation is applied, so 90°/270° samples swap dimensions.
    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, PixelHeightAsset.IMAGE_11_270_DEG_HEIC,
        PixelWidthAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(
        string fileName, ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        IImageData image = _imageProcessingService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsBitmapImage()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate90;

        IImageData image = _imageProcessingService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ReturnsBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = (ImageRotation)999;

        IImageData image = _imageProcessingService!.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG,
        PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_1_JPG, ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG,
        PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_9_PNG, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG)]
    [TestCase(FileNames.IMAGE_9_PNG, ImageRotation.Rotate90, PixelHeightAsset.IMAGE_9_PNG, PixelWidthAsset.IMAGE_9_PNG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_10_PORTRAIT_PNG,
        PixelHeightAsset.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, ImageRotation.Rotate0, PixelWidthAsset.HOMER_GIF, PixelHeightAsset.HOMER_GIF)]
    public void GetImageDimensions_ValidBufferAndRotation_ReturnsDimensions(string fileName, ImageRotation rotation,
        int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        (int width, int height) = _imageProcessingService!.GetImageDimensions(buffer, rotation);

        Assert.That(width, Is.EqualTo(expectedWidth));
        Assert.That(height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_HeicImageFormat_FallsBackToWpfAndReturnsDimensions()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;

        (int width, int height) = _imageProcessingService!.GetImageDimensions(buffer, rotation);

        Assert.That(width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferTooShort_ThrowsNotSupportedException()
    {
        // JPEG signature detected, but buffer is too short for the while loop (offset=2, buffer.Length-3=1)
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xFF];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithCorruptMarker_ThrowsNotSupportedException()
    {
        // JPEG signature valid, but byte at marker position is not 0xFF
        byte[] buffer = [0xFF, 0xD8, 0x00, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithTruncatedSofSegment_ThrowsNotSupportedException()
    {
        // SOF0 marker found (0xC0) but not enough bytes after it to read the dimensions
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithMalformedSegmentLength_ThrowsNotSupportedException()
    {
        // APP0 marker (0xE0) with segment length = 1, which is below the minimum of 2
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x01, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithNoSofMarkerFoundAfterSegmentSkip_ThrowsNotSupportedException()
    {
        // APP0 marker (0xE0) with valid length=8 is skipped, then the while condition becomes false (no SOF found)
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage =
            "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                _imageProcessingService!.GetImageDimensions(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(ImageProcessingService));
    }
}
