using System;
using System.Globalization;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class FileNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && fileName[0] == '_')
            {
                return "_" + fileName;
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
