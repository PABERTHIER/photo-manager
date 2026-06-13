using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Converters;

public class FileSizeConverter : IValueConverter
{
    private const long ONE_KILOBYTE = 1024;
    private const long ONE_MEGABYTE = ONE_KILOBYTE * 1024;
    private const long ONE_GIGABYTE = ONE_MEGABYTE * 1024;
    private const string KILOBYTE_UNIT = "KB";
    private const string MEGABYTE_UNIT = "MB";
    private const string GIGABYTE_UNIT = "GB";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        long fileSize = System.Convert.ToInt64(value, culture);

        if (fileSize < ONE_KILOBYTE)
        {
            return fileSize + " bytes";
        }

        decimal bytes = fileSize;
        decimal decimalValue;
        string unit;

        bool sizeInKb = fileSize < ONE_MEGABYTE;
        bool sizeInMb = fileSize < ONE_GIGABYTE;

        if (sizeInKb)
        {
            decimalValue = bytes / ONE_KILOBYTE;
            unit = KILOBYTE_UNIT;
        }
        else if (sizeInMb)
        {
            decimalValue = bytes / ONE_MEGABYTE;
            unit = MEGABYTE_UNIT;
        }
        else
        {
            decimalValue = bytes / ONE_GIGABYTE;
            unit = GIGABYTE_UNIT;
        }

        return decimalValue.ToString("0.0", culture) + " " + unit;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
