#nullable disable
using PhotoManager.Domain;
using System.Globalization;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class PixelSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = "";

        if (value != null)
        {
            result = value is Asset asset ? $"{asset.Pixel.Asset.Width}x{asset.Pixel.Asset.Height} pixels" : "";
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
