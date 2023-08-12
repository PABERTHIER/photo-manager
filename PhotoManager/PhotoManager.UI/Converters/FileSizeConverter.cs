using System;
using System.Globalization;
using System.Windows.Data;

namespace PhotoManager.UI.Converters;

public class FileSizeConverter : IValueConverter
{
    private const long ONE_KILOBYTE = 1024;
    private const long ONE_MEGABYTE = ONE_KILOBYTE * 1024;
    private const long ONE_GIGABYTE = ONE_MEGABYTE * 1024;
    private const string KILOBYTE_UNIT = "KB";
    private const string MEGABYTE_UNIT = "MB";
    private const string GIGABYTE_UNIT = "GB";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        long fileSize = System.Convert.ToInt64(value, culture);
        string result;

        if (fileSize < ONE_KILOBYTE)
        {
            result = fileSize + " bytes";
        }
        else
        {
            decimal bytes = fileSize;
            decimal decimal_value;
            string unit;

            bool sizeInKb = (fileSize >= ONE_KILOBYTE && fileSize < ONE_MEGABYTE && fileSize < ONE_GIGABYTE);
            bool sizeInMb = (fileSize >= ONE_MEGABYTE && fileSize < ONE_GIGABYTE);

            if (sizeInKb)
            {
                decimal_value = bytes / ONE_KILOBYTE;
                unit = KILOBYTE_UNIT;
            }
            else if (!sizeInKb && sizeInMb)
            {
                decimal_value = bytes / ONE_MEGABYTE;
                unit = MEGABYTE_UNIT;
            }
            else
            {
                decimal_value = bytes / ONE_GIGABYTE;
                unit = GIGABYTE_UNIT;
            }

            result = decimal_value.ToString("0.0", culture) + " " + unit;
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
