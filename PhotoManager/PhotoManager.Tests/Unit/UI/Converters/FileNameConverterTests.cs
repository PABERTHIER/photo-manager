using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class FileNameConverterTests
{
    [Test]
    [TestCase("", "")]
    [TestCase("example.jpg", "example.jpg")]
    [TestCase("_example.jpg", "__example.jpg")] // Always adding an extra _ per _ because XAML remove it thinking it is an event
    [TestCase("_toto_toto_toto_toto.jpg", "__toto__toto__toto__toto.jpg")] // Always adding an extra _ per _ because XAML remove it thinking it is an event
    [TestCase("_____toto_toto_toto_toto.jpg", "__________toto__toto__toto__toto.jpg")] // Always adding an extra _ per _ because XAML remove it thinking it is an event
    [TestCase("toto_tutu", "toto__tutu")] // Always adding an extra _ per _ because XAML remove it thinking it is an event
    public void Convert_InputStringNotNull_ReturnsFormattedFileName(string name, string expected)
    {
        FileNameConverter fileNameConverter = new();
        Asset asset = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = name,
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Hash = string.Empty
        };

        object? parameter = null;

        string result = (string)fileNameConverter.Convert(asset.FileName, typeof(string), parameter!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_InputStringNull_ReturnsNull()
    {
        FileNameConverter fileNameConverter = new();
        string? fileName = null;
        object? parameter = null;

        object result = fileNameConverter.Convert(fileName!, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_InputValueType_ReturnsInputValue()
    {
        FileNameConverter fileNameConverter = new();
        const int input = 123;
        object? parameter = null;

        object result = fileNameConverter.Convert(input, typeof(int), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        FileNameConverter fileNameConverter = new();
        Type? targetType = null;

        NotImplementedException? exception = Assert.Throws<NotImplementedException>(() => fileNameConverter.ConvertBack("toto.jpg", typeof(string), targetType!, CultureInfo.InvariantCulture));

        Assert.That(exception?.Message, Is.EqualTo("The method or operation is not implemented."));
    }
}
