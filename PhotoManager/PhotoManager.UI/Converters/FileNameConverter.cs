using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Converters;

public class FileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string fileName && !string.IsNullOrWhiteSpace(fileName)
            ? fileName.Replace("_", "__")
            : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
