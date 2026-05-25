using PhotoManager.Common.Imaging;
using PhotoManager.UI.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Converters;

public class ImageDataConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BitmapImageData bitmapImageData)
        {
            return bitmapImageData.BitmapImage;
        }

        if (value is IImageData imageData)
        {
            return ConvertToWpfBitmapImage(imageData);
        }

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static BitmapImage? ConvertToWpfBitmapImage(IImageData imageData)
    {
        return BitmapImageFactory.Create(imageData, ImageEncodingFormat.Png);
    }
}
