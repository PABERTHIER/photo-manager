using System.Globalization;
using System.Windows.Data;

#nullable disable
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
            decimal decimalValue;
            string unit;

            bool sizeInKb = fileSize is >= ONE_KILOBYTE and < ONE_MEGABYTE;
            bool sizeInMb = fileSize is >= ONE_MEGABYTE and < ONE_GIGABYTE;

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

            result = decimalValue.ToString("0.0", culture) + " " + unit;
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
