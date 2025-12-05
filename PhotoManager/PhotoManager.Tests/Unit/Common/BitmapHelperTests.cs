using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class BitmapHelperTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(Rotation.Rotate0, 1280, 720)]
    [TestCase(Rotation.Rotate90, 720, 1280)]
    [TestCase(Rotation.Rotate180, 1280, 720)]
    [TestCase(Rotation.Rotate270, 720, 1280)]
    // [TestCase(null, 1280, 720)]
    public void LoadBitmapOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_HeicImageFormat_ReturnsBitmapImageWithIncorrectData()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate0;

        BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
            Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
            Assert.That(image.PixelWidth, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
            Assert.That(image.PixelHeight, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0)); // We should have the width value
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0)); // We should have the height value
        }
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);

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

        OverflowException? exception = Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate0;
        const int width = 100;
        const int height = 100;

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.PixelWidth, Is.EqualTo(width));
            Assert.That(image.PixelHeight, Is.EqualTo(height));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
        }
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(FileNames.IMAGE_11_HEIC, Rotation.Rotate0, Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, Rotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, Rotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, Rotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    // [TestCase("FileNames.IMAGE_11_HEIC", null, Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicOriginalImage_ValidBufferAndRotation_ReturnsBitmapImage(string fileName, Rotation rotation, Rotation expectedRotation, int expectedPixelWidth, int expectedPixelHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));

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
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
    
        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

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
    public void LoadBitmapHeicThumbnailImage_NegativeWidthOrHeight_ReturnsBitmapImage(int width, int height, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate90, -100, -100);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be empty. (Parameter 'stream')"));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(Rotation.Rotate0, 1280, 720)]
    [TestCase(Rotation.Rotate90, 720, 1280)]
    [TestCase(Rotation.Rotate180, 1280, 720)]
    [TestCase(Rotation.Rotate270, 720, 1280)]
    // [TestCase(null, 1280, 720)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation, int expectedWith, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath!, rotation);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const Rotation rotation = Rotation.Rotate0;

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
            Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
            Assert.That(image.PixelWidth, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
            Assert.That(image.PixelHeight, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

        using (Assert.EnterMultipleScope())
        {
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
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

        using (Assert.EnterMultipleScope())
        {
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
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_FilePathIsNull_ReturnsBitmapImage()
    {
        string? filePath = null;
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath!, rotation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageFromPathViewerUserControl_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation));

        Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);

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

        OverflowException? exception = Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));

        Assert.That(exception?.Message, Is.EqualTo("The image data generated an overflow during processing."));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'buffer')"));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsNotSupportedException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_HeicImageFormat_ReturnsBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
        byte[] buffer = File.ReadAllBytes(filePath);
        const int width = 100;
        const int height = 100;

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.PixelWidth, Is.EqualTo(width));
            Assert.That(image.PixelHeight, Is.EqualTo(height));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
        }
    }

    [Test]
    [TestCase(FileNames.IMAGE_1_JPG, 1280, 720)]
    [TestCase(FileNames.IMAGE_8_JPEG, 1280, 720)]
    [TestCase(FileNames.IMAGE_10_PORTRAIT_PNG, 720, 1280)]
    [TestCase(FileNames.HOMER_GIF, 320, 320)]
    [TestCase(FileNames.IMAGE_11_HEIC, 3024, 4032)]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    public void LoadBitmapFromPath_ValidImagePath_ReturnsNonNullBitmap(string fileName, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(bitmap, Is.Not.Null);
        Assert.That(bitmap!.Width, Is.EqualTo(expectedWidth));
        Assert.That(bitmap.Height, Is.EqualTo(expectedHeight));

        AssertBrightnessValues(bitmap, 0, 0);
        AssertBrightnessValues(bitmap, 1, 0);
        AssertBrightnessValues(bitmap, 0, 1);
        AssertBrightnessValues(bitmap, 1, 1);
        AssertBrightnessValues(bitmap, 2, 5);
    }

    [Test]
    public void LoadBitmapFromPath_ImageDoesNotExist_ReturnsNull()
    {
        string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_PNG);

        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.That(bitmap, Is.Null);
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsInvalid_ReturnsNull()
    {
        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(_dataDirectory!);

        Assert.That(bitmap, Is.Null);
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsNull_ReturnsNull()
    {
        string? imagePath = null;

        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(imagePath!);

        Assert.That(bitmap, Is.Null);
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetJpegBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetPngBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    [Test]
    [TestCase(FileNames.IMAGE_8_JPEG)]
    [TestCase(FileNames.IMAGE_1_JPG)]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.That(imageBuffer, Is.Not.Null);
        Assert.That(imageBuffer, Is.Not.Empty);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, Directories.IMAGE_CONVERTED);

        try
        {
            Assert.That(ExifHelper.IsValidGDIPlusImage(imageBuffer), Is.True);
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetGifBitmapImage(image));

        Assert.That(exception?.Message, Is.EqualTo("Operation is not valid due to the current state of the object."));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBitmapImage(invalidImage!));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'source')"));
    }

    private static void AssertBrightnessValues(Bitmap bitmap, int x, int y)
    {
        Color pixelColor = bitmap.GetPixel(x, y);
        float brightness = pixelColor.GetBrightness();

        // Assert.That(brightness, Is.Not.Null);
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
