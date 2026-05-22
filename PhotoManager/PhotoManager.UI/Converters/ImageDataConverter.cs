using PhotoManager.Common;
using PhotoManager.Common.Imaging;
using System.Globalization;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class ImageDataConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BitmapImageData bitmapImageData)
        {
            return bitmapImageData.BitmapImage;
        }

        if (value is IImageData)
        {
            return null;
        }

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
