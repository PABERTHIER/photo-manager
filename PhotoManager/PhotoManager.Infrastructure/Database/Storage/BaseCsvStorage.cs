namespace PhotoManager.Infrastructure.Database.Storage;

public abstract class BaseCsvStorage
{
    protected const char QUOTE = '"';
    protected DataTableProperties? Properties { get; private set; }
    protected char Separator { get; private set; }

    public void Initialize(DataTableProperties? properties, char separator)
    {
        Properties = properties;
        Separator = separator;
    }

    protected string[] GetValuesFromCsvLine(string line)
    {
        string[] fields = new string[Properties!.ColumnProperties.Length];
        int startIndex = 0;
        string quoteAndSeparator = $"{QUOTE}{Separator}";

        for (int i = 0; i < Properties.ColumnProperties.Length; i++)
        {
            bool escapeText = EscapeText(Properties.ColumnProperties[i].ColumnName);

            int endIndex;
            if (escapeText)
            {
                endIndex = line.IndexOf(quoteAndSeparator, startIndex, StringComparison.Ordinal);
                startIndex++;
            }
            else
            {
                endIndex = line.IndexOf(Separator, startIndex);
            }

            if (endIndex >= 0 && (endIndex < (line.Length - 1)))
            {
                string field = line[startIndex..endIndex];
                fields[i] = field;
                startIndex = endIndex + (escapeText ? 2 : 1);
            }
            else if (endIndex == -1)
            {
                string field = escapeText ? line.Substring(startIndex, line.Length - startIndex - 1) : line[startIndex..];
                fields[i] = field;
            }
        }

        return fields;
    }

    protected bool EscapeText(string columnName)
    {
        bool? result = Properties?.ColumnProperties.Any(c => string.Compare(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase) == 0 && c.EscapeText);

        return result.HasValue && result.Value;
    }
}
