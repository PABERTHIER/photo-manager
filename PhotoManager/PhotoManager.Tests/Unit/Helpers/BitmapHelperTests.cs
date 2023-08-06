using NUnit.Framework;
using PhotoManager.Common;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.Tests.Unit.Helpers;

[TestFixture]
public class BitmapHelperTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var directoryName = Path.GetDirectoryName(typeof(ApplicationTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    [TestCase(Rotation.Rotate0)]
    [TestCase(Rotation.Rotate90)]
    [TestCase(Rotation.Rotate180)]
    [TestCase(Rotation.Rotate270)]
    [TestCase(null)]
    public void LoadBitmapImageAsset_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation)
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(rotation));
        Assert.IsNotNull(image.Width);
        Assert.IsNotNull(image.Height);
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapImageAsset_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapImageAsset_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>(); // An empty buffer, which is considered invalid.
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapImageAsset_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage")]
    public void LoadBitmapImageAsset_InvalidImageFormat_ThrowsArgumentException()
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
    public void LoadBitmapImageThumbnail_ValidBufferAndRotationAndWidthAndHeight_ReturnsBitmapImage(Rotation rotation, int width, int height)
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
    public void LoadBitmapImageThumbnail_LargeWidthAndHeight_ThrowsOverflowException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, Rotation.Rotate0, 1000000, 1000000));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapImageThumbnail_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapImageThumbnail_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>(); // An empty buffer, which is considered invalid.
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapImageThumbnail_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage")]
    public void LoadBitmapImageThumbnail_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    [TestCase(Rotation.Rotate0)]
    [TestCase(Rotation.Rotate90)]
    [TestCase(Rotation.Rotate180)]
    [TestCase(Rotation.Rotate270)]
    [TestCase(null)]
    public void LoadBitmapHeicImage_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation)
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
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicImage_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer!, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicImage_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>(); // An empty buffer, which is considered invalid.
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicOriginalImage(buffer, rotation));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicImage_InvalidRotation_ReturnsPartialBitmapImage()
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
    [Category("From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC")]
    public void LoadBitmapHeicImage_CorruptedHeicImageBuffer_ReturnsDefaultBitmapImage()
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
    public void LoadBitmapHeicImageThumbnail_ValidBufferAndRotation_ReturnsBitmapImage(Rotation rotation, int width, int height)
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
    public void LoadBitmapHeicImageThumbnail_InvalidWidthOrHeightOrBoth_ThrowsArgumentException(int width, int height)
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, width, height));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicImageThumbnail_LargeWidthAndHeight_ReturnsDefaultBitmapImage()
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
    public void LoadBitmapHeicImageThumbnail_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer!, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicImageThumbnail_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>(); // An empty buffer, which is considered invalid.
        Rotation rotation = Rotation.Rotate90;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, 100, 100));
    }

    [Test]
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicImageThumbnail_InvalidRotation_ReturnsPartialBitmapImage()
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
    [Category("From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC")]
    public void LoadBitmapHeicImageThumbnail_CorruptedHeicImageBuffer_ReturnsDefaultBitmapImage()
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
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    [TestCase(Rotation.Rotate0)]
    [TestCase(Rotation.Rotate90)]
    [TestCase(Rotation.Rotate180)]
    [TestCase(Rotation.Rotate270)]
    [TestCase(null)]
    public void LoadBitmapImageAssetViewerUserControl_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation)
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
    public void LoadBitmapImageAssetViewerUserControl_FileNotExists_ReturnsDefaultBitmapImage()
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
    public void LoadBitmapImageAssetViewerUserControl_InvalidRotation_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        Rotation rotation = (Rotation)999;

        Assert.Throws<ArgumentException>(() => BitmapHelper.LoadBitmapImageFromPath(filePath, rotation));
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode")]
    public void LoadBitmapImageAssetViewerUserControl_InvalidImageFormat_ThrowsArgumentException()
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
    public void LoadBitmapHeicImageViewerUserControl_ValidPathAndRotation_ReturnsBitmapImage(Rotation rotation)
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
    public void LoadBitmapHeicImageViewerUserControl_NullBuffer_ThrowsArgumentNullException()
    {
        var filePath = Path.Combine(dataDirectory!, "Invalid.jpg");
        Rotation rotation = Rotation.Rotate90;

        BitmapImage image = BitmapHelper.LoadBitmapHeicImageFromPath(filePath, rotation);

        Assert.IsNotNull(image);
        Assert.IsNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.IsNotNull(image.DecodePixelWidth);
        Assert.IsNotNull(image.DecodePixelHeight);
    }

    [Test]
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageViewerUserControl_InvalidRotation_ReturnsPartialBitmapImage()
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
    [Category("From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic")]
    public void LoadBitmapHeicImageViewerUserControl_CorruptedHeicImageBuffer_ReturnsDefaultBitmapImage()
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
    public void LoadBitmapImageThumbnailAssetRepository_ValidBufferAndWidthAndHeight_ReturnsBitmapImage(int width, int height)
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        BitmapImage image = BitmapHelper.LoadBitmapImage(buffer, width, height);

        Assert.IsNotNull(image);
        Assert.IsNotNull(image.StreamSource);
        Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
        Assert.That(image.DecodePixelWidth, Is.EqualTo(width));
        Assert.That(image.DecodePixelHeight, Is.EqualTo(height));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapImageThumbnailAssetRepository_LargeWidthAndHeight_ThrowsOverflowException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image 1.jpg");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<OverflowException>(() => BitmapHelper.LoadBitmapImage(buffer, 1000000, 1000000));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapImageThumbnailAssetRepository_NullBuffer_ThrowsArgumentNullException()
    {
        byte[]? buffer = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.LoadBitmapImage(buffer!, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapImageThumbnailAssetRepository_InvalidBuffer_ThrowsArgumentException()
    {
        byte[] buffer = Array.Empty<byte>(); // An empty buffer, which is considered invalid.

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapImage(buffer, 100, 100));
    }

    [Test]
    [Category("From AssetRepository")]
    public void LoadBitmapImageThumbnailAssetRepository_InvalidImageFormat_ThrowsArgumentException()
    {
        var filePath = Path.Combine(dataDirectory!, "Image_11.heic");
        byte[] buffer = File.ReadAllBytes(filePath);

        Assert.Throws<NotSupportedException>(() => BitmapHelper.LoadBitmapImage(buffer, 100, 100));
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

    // For example:
    // - Test with an invalid image

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
    public void GetJpegBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetJpegBitmapImage(invalidImage!));
    }

    [Test]
    public void GetPngBitmapImage_NullImage_ThrowsArgumentNullException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetPngBitmapImage(invalidImage!));
    }

    [Test]
    public void GetGifBitmapImage_NullImage_ThrowsArgumentException()
    {
        BitmapImage? invalidImage = null;

        Assert.Throws<ArgumentNullException>(() => BitmapHelper.GetGifBitmapImage(invalidImage!));
    }
}
