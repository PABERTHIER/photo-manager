using PhotoManager.UI.Models;
using SkiaSharp;
using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class ImageDataConverterTests
{
    [Test]
    public void Convert_BitmapImageData_ReturnsBitmapImage()
    {
        ImageDataConverter converter = new();
        BitmapImage bitmapImage = new();
        BitmapImageData imageData = new(bitmapImage, ImageRotation.Rotate0);

        object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.SameAs(bitmapImage));
    }

    [Test]
    public void Convert_IImageDataNotBitmapImageData_ReturnsNull()
    {
        ImageDataConverter converter = new();
        IImageData imageData = Substitute.For<IImageData>();

        object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
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
    public void Convert_SkiaImageData_ReturnsBitmapImage()
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
                    Assert.That(result, Is.TypeOf<BitmapImage>());
                }
            }
        }
    }

    [Test]
    [TestCase(ImageRotation.Rotate90, Rotation.Rotate90)]
    [TestCase(ImageRotation.Rotate180, Rotation.Rotate180)]
    [TestCase(ImageRotation.Rotate270, Rotation.Rotate270)]
    public void Convert_RotatedSkiaImageData_ReturnsBitmapImageWithRotation(ImageRotation rotation,
        Rotation expectedRotation)
    {
        ImageDataConverter converter = new();

        using (SKBitmap bitmap = new(10, 10))
        {
            using (SKCanvas canvas = new(bitmap))
            {
                canvas.Clear(SKColors.Blue);
            }

            using (SkiaImageData imageData = new(bitmap, rotation))
            {
                object? result = converter.Convert(imageData, typeof(object), null, CultureInfo.InvariantCulture);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.TypeOf<BitmapImage>());
                    Assert.That(((BitmapImage)result!).Rotation, Is.EqualTo(expectedRotation));
                }
            }
        }
    }

    [Test]
    public void Convert_SkiaImageDataWithInvalidRotation_ReturnsBitmapImageWithRotate0()
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
                    Assert.That(result, Is.TypeOf<BitmapImage>());
                    Assert.That(((BitmapImage)result!).Rotation, Is.EqualTo(Rotation.Rotate0));
                }
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
