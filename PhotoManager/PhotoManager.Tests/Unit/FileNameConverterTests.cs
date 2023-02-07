using FluentAssertions;
using PhotoManager.Domain;
using PhotoManager.UI.Converters;
using System.Globalization;
using Xunit;

namespace PhotoManager.Tests.Unit;

public class FileNameConverterTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("toto", "toto")]
    [InlineData("_toto", "__toto")] // Always adding an extra _ because XAML remove it thinking it is an event
    [InlineData("_toto_toto_toto_toto", "__toto_toto_toto_toto")] // Always adding an extra _ because XAML remove it thinking it is an event
    [InlineData("_____toto_toto_toto_toto", "______toto_toto_toto_toto")] // Always adding an extra _ because XAML remove it thinking it is an event
    public void GetFormattedFileNameTest(string name, string expected)
    {
        FileNameConverter converter = new();
        Asset asset = new()
        {
            FileName = name,
        };

        string result = (string)converter.Convert(asset.FileName, typeof(Asset), null, Thread.CurrentThread.CurrentCulture);
        result.Should().Be(expected);
    }

    [Fact]
    public void ConvertBackTest()
    {
        FileNameConverter converter = new();
        Func<object> function = () => converter.ConvertBack("toto", typeof(string), null, new CultureInfo("en-US"));
        function.Should().Throw<NotImplementedException>();
    }
}
