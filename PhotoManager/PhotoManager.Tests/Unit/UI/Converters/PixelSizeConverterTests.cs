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
            PixelWidth = width,
            PixelHeight = height
        };
        object? parameter = null;

        string result = (string)pixelSizeConverter.Convert(asset, typeof(Asset), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Convert_NullInput_ReturnsEmptyString()
    {
        PixelSizeConverter pixelSizeConverter = new();
        object? input = null;
        object? parameter = null;

        var result = pixelSizeConverter.Convert(input!, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual("", result);
    }

    [Test]
    public void Convert_NonAssetValue_ReturnsEmptyString()
    {
        PixelSizeConverter pixelSizeConverter = new();
        string input = "Not an Asset object";
        object? parameter = null;

        var result = pixelSizeConverter.Convert(input, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual("", result);
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        PixelSizeConverter pixelSizeConverter = new();
        object? parameter = null;

        Assert.Throws<NotImplementedException>(() => pixelSizeConverter.ConvertBack("1920x1080 pixels", typeof(string), parameter!, CultureInfo.InvariantCulture));
    }
}
