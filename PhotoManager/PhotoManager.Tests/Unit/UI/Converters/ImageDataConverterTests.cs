using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
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
}
