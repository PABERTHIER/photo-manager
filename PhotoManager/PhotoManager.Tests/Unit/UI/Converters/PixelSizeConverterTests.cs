using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class PixelSizeConverterTests
{
    [Test]
    [TestCase(1920, 1080, "1920x1080 pixels")]
    [TestCase(1024, 768, "1024x768 pixels")]
    public void Convert_AssetWithDifferentPixelsSize_ReturnsResolution(int width, int height, string expected)
    {
        PixelSizeConverter pixelSizeConverter = new();
        Asset asset = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = "toto.jpg",
            Pixel = new()
            {
                Asset = new() { Width = width, Height = height }
            },
            Hash = string.Empty
        };
        object? parameter = null;

        string? result = (string?)pixelSizeConverter.Convert(asset, typeof(Asset), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_NullInput_ReturnsEmptyString()
    {
        PixelSizeConverter pixelSizeConverter = new();
        object? input = null;
        object? parameter = null;

        object? result = pixelSizeConverter.Convert(input!, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void Convert_NonAssetValue_ReturnsEmptyString()
    {
        PixelSizeConverter pixelSizeConverter = new();
        const string input = "Not an Asset object";
        object? parameter = null;

        object? result = pixelSizeConverter.Convert(input, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        PixelSizeConverter pixelSizeConverter = new();
        object? parameter = null;

        NotImplementedException? exception = Assert.Throws<NotImplementedException>(() => pixelSizeConverter.ConvertBack("1920x1080 pixels", typeof(string), parameter!, CultureInfo.InvariantCulture));

        Assert.That(exception?.Message, Is.EqualTo("The method or operation is not implemented."));
    }
}
