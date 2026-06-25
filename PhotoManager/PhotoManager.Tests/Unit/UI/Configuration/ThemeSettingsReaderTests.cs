using Avalonia.Styling;
using PhotoManager.UI.Configuration;

namespace PhotoManager.Tests.Unit.UI.Configuration;

[TestFixture]
public class ThemeSettingsReaderTests
{
    [Test]
    public void GetRequestedThemeVariant_NullThemeMode_ReturnsDefault()
    {
        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(null);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void GetRequestedThemeVariant_WhitespaceThemeMode_ReturnsDefault(string themeMode)
    {
        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(themeMode);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("System")]
    [TestCase("system")]
    public void GetRequestedThemeVariant_SystemThemeMode_ReturnsDefault(string themeMode)
    {
        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(themeMode);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("Light")]
    [TestCase("light")]
    public void GetRequestedThemeVariant_LightThemeMode_ReturnsLight(string themeMode)
    {
        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(themeMode);

        Assert.That(result, Is.SameAs(ThemeVariant.Light));
    }

    [Test]
    [TestCase("Dark")]
    [TestCase("dark")]
    public void GetRequestedThemeVariant_DarkThemeMode_ReturnsDark(string themeMode)
    {
        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(themeMode);

        Assert.That(result, Is.SameAs(ThemeVariant.Dark));
    }

    [Test]
    public void GetRequestedThemeVariant_UnsupportedThemeMode_ThrowsInvalidOperationException()
    {
        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            ThemeSettingsReader.GetRequestedThemeVariant("Blue"));

        Assert.That(exception!.Message,
            Is.EqualTo("Unsupported UI theme mode 'Blue'. Supported values: System, Light, Dark."));
    }

    [Test]
    public void GetRequestedThemeVariant_NumericUnsupportedThemeMode_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThemeSettingsReader.GetRequestedThemeVariant("42"));

        Assert.That(exception!.ParamName, Is.EqualTo("themeMode"));
    }
}
