#nullable disable
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.GetType().Name == "NamedObject" ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
