using Avalonia.Styling;

namespace PhotoManager.UI.Configuration;

public static class ThemeSettingsReader
{
    public static ThemeVariant GetRequestedThemeVariant(string? configuredThemeMode)
    {
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
