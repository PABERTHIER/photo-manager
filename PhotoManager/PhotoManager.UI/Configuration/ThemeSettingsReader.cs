using Avalonia.Styling;
using Microsoft.Extensions.Configuration;

namespace PhotoManager.UI.Configuration;

public static class ThemeSettingsReader
{
    private const string THEME_MODE_CONFIGURATION_KEY = "appsettings:Ui:ThemeMode";

    public static ThemeVariant GetRequestedThemeVariant(IConfiguration configuration)
    {
        string? configuredThemeMode = configuration[THEME_MODE_CONFIGURATION_KEY];

        if (string.IsNullOrWhiteSpace(configuredThemeMode))
        {
            return ThemeVariant.Default;
        }

        if (!Enum.TryParse(configuredThemeMode, ignoreCase: true, out ThemeMode themeMode))
        {
            throw new InvalidOperationException(
                $"Unsupported UI theme mode '{configuredThemeMode}'. Supported values: System, Light, Dark.");
        }

        return themeMode switch
        {
            ThemeMode.System => ThemeVariant.Default,
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(themeMode), themeMode, null)
        };
    }
}
