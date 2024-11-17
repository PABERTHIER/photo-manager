namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class BitmapHelperTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
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

        BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidImageFormat_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);

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

        OverflowException? exception = Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));

        Assert.AreEqual("The image data generated an overflow during processing.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        const Rotation rotation = Rotation.Rotate90;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = (Rotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidImageFormat_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));

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
    public void LoadBitmapHeicThumbnailImage_ValidBufferAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int width, int height, Rotation expectedRotation, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
    
        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);
    
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height));

        Assert.AreEqual($"Value should not be negative. (Parameter '{exceptionParameter}')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 1000000, 1000000);

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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];
        const Rotation rotation = Rotation.Rotate90;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual("Value cannot be empty. (Parameter 'stream')", exception?.Message);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = [0x00, 0x01, 0x02, 0x03];
        const Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath!, rotation);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        const Rotation rotation = Rotation.Rotate90;

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath!, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation));

        Assert.AreEqual($"'{rotation}' is not a valid value for property 'Rotation'.", exception?.Message);
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);

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

        OverflowException? exception = Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));

        Assert.AreEqual("The image data generated an overflow during processing.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, 100, 100));

        Assert.AreEqual("Value cannot be null. (Parameter 'buffer')", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = [];

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidImageFormat_ThrowsArgumentException()
    {
        string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));

        Assert.AreEqual("No imaging component suitable to complete this operation was found.", exception?.Message);
    }

    [Test]
    [TestCase("Image 1.jpg", 1280, 720)]
    [TestCase("Image 8.jpeg", 1280, 720)]
    [TestCase("Image 10 portrait.png", 720, 1280)]
    [TestCase("Homer.gif", 320, 320)]
    [TestCase("Image_11.heic", 3024, 4032)]
    [TestCase("Image_11_90.heic", 4032, 3024)]
    public void LoadBitmapFromPath_ValidImagePath_ReturnsNonNullBitmap(string fileName, int expectedWidth, int expectedHeight)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.IsNotNull(bitmap);
        Assert.AreEqual(expectedWidth, bitmap!.Width);
        Assert.AreEqual(expectedHeight, bitmap.Height);

        AssertBrightnessValues(bitmap, 0, 0);
        AssertBrightnessValues(bitmap, 1, 0);
        AssertBrightnessValues(bitmap, 0, 1);
        AssertBrightnessValues(bitmap, 1, 1);
        AssertBrightnessValues(bitmap, 2, 5);
    }

    [Test]
    public void LoadBitmapFromPath_ImageDoesNotExist_ReturnsNull()
    {
        string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.png");

        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.IsNull(bitmap);
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsInvalid_ReturnsNull()
    {
        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(_dataDirectory!);

        Assert.IsNull(bitmap);
    }

    [Test]
    public void LoadBitmapFromPath_ImagePathIsNull_ReturnsNull()
    {
        string? imagePath = null;

        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(imagePath!);

        Assert.IsNull(bitmap);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetJpegBitmapImage_ValidImage_ReturnsJpegByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetJpegBitmapImage(image));

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetPngBitmapImage(image));

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        string filePath = Path.Combine(_dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.AreNotEqual(0, imageBuffer.Length);

        string destinationNewFileDirectory = Path.Combine(_dataDirectory!, "ImageConverted");

        try
        {
            Assert.IsTrue(ExifHelper.IsValidGDIPlusImage(imageBuffer));
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

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetGifBitmapImage(image));

        Assert.AreEqual("Operation is not valid due to the current state of the object.", exception?.Message);
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBitmapImage(invalidImage!));

        Assert.AreEqual("Value cannot be null. (Parameter 'source')", exception?.Message);
    }

    private static void AssertBrightnessValues(Bitmap bitmap, int x, int y)
    {
        Color pixelColor = bitmap.GetPixel(x, y);
        float brightness = pixelColor.GetBrightness();

        Assert.IsNotNull(brightness);
        Assert.Greater(brightness, 0);
        Assert.Less(brightness, 1);
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
