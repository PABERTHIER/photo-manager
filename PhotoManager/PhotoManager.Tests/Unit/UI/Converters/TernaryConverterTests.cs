using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class TernaryConverterTests
{
    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null, null)]
    public void Convert_AssetCorrupted_ReturnsValue(bool assertion, string? message, string? expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" })
            .WithFileName("toto.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(default)
            .WithHash(string.Empty)
            .WithCorrupted(assertion, message)
            .WithRotated(false, null)
            .Build();

        object?[] converterParameters = [asset.Metadata.Corrupted.IsTrue, asset.Metadata.Corrupted.Message];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null, null)]
    public void Convert_AssetRotated_ReturnsValue(bool assertion, string? message, string? expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" })
            .WithFileName("toto.jpg")
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(1280, 720, 200, 112)
            .WithFileSize(0)
            .WithThumbnailCreationDateTime(default)
            .WithHash(string.Empty)
            .WithCorrupted(false, null)
            .WithRotated(assertion, message)
            .Build();

        object?[] converterParameters = [asset.Metadata.Rotated.IsTrue, asset.Metadata.Rotated.Message];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_EmptyValues_ReturnsNull()
    {
        TernaryConverter ternaryConverter = new();

        object[] converterParameters = [];

        object? result = ternaryConverter.Convert(converterParameters, typeof(object[]), null,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_SingleValue_ReturnsNull()
    {
        TernaryConverter ternaryConverter = new();

        object[] converterParameters = [true];

        object? result = ternaryConverter.Convert(converterParameters, typeof(object[]), null,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_FirstValueIsNotBoolean_ReturnsNull()
    {
        TernaryConverter ternaryConverter = new();

        object[] converterParameters = ["notABoolean", "Message"];

        object? result = ternaryConverter.Convert(converterParameters, typeof(object[]), null,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_NullValues_ReturnsNull()
    {
        TernaryConverter ternaryConverter = new();

        object? result = ternaryConverter.Convert(null!, typeof(object[]), null!,
            CultureInfo.InvariantCulture);

        Assert.That(result, Is.Null);
    }

}
