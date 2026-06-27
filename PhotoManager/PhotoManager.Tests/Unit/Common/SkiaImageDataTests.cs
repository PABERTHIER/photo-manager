using SkiaSharp;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Common;

[TestFixture]
public class SkiaImageDataTests
{
    private string? _assetsDirectory;

    private TestLogger<SkiaImageData>? _testLogger;

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
    public void Constructor_NullBitmap_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SkiaImageData(null!, ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void Constructor_ValidBitmap_SetsProperties()
    {
        using (SKBitmap bitmap = new(10, 20))
        {
            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate90))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.Width, Is.EqualTo(10));
                    Assert.That(imageData.Height, Is.EqualTo(20));
                    Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate90));
                    Assert.That(imageData.Bitmap, Is.EqualTo(bitmap));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void ToByteArray_JpegFormat_ReturnsByteArray()
    {
        using (SkiaImageData imageData = CreateTestImageData())
        {
            byte[] result = imageData.ToByteArray(ImageEncodingFormat.Jpeg);

            Assert.That(result, Is.Not.Empty);

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void ToByteArray_PngFormat_ReturnsByteArray()
    {
        using (SkiaImageData imageData = CreateTestImageData())
        {
            byte[] result = imageData.ToByteArray(ImageEncodingFormat.Png);

            Assert.That(result, Is.Not.Empty);

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void ToByteArray_GifFormat_ReturnsByteArray()
    {
        using (SkiaImageData imageData = CreateTestImageData())
        {
            byte[] result = imageData.ToByteArray(ImageEncodingFormat.Gif);

            Assert.That(result, Is.Not.Empty);

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void ToByteArray_BmpFormat_ReturnsByteArray()
    {
        using (SkiaImageData imageData = CreateTestImageData())
        {
            byte[] result = imageData.ToByteArray(ImageEncodingFormat.Bmp);

            Assert.That(result, Is.Not.Empty);

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    [TestCase(ImageEncodingFormat.Gif)]
    [TestCase(ImageEncodingFormat.Bmp)]
    public void ToByteArray_GifOrBmpFromRgbaBitmap_PreservesColorChannels(ImageEncodingFormat format)
    {
        using (SKBitmap bitmap = new(new SKImageInfo(2, 2, SKColorType.Rgba8888, SKAlphaType.Premul)))
        {
            bitmap.Erase(SKColors.Red);

            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                byte[] result = imageData.ToByteArray(format);

                using (SKBitmap? decoded = SKBitmap.Decode(result))
                {
                    Assert.That(decoded, Is.Not.Null);
                    SKColor pixel = decoded!.GetPixel(0, 0);

                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(pixel.Red, Is.GreaterThan(200));
                        Assert.That(pixel.Blue, Is.LessThan(50));
                    }
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void ToByteArray_InvalidFormat_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using (SkiaImageData imageData = CreateTestImageData())
            {
                imageData.ToByteArray((ImageEncodingFormat)999);
            }
        });

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void ToByteArray_EmptyBitmap_ReturnsEmptyArray()
    {
        using (SKBitmap emptyBitmap = new())
        {
            using (SkiaImageData imageData = new(emptyBitmap, ImageRotation.Rotate0))
            {
                byte[] result = imageData.ToByteArray(ImageEncodingFormat.Jpeg);

                Assert.That(result, Is.Empty);

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void ToByteArray_DisposedInstance_ThrowsObjectDisposedException()
    {
        SkiaImageData imageData = CreateTestImageData();
        imageData.Dispose();

        Assert.Throws<ObjectDisposedException>(() => imageData.ToByteArray(ImageEncodingFormat.Jpeg));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void ToByteArray_JpegWithUnsupportedColorType_ReturnsEmptyArray()
    {
        // Rg88 color type causes SKImage.Encode to return null for JPEG
        using (SKBitmap bitmap = new(2, 2, SKColorType.Rg88, SKAlphaType.Premul))
        {
            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                byte[] result = imageData.ToByteArray(ImageEncodingFormat.Jpeg);

                Assert.That(result, Is.Empty);

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void ToByteArray_PngWithUnsupportedColorType_ReturnsEmptyArray()
    {
        // Rg88 color type causes SKImage.Encode to return null for PNG
        using (SKBitmap bitmap = new(2, 2, SKColorType.Rg88, SKAlphaType.Premul))
        {
            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                byte[] result = imageData.ToByteArray(ImageEncodingFormat.Png);

                Assert.That(result, Is.Empty);

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void ToStream_JpegFormat_ReturnsReadableStream()
    {
        using (SkiaImageData imageData = CreateTestImageData())
        {
            using (Stream result = imageData.ToStream(ImageEncodingFormat.Jpeg))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result.CanRead, Is.True);
                    Assert.That(result.Length, Is.GreaterThan(0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void Dispose_CalledOnce_DisposesResources()
    {
        SkiaImageData imageData = CreateTestImageData();

        Assert.DoesNotThrow(imageData.Dispose);

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        SkiaImageData imageData = CreateTestImageData();

        imageData.Dispose();

        Assert.DoesNotThrow(imageData.Dispose);

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_ValidJpegBuffer_ReturnsImageData()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData imageData = SkiaImageData.FromEncodedBytes(buffer, ImageRotation.Rotate90, _testLogger!))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(1280));
                Assert.That(imageData.Height, Is.EqualTo(720));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate90));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromEncodedBytes_NullBuffer_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() =>
            SkiaImageData.FromEncodedBytes(null!, ImageRotation.Rotate0, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_EmptyBuffer_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            SkiaImageData.FromEncodedBytes([], ImageRotation.Rotate0, _testLogger!));

        _testLogger!.AssertLogExceptions(
            [new ArgumentException("Value cannot be empty. (Parameter 'buffer')")], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_InvalidBuffer_ThrowsException()
    {
        byte[] invalidBuffer = [0x00, 0x01, 0x02, 0x03];

        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromEncodedBytes(invalidBuffer, ImageRotation.Rotate0, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_WithResize_ReturnsResizedImageData()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData imageData = SkiaImageData.FromEncodedBytes(buffer, ImageRotation.Rotate0, 100, 100,
                   _testLogger!))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(100));
                Assert.That(imageData.Height, Is.EqualTo(100));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromEncodedBytes_WithResizeNullBuffer_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() =>
            SkiaImageData.FromEncodedBytes(null!, ImageRotation.Rotate0, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_WithResizeEmptyBuffer_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            SkiaImageData.FromEncodedBytes([], ImageRotation.Rotate0, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions(
            [new ArgumentException("Value cannot be empty. (Parameter 'buffer')")], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_WithResizeCausingZeroWidth_ThrowsNotSupportedException()
    {
        // Create a 1x200 image so that CalculateTargetDimensions(0, 1, 1, 200) yields width = 1*1/200 = 0
        byte[] buffer = CreateNarrowTallImageBuffer(1, 200);

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() =>
            SkiaImageData.FromEncodedBytes(buffer, ImageRotation.Rotate0, 0, 1, _testLogger!))!;

        Assert.That(exception.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytes_WithResizeInvalidBuffer_ThrowsException()
    {
        byte[] invalidBuffer = [0x00, 0x01, 0x02, 0x03];

        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromEncodedBytes(invalidBuffer, ImageRotation.Rotate0, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_Rotate0_ReturnsUnrotatedImage()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData imageData = SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate0,
                   _testLogger!))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(1280));
                Assert.That(imageData.Height, Is.EqualTo(720));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromEncodedBytesWithRotation_Rotate90_ReturnsRotatedImageWithRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData original = SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate0,
                   _testLogger!))
        {
            using (SkiaImageData rotated = SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate90,
                       _testLogger!))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rotated.Width, Is.EqualTo(original.Height));
                    Assert.That(rotated.Height, Is.EqualTo(original.Width));
                    Assert.That(rotated.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void FromEncodedBytesWithRotation_NullBuffer_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation(null!, ImageRotation.Rotate0, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_EmptyBuffer_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation([], ImageRotation.Rotate0, _testLogger!));

        _testLogger!.AssertLogExceptions(
            [new ArgumentException("Value cannot be empty. (Parameter 'buffer')")], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_WithResize_ReturnsResizedRotatedImageWithRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData imageData =
               SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate90, 200, 150, _testLogger!))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(200));
                Assert.That(imageData.Height, Is.EqualTo(150));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromEncodedBytesWithRotation_RotatedThumbnailResize_KeepsRequestedDimensionsAndRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_1_JPG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (SkiaImageData imageData =
               SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate90, 84, 150, _testLogger!))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(84));
                Assert.That(imageData.Height, Is.EqualTo(150));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromEncodedBytesWithRotation_WithResizeNullBuffer_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation(null!, ImageRotation.Rotate0, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_WithResizeEmptyBuffer_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation([], ImageRotation.Rotate0, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions(
            [new ArgumentException("Value cannot be empty. (Parameter 'buffer')")], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_InvalidBuffer_ThrowsException()
    {
        byte[] invalidBuffer = [0x00, 0x01, 0x02, 0x03];

        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation(invalidBuffer, ImageRotation.Rotate90, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_WithResizeInvalidBuffer_ThrowsException()
    {
        byte[] invalidBuffer = [0x00, 0x01, 0x02, 0x03];

        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation(invalidBuffer, ImageRotation.Rotate90, 100, 100, _testLogger!));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedBytesWithRotation_WithResizeCausingZeroWidth_ThrowsNotSupportedException()
    {
        // Create a 1x200 image so that CalculateTargetDimensions(0, 1, 1, 200) yields width = 0
        byte[] buffer = CreateNarrowTallImageBuffer(1, 200);

        NotSupportedException exception = Assert.Throws<NotSupportedException>(() =>
            SkiaImageData.FromEncodedBytesWithRotation(buffer, ImageRotation.Rotate0, 0, 1, _testLogger!))!;

        Assert.That(exception.Message,
            Is.EqualTo("No imaging component suitable to complete this operation was found."));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromBitmapWithRotation_Rotate0_ReturnsSameDimensions()
    {
        SKBitmap bitmap = new(100, 50);

        using (SkiaImageData imageData = SkiaImageData.FromBitmapWithRotation(bitmap, ImageRotation.Rotate0))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(100));
                Assert.That(imageData.Height, Is.EqualTo(50));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromBitmapWithRotation_Rotate90_ReturnsSwappedDimensionsWithRotate0()
    {
        SKBitmap bitmap = new(100, 50);

        using (SkiaImageData imageData = SkiaImageData.FromBitmapWithRotation(bitmap, ImageRotation.Rotate90))
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData.Width, Is.EqualTo(50));
                Assert.That(imageData.Height, Is.EqualTo(100));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void FromBitmapWithRotation_NullBitmap_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromBitmapWithRotation(null!, ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void Empty_ReturnsNonNullInstance()
    {
        using (SkiaImageData imageData = SkiaImageData.Empty())
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(imageData, Is.Not.Null);
                Assert.That(imageData.Width, Is.EqualTo(1));
                Assert.That(imageData.Height, Is.EqualTo(1));
                Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            }

            _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
        }
    }

    [Test]
    public void GetPixelBrightness_ValidCoordinates_ReturnsBrightnessValue()
    {
        using (SKBitmap bitmap = new(2, 2))
        {
            bitmap.SetPixel(0, 0, new SKColor(255, 255, 255)); // White = brightness 1.0
            bitmap.SetPixel(1, 0, new SKColor(0, 0, 0));       // Black = brightness 0.0

            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.GetPixelBrightness(0, 0), Is.EqualTo(1.0f));
                    Assert.That(imageData.GetPixelBrightness(1, 0), Is.Zero);
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void GetPixelBrightness_OutOfBoundsCoordinates_ReturnsZero()
    {
        using (SKBitmap bitmap = new(2, 2))
        {
            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.GetPixelBrightness(-1, 0), Is.Zero);
                    Assert.That(imageData.GetPixelBrightness(0, -1), Is.Zero);
                    Assert.That(imageData.GetPixelBrightness(2, 0), Is.Zero);
                    Assert.That(imageData.GetPixelBrightness(0, 2), Is.Zero);
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void GetPixelBrightness_DisposedInstance_ThrowsObjectDisposedException()
    {
        SkiaImageData imageData = CreateTestImageData();
        imageData.Dispose();

        Assert.Throws<ObjectDisposedException>(() => imageData.GetPixelBrightness(0, 0));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedStreamWithRotation_Rotate0_ReturnsUnrotatedImageWithRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (MemoryStream stream = new(buffer))
        {
            using (SkiaImageData imageData = SkiaImageData.FromEncodedStreamWithRotation(stream, ImageRotation.Rotate0))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.Width, Is.EqualTo(PixelWidthAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Height, Is.EqualTo(PixelHeightAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void FromEncodedStreamWithRotation_Rotate90_ReturnsRotatedImageWithSwappedDimensionsAndRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (MemoryStream stream = new(buffer))
        {
            using (SkiaImageData imageData =
                   SkiaImageData.FromEncodedStreamWithRotation(stream, ImageRotation.Rotate90))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.Width, Is.EqualTo(PixelHeightAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Height, Is.EqualTo(PixelWidthAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void FromEncodedStreamWithRotation_Rotate180_ReturnsSameDimensionsWithRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (MemoryStream stream = new(buffer))
        {
            using (SkiaImageData imageData =
                   SkiaImageData.FromEncodedStreamWithRotation(stream, ImageRotation.Rotate180))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.Width, Is.EqualTo(PixelWidthAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Height, Is.EqualTo(PixelHeightAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void FromEncodedStreamWithRotation_Rotate270_ReturnsRotatedImageWithSwappedDimensionsAndRotate0()
    {
        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_8_JPEG);
        byte[] buffer = File.ReadAllBytes(filePath);

        using (MemoryStream stream = new(buffer))
        {
            using (SkiaImageData imageData =
                   SkiaImageData.FromEncodedStreamWithRotation(stream, ImageRotation.Rotate270))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(imageData.Width, Is.EqualTo(PixelHeightAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Height, Is.EqualTo(PixelWidthAsset.IMAGE_8_JPEG));
                    Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
                }

                _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
            }
        }
    }

    [Test]
    public void FromEncodedStreamWithRotation_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SkiaImageData.FromEncodedStreamWithRotation(null!, ImageRotation.Rotate0));

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    [Test]
    public void FromEncodedStreamWithRotation_InvalidStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            using (MemoryStream stream = new([0x00, 0x01, 0x02, 0x03]))
            {
                SkiaImageData.FromEncodedStreamWithRotation(stream, ImageRotation.Rotate0);
            }
        });

        _testLogger!.AssertLogExceptions([], typeof(SkiaImageData));
    }

    private static SkiaImageData CreateTestImageData()
    {
        SKBitmap bitmap = new(10, 10);

        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Red);
        }

        return new SkiaImageData(bitmap, ImageRotation.Rotate0);
    }

    private static byte[] CreateNarrowTallImageBuffer(int width, int height)
    {
        using (SKBitmap bitmap = new(width, height))
        {
            using (SKCanvas canvas = new(bitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            using (SKImage image = SKImage.FromBitmap(bitmap))
            {
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }
        }
    }
}
