using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class TernaryConverterTests
{
    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null!, null!)]
    public void Convert_AssetCorrupted_ReturnsValue(bool assertion, string message, string expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "toto.jpg",
            Hash = string.Empty,
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Metadata = new()
            {
                Corrupted = new() { IsTrue = assertion, Message = message },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };

        object[] converterParameters = [asset.Metadata.Corrupted.IsTrue, asset.Metadata.Corrupted.Message];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null!, null!)]
    public void Convert_AssetRotated_ReturnsValue(bool assertion, string message, string expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "toto.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            Hash = string.Empty,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = assertion, Message = message }
            }
        };

        object[] converterParameters = [asset.Metadata.Rotated.IsTrue, asset.Metadata.Rotated.Message];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        TernaryConverter ternaryConverter = new();
        object? value = null;
        Type[]? targetTypes = null;
        object? parameter = null;

        NotImplementedException? exception = Assert.Throws<NotImplementedException>(() => ternaryConverter.ConvertBack(value!, targetTypes!, parameter!, CultureInfo.InvariantCulture));

        Assert.That(exception?.Message, Is.EqualTo("The method or operation is not implemented."));
    }
}
