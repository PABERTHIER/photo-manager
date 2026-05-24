using SkiaSharp;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class BitmapHelperTests
{
    private string? _assetsDirectory;
    private TestLogger<BitmapHelperTests>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(ImageRotation rotation,
        int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_1_JPG));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_1_JPG));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_HeicImageFormat_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapOriginalImage(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    [TestCase(ImageRotation.Rotate0, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate180, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate270, 100, 100, 100, 100)]
    [TestCase(ImageRotation.Rotate90, 10000, 100, 10000, 100)]
    [TestCase(ImageRotation.Rotate90, 100, 10000, 100, 10000)]
    // [TestCase(null, 100, 100)]
    // [TestCase(ImageRotation.Rotate90, null, 100)]
    // [TestCase(ImageRotation.Rotate90, 100, null)]
    // [TestCase(ImageRotation.Rotate90, null, null)]
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

        SkiaImageData image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_LargeWidthAndHeight_ThrowsNotSupportedException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            BitmapHelper.LoadBitmapThumbnailImage(buffer, ImageRotation.Rotate0, 1000000, 1000000, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException("No imaging component suitable to complete this operation was found.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(100));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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
            BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
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
    // [TestCase("FileNames.IMAGE_11_HEIC", null, ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(string fileName,
        ImageRotation rotation, ImageRotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedPixelWidth));
        Assert.That(image.Height, Is.EqualTo(expectedPixelHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation, new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation, new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image =
            BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation, new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndNotRotatedImage_ReturnsBitmapImage(
        ImageRotation rotation, int width, int height, ImageRotation expectedRotation, int expectedWidth,
        int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(expectedRotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NegativeWidthAndHeight_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate90, -100, -100,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate90));
        Assert.That(image.Width, Is.EqualTo(75));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
            BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>()));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
            new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100,
                new TestLogger<BitmapHelperTests>());

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(75));
        Assert.That(image.Height, Is.EqualTo(100));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        SkiaImageData image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(expectedWith));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultBitmapImage()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapImageFromPath(filePath!, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_1_JPG));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_1_JPG));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate0;

        SkiaImageData image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
            Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

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

        SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsBitmapImage()
    {
        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath!, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            Assert.That(image.Width, Is.EqualTo(1));
            Assert.That(image.Height, Is.EqualTo(1));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate90;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            Assert.That(image.Width, Is.EqualTo(1));
            Assert.That(image.Height, Is.EqualTo(1));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_CorruptedHeicFile_ReturnsDefaultBitmapImage()
    {
        string validFilePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        string tempDirectory = Path.Combine(_assetsDirectory!, "Temp");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            const string invalidHeicFileName = "Invalid_Corrupted.heic";
            string filePath = Path.Combine(tempDirectory, invalidHeicFileName);
            const ImageRotation rotation = ImageRotation.Rotate90;

            ImageHelper.CreateInvalidImage(validFilePath, filePath);

            SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(image, Is.Not.Null);
                Assert.That(image.Bitmap, Is.Not.Null);
                Assert.That(image.Bitmap.IsEmpty, Is.False);
                Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                Assert.That(image.Width, Is.EqualTo(1));
                Assert.That(image.Height, Is.EqualTo(1));

                string expectedErrorMessage = $"Failed to load HEIC image from path: {filePath}.";
                _testLogger!.AssertLogExceptions([new Exception(expectedErrorMessage)], typeof(BitmapHelperTests));
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ReturnBitmapImageWithInvalidRotation()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = (ImageRotation)999;

        SkiaImageData image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        SkiaImageData image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height, _testLogger!);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 1000000, 1000000, _testLogger!));

        Assert.That(exception?.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([new Exception("Unable to allocate pixels for the bitmap.")],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer!, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() =>
                BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        // ReSharper disable once NotResolvedInText
        _testLogger!.AssertLogExceptions([new ArgumentNullException("codec")], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_HeicImageFormat_ReturnsValidImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const int width = 100;
        const int height = 100;

        SkiaImageData image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height, _testLogger!);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image.Bitmap, Is.Not.Null);
            Assert.That(image.Bitmap.IsEmpty, Is.False);
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Width, Is.EqualTo(width));
            Assert.That(image.Height, Is.EqualTo(height));
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(FileNames.IMAGE_8_JPEG, PixelWidthAsset.IMAGE_8_JPEG, PixelHeightAsset.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, PixelWidthAsset.IMAGE_10_PORTRAIT_PNG,
        PixelHeightAsset.IMAGE_10_PORTRAIT_PNG)]
    [TestCase(FileNames.HOMER_GIF, PixelWidthAsset.HOMER_GIF, PixelHeightAsset.HOMER_GIF)]
    public void LoadBitmapFromPath_ValidImagePath_ReturnsValidImage(string fileName, int expectedWidth,
        int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        AssertBrightnessValues(image.Bitmap, 0, 0);
        AssertBrightnessValues(image.Bitmap, 1, 0);
        AssertBrightnessValues(image.Bitmap, 0, 1);
        AssertBrightnessValues(image.Bitmap, 1, 1);
        AssertBrightnessValues(image.Bitmap, 2, 5);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, PixelWidthAsset.IMAGE_11_90_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    public void LoadBitmapFromPath_HeicImagePath_ReturnsValidImage(string fileName, int expectedWidth,
        int expectedHeight)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);

        SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(image, Is.Not.Null);
        Assert.That(image!.Bitmap, Is.Not.Null);
        Assert.That(image.Bitmap.IsEmpty, Is.False);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));

        AssertBrightnessValues(image.Bitmap, 0, 0);
        AssertBrightnessValues(image.Bitmap, 1, 0);
        AssertBrightnessValues(image.Bitmap, 0, 1);
        AssertBrightnessValues(image.Bitmap, 1, 1);
        AssertBrightnessValues(image.Bitmap, 2, 5);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_ImageDoesNotExist_ReturnsNull()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(image, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_InvalidImageFile_ReturnsNull()
    {
        string testDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);
        string filePath = Path.Combine(testDirectory, "invalid-image.jpg");

        try
        {
            Directory.CreateDirectory(testDirectory);
            File.WriteAllBytes(filePath, [0x00, 0x01, 0x02, 0x03]);

            SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(filePath);

            Assert.That(image, Is.Null);

            _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void LoadBitmapFromPath_InvalidHeicFile_ReturnsNull()
    {
        string testDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);
        string filePath = Path.Combine(testDirectory, "invalid-image.heic");

        try
        {
            Directory.CreateDirectory(testDirectory);
            byte[] invalidHeicBuffer =
            [
                0x00, 0x00, 0x00, 0x0C,
                0x66, 0x74, 0x79, 0x70,
                0x68, 0x65, 0x69, 0x63
            ];
            File.WriteAllBytes(filePath, invalidHeicBuffer);

            SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(filePath);

            Assert.That(image, Is.Null);

            _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsInvalid_ReturnsNull()
    {
        SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(_assetsDirectory!);

        Assert.That(image, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsNull_ReturnsNull()
    {
        string? imagePath = null;

        SkiaImageData? image = BitmapHelper.LoadBitmapFromPath(imagePath!);

        Assert.That(image, Is.Null);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
            _testLogger!);

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBitmapImage_HeicValidImage_ReturnsJpegByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_JPEG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
            _testLogger!);

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    // TODO: Inspire from this test to dev the conversion feature
    [Test]
    public void GetPngBitmapImage_HeicValidImage_ReturnsPngByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_PNG);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetPngBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_assetsDirectory!, fileName);
        SkiaImageData image = SkiaImageData.FromEncodedBytes(File.ReadAllBytes(filePath), ImageRotation.Rotate0,
            _testLogger!);

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageBuffer, Is.Not.Empty);
            Assert.That(imageBuffer[0], Is.EqualTo((byte)0x47));
            Assert.That(imageBuffer[1], Is.EqualTo((byte)0x49));
            Assert.That(imageBuffer[2], Is.EqualTo((byte)0x46));
            Assert.That(imageBuffer[3], Is.EqualTo((byte)0x38));
        }

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBitmapImage_HeicValidImage_ReturnsGifByteArray()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);

        SkiaImageData image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, ImageRotation.Rotate0, 100, 100,
            new TestLogger<BitmapHelperTests>());

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageBuffer, Is.Not.Empty);
            Assert.That(imageBuffer[0], Is.EqualTo((byte)0x47));
            Assert.That(imageBuffer[1], Is.EqualTo((byte)0x49));
            Assert.That(imageBuffer[2], Is.EqualTo((byte)0x46));
            Assert.That(imageBuffer[3], Is.EqualTo((byte)0x38));
        }

        string destinationNewFileDirectory = Path.Combine(_assetsDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGdiPlusImage(imageBuffer, new TestLogger<BitmapHelperTests>()),
                Is.True);
            Directory.CreateDirectory(destinationNewFileDirectory);
            string destinationNewFilePath = Path.Combine(destinationNewFileDirectory, FileNames.IMAGE_CONVERTED_GIF);
            File.WriteAllBytes(destinationNewFilePath, imageBuffer);
            Assert.That(IsValidImage(destinationNewFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(destinationNewFileDirectory, true);
        }

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBitmapImage_InvalidImage_ReturnsEmptyByteArray()
    {
        SkiaImageData image = new(new(), ImageRotation.Rotate0);

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Empty);

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        IImageData? invalidImage = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'image')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
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

        (int width, int height) = BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!);

        Assert.That(width, Is.EqualTo(expectedWidth));
        Assert.That(height, Is.EqualTo(expectedHeight));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_HeicImageFormat_ReturnsDimensions()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const ImageRotation rotation = ImageRotation.Rotate0;

        (int width, int height) = BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!);

        Assert.That(width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_BmpBufferWithoutHeaderReader_ReturnsDimensions()
    {
        using SKBitmap bitmap = new(13, 17);
        using SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0);
        byte[] buffer = imageData.ToByteArray(ImageEncodingFormat.Bmp);

        (int width, int height) = BitmapHelper.GetImageDimensions(buffer, ImageRotation.Rotate90, _testLogger!);

        Assert.That(width, Is.EqualTo(17));
        Assert.That(height, Is.EqualTo(13));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() =>
                BitmapHelper.GetImageDimensions(buffer!, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));

        _testLogger!.AssertLogExceptions([], typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() => BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() => BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferTooShort_ThrowsNotSupportedException()
    {
        // JPEG signature detected, but buffer is too short for the while loop (offset=2, buffer.Length-3=1)
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xFF];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception =
            Assert.Throws<NotSupportedException>(() => BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithCorruptMarker_ThrowsNotSupportedException()
    {
        // JPEG signature valid, but byte at marker position is not 0xFF
        byte[] buffer = [0xFF, 0xD8, 0x00, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithTruncatedSofSegment_ThrowsNotSupportedException()
    {
        // SOF0 marker found (0xC0) but not enough bytes after it to read the dimensions
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions(
            [new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithMalformedSegmentLength_ThrowsNotSupportedException()
    {
        // APP0 marker (0xE0) with segment length = 1, which is below the minimum of 2
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x01, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    [Test]
    [Category("From AssetCreationService for CreateAsset() to get image dimensions")]
    public void GetImageDimensions_JpegBufferWithNoSofMarkerFoundAfterSegmentSkip_ThrowsNotSupportedException()
    {
        // APP0 marker (0xE0) with valid length=8 is skipped, then the while condition becomes false (no SOF found)
        byte[] buffer = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00];
        const ImageRotation rotation = ImageRotation.Rotate0;
        const string expectedExceptionMessage = "No imaging component suitable to complete this operation was found.";

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() =>
            BitmapHelper.GetImageDimensions(buffer, rotation, _testLogger!));

        Assert.That(exception?.Message, Is.EqualTo(expectedExceptionMessage));

        _testLogger!.AssertLogExceptions([new NotSupportedException(expectedExceptionMessage)],
            typeof(BitmapHelperTests));
    }

    private static void AssertBrightnessValues(SKBitmap bitmap, int x, int y)
    {
        SKColor pixelColor = bitmap.GetPixel(x, y);
        float red = pixelColor.Red / 255f;
        float green = pixelColor.Green / 255f;
        float blue = pixelColor.Blue / 255f;
        float max = Math.Max(red, Math.Max(green, blue));
        float min = Math.Min(red, Math.Min(green, blue));
        float brightness = (max + min) / 2f;

        Assert.That(brightness, Is.GreaterThan(0));
        Assert.That(brightness, Is.LessThan(1));
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
