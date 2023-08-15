using NUnit.Framework;
using PhotoManager.Common;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class BitmapHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var directoryName = Path.GetDirectoryName(typeof(BitmapHelperTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
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

        BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);

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

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapOriginalImage_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);

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

        Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapThumbnailImage_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicOriginalImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);

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

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, width, height));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        var image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 1000000, 1000000);

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

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicThumbnailImage_InvalidBuffer_ReturnsDefaultBitmapImage()
    {
        byte[] buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.AreEqual(0, image.DecodePixelWidth);
        Assert.AreEqual(0, image.DecodePixelHeight);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapImageFromPath(filePath!, rotation);

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

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));
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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath!, rotation);

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

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

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

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation));
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

        BitmapImage image = BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);

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

        Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 1000000, 1000000));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_EmptyBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>();

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapThumbnailImageAssetRepository_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, 100, 100));
    }

    [Test]
    [TestCase("Image 1.jpg", 1280, 720)]
    [TestCase("Image 8.jpeg", 1280, 720)]
    [TestCase("Image 10 portrait.png", 720, 1280)]
    [TestCase("Homer.gif", 320, 320)]
    [TestCase("Image_11.heic", 3024, 4032)]
    public void LoadBitmapFromPath_ValidImagePath_ReturnsNonNullBitmap(string fileName, int expectedWidth, int expectedHeight)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath);

        Assert.IsNotNull(bitmap);
        Assert.AreEqual(bitmap!.Width, expectedWidth);
        Assert.AreEqual(bitmap.Height, expectedHeight);

        AssertBrightnessValues(bitmap, 0, 0);
        AssertBrightnessValues(bitmap, 1, 0);
        AssertBrightnessValues(bitmap, 0, 1);
        AssertBrightnessValues(bitmap, 1, 1);
        AssertBrightnessValues(bitmap, 2, 5);
    }

    [Test]
    public void LoadBitmapFromPath_InvalidImagePath_ReturnsNull()
    {
        var filePath = Path.Combine(dataDirectory!, "invalid_path.png");

        Bitmap? bitmap = BitmapHelper.LoadBitmapFromPath(filePath!);

        Assert.IsNull(bitmap);
    }

    [Test]
    public void LoadBitmapFromPath_NullImagePath_ReturnsNull()
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
        var filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid JPEG image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.jpg", imageBuffer);
    }

    [Test]
    public void GetJpegBitmapImage_HeicValidImage_ReturnsJpegByteArray()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetJpegBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid JPEG image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.jpg", imageBuffer);
    }

    [Test]
    public void GetJpegBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetJpegBitmapImage(image));
    }

    [Test]
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBitmapImage(invalidImage!));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetPngBitmapImage_ValidImage_ReturnsPngByteArray(string fileName)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid PNG image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.png", imageBuffer);
    }

    [Test]
    public void GetPngBitmapImage_HeicValidImage_ReturnsPngByteArray()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetPngBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid PNG image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.png", imageBuffer);
    }

    [Test]
    public void GetPngBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetPngBitmapImage(image));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBitmapImage(invalidImage!));
    }

    [Test]
    [TestCase("Image 8.jpeg")]
    [TestCase("Image 1.jpg")]
    public void GetGifBitmapImage_ValidImage_ReturnsGifByteArray(string fileName)
    {
        var filePath = Path.Combine(dataDirectory!, fileName);
        BitmapImage image = new (new Uri(filePath));

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid GIF image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.gif", imageBuffer);
    }

    [Test]
    public void GetGifBitmapImage_HeicValidImage_ReturnsGifByteArray()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, Rotation.Rotate0, 100, 100);

        byte[] imageBuffer = BitmapHelper.GetGifBitmapImage(image);

        Assert.IsNotNull(imageBuffer);
        Assert.That(imageBuffer.Length, Is.Not.EqualTo(0));
        // Optionally, you can save the byte array to a file and verify that it's a valid GIF image.
        // For example:
        // File.WriteAllBytes("path/to/your/image_converted.gif", imageBuffer);
    }

    [Test]
    public void GetGifBitmapImage_InvalidImage_ThrowsInvalidOperationException()
    {
        BitmapImage image = new();

        Assert.Throws<InvalidOperationException>(() => BitmapHelper.GetGifBitmapImage(image));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBitmapImage(invalidImage!));
    }

    private void AssertBrightnessValues(Bitmap bitmap, int x, int y)
    {
        Color pixelColor = bitmap.GetPixel(x, y);
        float brightness = pixelColor.GetBrightness();

        Assert.IsNotNull(brightness);
        Assert.Greater(brightness, 0);
        Assert.Less(brightness, 1);
    }
}
