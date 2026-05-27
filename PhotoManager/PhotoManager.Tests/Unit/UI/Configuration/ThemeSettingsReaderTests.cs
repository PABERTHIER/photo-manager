using Avalonia.Styling;
using PhotoManager.UI.Configuration;

namespace PhotoManager.Tests.Unit.UI.Configuration;

[TestFixture]
public class ThemeSettingsReaderTests
{
    [Test]
    public void GetRequestedThemeVariant_MissingThemeMode_ReturnsDefault()
    {
        IConfiguration configuration = BuildConfiguration(null);

        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(configuration);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void GetRequestedThemeVariant_WhitespaceThemeMode_ReturnsDefault(string themeMode)
    {
        IConfiguration configuration = BuildConfiguration(themeMode);

        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(configuration);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("System")]
    [TestCase("system")]
    public void GetRequestedThemeVariant_SystemThemeMode_ReturnsDefault(string themeMode)
    {
        IConfiguration configuration = BuildConfiguration(themeMode);

        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(configuration);

        Assert.That(result, Is.SameAs(ThemeVariant.Default));
    }

    [Test]
    [TestCase("Light")]
    [TestCase("light")]
    public void GetRequestedThemeVariant_LightThemeMode_ReturnsLight(string themeMode)
    {
        IConfiguration configuration = BuildConfiguration(themeMode);

        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(configuration);

        Assert.That(result, Is.SameAs(ThemeVariant.Light));
    }

    [Test]
    [TestCase("Dark")]
    [TestCase("dark")]
    public void GetRequestedThemeVariant_DarkThemeMode_ReturnsDark(string themeMode)
    {
        IConfiguration configuration = BuildConfiguration(themeMode);

        ThemeVariant result = ThemeSettingsReader.GetRequestedThemeVariant(configuration);

        Assert.That(result, Is.SameAs(ThemeVariant.Dark));
    }

    [Test]
    public void GetRequestedThemeVariant_UnsupportedThemeMode_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = BuildConfiguration("Blue");

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            ThemeSettingsReader.GetRequestedThemeVariant(configuration));

        Assert.That(exception!.Message,
            Is.EqualTo("Unsupported UI theme mode 'Blue'. Supported values: System, Light, Dark."));
    }

    [Test]
    public void GetRequestedThemeVariant_NumericUnsupportedThemeMode_ThrowsArgumentOutOfRangeException()
    {
        IConfiguration configuration = BuildConfiguration("42");

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThemeSettingsReader.GetRequestedThemeVariant(configuration));

        Assert.That(exception!.ParamName, Is.EqualTo("themeMode"));
    }

    private static IConfigurationRoot BuildConfiguration(string? themeMode)
    {
        Dictionary<string, string?> settings = [];

        if (themeMode != null)
        {
            settings["appsettings:Ui:ThemeMode"] = themeMode;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
