using System;
using System.Globalization;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class TernaryConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)values[0] ? values[1] : null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

