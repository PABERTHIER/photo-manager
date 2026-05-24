using PhotoManager.UI.Models;
using System.Windows;
using System.Windows.Media;

namespace PhotoManager.Tests.Unit.UI;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class BitmapImageDataTests
{
    [Test]
    public void ToWpfRotation_InvalidRotation_ThrowsArgumentException()
    {
        const ImageRotation rotation = (ImageRotation)999;

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => BitmapImageData.ToWpfRotation(rotation));

        Assert.That(exception?.Message, Is.EqualTo("'999' is not a valid value for property 'Rotation'."));
    }

    [Test]
    [TestCase(ImageRotation.Rotate0, Rotation.Rotate0)]
    [TestCase(ImageRotation.Rotate90, Rotation.Rotate90)]
    [TestCase(ImageRotation.Rotate180, Rotation.Rotate180)]
    [TestCase(ImageRotation.Rotate270, Rotation.Rotate270)]
    public void ToWpfRotation_ValidRotation_ReturnsCorrectWpfRotation(ImageRotation input, Rotation expected)
    {
        Rotation result = BitmapImageData.ToWpfRotation(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MapFromWpfRotation_InvalidRotation_ReturnsRotate0()
    {
        const Rotation rotation = (Rotation)999;

        ImageRotation imageRotation = BitmapImageData.MapFromWpfRotation(rotation);

        Assert.That(imageRotation, Is.EqualTo(ImageRotation.Rotate0));
    }

    [Test]
    public void Constructor_BitmapImageWithRotate0_MapsToRotate0()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage();

        BitmapImageData imageData = new(bitmapImage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
        }
    }

    [Test]
    public void Constructor_BitmapImageWithRotate90_MapsToRotate90()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage(Rotation.Rotate90);

        BitmapImageData imageData = new(bitmapImage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate90));
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
        }
    }

    [Test]
    public void Constructor_BitmapImageWithRotate180_MapsToRotate180()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage(Rotation.Rotate180);

        BitmapImageData imageData = new(bitmapImage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate180));
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
        }
    }

    [Test]
    public void Constructor_BitmapImageWithRotate270_MapsToRotate270()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage(Rotation.Rotate270);

        BitmapImageData imageData = new(bitmapImage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate270));
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
        }
    }

    [Test]
    public void Constructor_BitmapImageWithInvalidRotation_DefaultsToRotate0()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage();

        BitmapImageData imageData = new(bitmapImage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate0));
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
        }
    }

    [Test]
    public void ToByteArray_JpegFormat_ReturnsByteArray()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage();
        BitmapImageData imageData = new(bitmapImage, ImageRotation.Rotate90);

        byte[] result = imageData.ToByteArray(ImageEncodingFormat.Jpeg);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Empty);
            Assert.That(imageData.BitmapImage, Is.SameAs(bitmapImage));
            Assert.That(imageData.Rotation, Is.EqualTo(ImageRotation.Rotate90));
        }
    }

    [Test]
    public void ToByteArray_PngFormat_ReturnsByteArray()
    {
        BitmapImageData imageData = new(CreateTestBitmapImage(), ImageRotation.Rotate0);

        byte[] result = imageData.ToByteArray(ImageEncodingFormat.Png);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void ToByteArray_GifFormat_ReturnsByteArray()
    {
        BitmapImageData imageData = new(CreateTestBitmapImage(), ImageRotation.Rotate0);

        byte[] result = imageData.ToByteArray(ImageEncodingFormat.Gif);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void ToByteArray_BmpFormat_ReturnsByteArray()
    {
        BitmapImageData imageData = new(CreateTestBitmapImage(), ImageRotation.Rotate0);

        byte[] result = imageData.ToByteArray(ImageEncodingFormat.Bmp);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void ToByteArray_InvalidFormat_ThrowsArgumentOutOfRangeException()
    {
        BitmapImageData imageData = new(CreateTestBitmapImage(), ImageRotation.Rotate0);

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            imageData.ToByteArray((ImageEncodingFormat)999));

        Assert.That(exception?.ParamName, Is.EqualTo("format"));
    }

    [Test]
    public void Width_ReturnsPixelWidth()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage(pixelWidth: 2, pixelHeight: 3);
        BitmapImageData imageData = new(bitmapImage, ImageRotation.Rotate0);

        Assert.That(imageData.Width, Is.EqualTo(bitmapImage.PixelWidth));
    }

    [Test]
    public void Height_ReturnsPixelHeight()
    {
        BitmapImage bitmapImage = CreateTestBitmapImage(pixelWidth: 2, pixelHeight: 3);
        BitmapImageData imageData = new(bitmapImage, ImageRotation.Rotate0);

        Assert.That(imageData.Height, Is.EqualTo(bitmapImage.PixelHeight));
    }

    private static BitmapImage CreateTestBitmapImage(Rotation rotation = Rotation.Rotate0, int pixelWidth = 1,
        int pixelHeight = 1)
    {
        WriteableBitmap writeableBitmap = new(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgra32, null);
        byte[] pixels = new byte[pixelWidth * pixelHeight * 4];

        for (int index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = 255;
            pixels[index + 1] = 0;
            pixels[index + 2] = 0;
            pixels[index + 3] = 255;
        }

        writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixels,
            pixelWidth * 4, 0);

        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

        using (MemoryStream memoryStream = new())
        {
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.Rotation = rotation;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
