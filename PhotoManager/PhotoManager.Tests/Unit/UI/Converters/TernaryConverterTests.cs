using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class TernaryConverterTests
{
    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null, null)]
    public void Convert_AssetCorrupted_ReturnsValue(bool assertion, string message, string expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = new()
        {
            FileName = "toto",
            IsAssetCorrupted = assertion,
            AssetCorruptedMessage = message,
        };

        object[] converterParameters = [asset.IsAssetCorrupted, asset.AssetCorruptedMessage];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase(true, "Message", "Message")]
    [TestCase(false, null, null)]
    public void Convert_AssetRotated_ReturnsValue(bool assertion, string message, string expected)
    {
        TernaryConverter ternaryConverter = new();
        Asset asset = new()
        {
            FileName = "toto",
            IsAssetRotated = assertion,
            AssetRotatedMessage = message,
        };

        object[] converterParameters = [asset.IsAssetRotated, asset.AssetRotatedMessage];
        object? parameter = null;

        string? result = (string?)ternaryConverter.Convert(converterParameters, typeof(object[]), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        TernaryConverter ternaryConverter = new();
        object? value = null;
        Type[]? targetTypes = null;
        object? parameter = null;

        Assert.Throws<NotImplementedException>(() => ternaryConverter.ConvertBack(value!, targetTypes!, parameter!, CultureInfo.InvariantCulture));
    }
}
