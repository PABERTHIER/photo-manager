using System.Text;

namespace PhotoManager.Infrastructure.Database.Storage;

public class ObjectListStorage : BaseCsvStorage, IObjectListStorage
{
    public List<T> ReadObjectList<T>(string dataFilePath, Func<string[], T> mapObjectFromCsvFields, Diagnostics diagnostics)
    {
        List<T> list = new();
        
        if (File.Exists(dataFilePath))
        {
            string csv = File.ReadAllText(dataFilePath);
            diagnostics.LastReadFileRaw = csv;
            list = GetObjectListFromCsv(csv, mapObjectFromCsvFields);
        }

        return list;
    }

    public void WriteObjectList<T>(string dataFilePath, List<T> list, Func<T, int, object> mapCsvFieldIndexToCsvField, Diagnostics diagnostics)
    {
        string tableName = Properties?.TableName ?? "NoTableName";
        string csv = GetCsvFromObjectList(list, tableName, mapCsvFieldIndexToCsvField);
        diagnostics.LastWriteFileRaw = csv;
        File.WriteAllText(dataFilePath, csv);
    }

    private List<T> GetObjectListFromCsv<T>(string csv, Func<string[], T> mapObjectFromCsvFields)
    {
        List<T> list = new();
        bool hasRecord;

        using (StringReader reader = new(csv))
        {
            string? line = reader.ReadLine();

            if (line == null)
            {
                return list;
            }

            if (Properties != null)
            {
                string[] headers = GetValuesFromCsvLine(line);

                do
                {
                    line = reader.ReadLine();
                    hasRecord = !string.IsNullOrEmpty(line);

                    if (line == null)
                    {
                        continue;
                    }

                    if (hasRecord)
                    {
                        string[] fields = GetValuesFromCsvLine(line);
                        list.Add(mapObjectFromCsvFields(fields));
                    }
                }
                while (hasRecord);
            }
            else
            {
                string[] headers = line.Split(Separator);

                do
                {
                    line = reader.ReadLine();
                    hasRecord = !string.IsNullOrEmpty(line);

                    if (line == null)
                    {
                        continue;
                    }

                    if (hasRecord)
                    {
                        string[] fields = line.Split(Separator);
                        list.Add(mapObjectFromCsvFields(fields));
                    }
                }
                while (hasRecord);
            }
        }

        return list;
    }

    private string GetCsvFromObjectList<T>(List<T> list, string tableName, Func<T, int, object> mapCsvFieldIndexToCsvField)
    {
        StringBuilder builder = new();

        if (Properties == null)
        {
            throw new Exception($"Properties must be defined for the columns in the table {tableName}.");
        }

        for (int i = 0; i < Properties.ColumnProperties.Length; i++)
        {
            if (EscapeText(Properties.ColumnProperties[i].ColumnName))
            {
                builder.Append(QUOTE);
                builder.Append(Properties.ColumnProperties[i].ColumnName);
                builder.Append(QUOTE);
            }
            else
            {
                builder.Append(Properties.ColumnProperties[i].ColumnName);
            }

            if (i < Properties.ColumnProperties.Length - 1)
            {
                builder.Append(Separator);
            }
        }

        builder.Append(Environment.NewLine);

        for (int i = 0; i < list.Count; i++)
        {
            T row = list[i];

            for (int j = 0; j < Properties.ColumnProperties.Length; j++)
            {
                if (EscapeText(Properties.ColumnProperties[j].ColumnName))
                {
                    builder.Append(QUOTE);
                    builder.Append(mapCsvFieldIndexToCsvField(row, j));
                    builder.Append(QUOTE);
                }
                else
                {
                    builder.Append(mapCsvFieldIndexToCsvField(row, j));
                }

                if (j < Properties.ColumnProperties.Length - 1)
                {
                    builder.Append(Separator);
                }
            }

            builder.Append(Environment.NewLine);
        }

        return builder.ToString();
    }
}
