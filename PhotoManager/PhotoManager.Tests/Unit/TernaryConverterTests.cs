using FluentAssertions;
using PhotoManager.Domain;
using PhotoManager.UI.Converters;
using System.Globalization;
using Xunit;

namespace PhotoManager.Tests.Unit;

public class TernaryConverterTests
{
    [Theory]
    [InlineData(true, "Message", "Message")]
    [InlineData(false, null, null)]
    public void GetFormattedFileNameTest(bool assertion, string message, string expected)
    {
        TernaryConverter converter = new();
        Asset asset = new()
        {
            FileName = "toto",
            IsAssetCorrupted = assertion,
            AssetCorruptedMessage = message,
        };

        var converterParameters = new object[] { asset.IsAssetCorrupted, asset.AssetCorruptedMessage };

        string? result = (string?)converter.Convert(converterParameters, typeof(object[]), null, Thread.CurrentThread.CurrentCulture);
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertBackTest()
    {
        TernaryConverter converter = new();
        Func<object> function = () => converter.ConvertBack(null, null, null, new CultureInfo("en-US"));
        function.Should().Throw<NotImplementedException>();
    }
}
