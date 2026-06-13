using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class ImageDataConverterTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
    }

    [Test]
    public void Convert_IImageDataThatEncodesToEmptyBytes_ReturnsNull()
    {
        ImageDataConverter converter = new();

        using (IImageData imageData = Substitute.For<IImageData>())
        {
            imageData.ToByteArray(ImageEncodingFormat.Png).Returns([]);

            object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public void Convert_NullValue_ReturnsNull()
    {
        ImageDataConverter converter = new();

        object? result = converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_NonImageDataValue_ReturnsValueUnchanged()
    {
        ImageDataConverter converter = new();
        const string value = "hello";

        object? result = converter.Convert(value, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.SameAs(value));
    }

    [Test]
    public void ConvertBack_Always_ThrowsNotSupportedException()
    {
        ImageDataConverter converter = new();

        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Convert_SkiaImageData_ReturnsBitmap()
    {
        ImageDataConverter converter = new();

        using (SKBitmap bitmap = new(10, 10))
        {
            using (SKCanvas canvas = new(bitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            using (SkiaImageData imageData = new(bitmap, ImageRotation.Rotate0))
            {
                object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result, Is.TypeOf<Bitmap>());
                }

                ((Bitmap)result!).Dispose();
            }
        }
    }

    [Test]
    [TestCase(ImageRotation.Rotate90)]
    [TestCase(ImageRotation.Rotate180)]
    [TestCase(ImageRotation.Rotate270)]
    public void Convert_RotatedSkiaImageData_ReturnsBitmapWithRotatedDimensions(ImageRotation rotation)
    {
        ImageDataConverter converter = new();

        using (SKBitmap bitmap = new(10, 20))
        {
            using (SKCanvas canvas = new(bitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            int expectedWidth = rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270 ? 20 : 10;
            int expectedHeight = rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270 ? 10 : 20;

            using (SkiaImageData imageData = SkiaImageData.FromBitmapWithRotation(bitmap, rotation))
            {
                object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.TypeOf<Bitmap>());
                    Bitmap avaloniaBitmap = (Bitmap)result!;
                    Assert.That(avaloniaBitmap.PixelSize.Width, Is.EqualTo(expectedWidth));
                    Assert.That(avaloniaBitmap.PixelSize.Height, Is.EqualTo(expectedHeight));
                    avaloniaBitmap.Dispose();
                }
            }
        }
    }

    [Test]
    public void Convert_SkiaImageDataWithInvalidRotation_ReturnsBitmap()
    {
        ImageDataConverter converter = new();

        using (SKBitmap bitmap = new(10, 10))
        {
            using (SKCanvas canvas = new(bitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            using (SkiaImageData imageData = new(bitmap, (ImageRotation)999))
            {
                object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.TypeOf<Bitmap>());
                }

                ((Bitmap)result!).Dispose();
            }
        }
    }

    [Test]
    public void Convert_SkiaImageDataEmptyBitmap_ReturnsNull()
    {
        ImageDataConverter converter = new();

        using (SKBitmap emptyBitmap = new())
        {
            using (SkiaImageData imageData = new(emptyBitmap, ImageRotation.Rotate0))
            {
                object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

                Assert.That(result, Is.Null);
            }
        }
    }
}
