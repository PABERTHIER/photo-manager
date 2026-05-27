using Avalonia.Data.Converters;
using PhotoManager.Domain;
using System.Globalization;

namespace PhotoManager.UI.Converters;

public class PixelSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Asset asset
            ? $"{asset.Pixel.Asset.Width}x{asset.Pixel.Asset.Height} pixels"
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
