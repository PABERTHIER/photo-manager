using Avalonia.Data.Converters;
using System.Globalization;

namespace PhotoManager.UI.Converters;

public class TernaryConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values is [true, _, ..] ? values[1] : null;
    }
}
