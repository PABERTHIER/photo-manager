using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Converters;

public class ImageDataConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IImageData imageData
            ? AvaloniaBitmapFactory.Create(imageData, ImageEncodingFormat.Png)
            : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
