namespace PhotoManager.Infrastructure.TablesConfig;

public static class RecentPathsConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return
        [
            new() { ColumnName = "Path" }
        ];
    }

    public static string ReadFunc(string[] values)
    {
        return values[0];
    }

    public static object WriteFunc(string p, int i)
    {
        return i switch
        {
            0 => p,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}
