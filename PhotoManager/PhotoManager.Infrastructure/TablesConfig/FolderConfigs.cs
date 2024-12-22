namespace PhotoManager.Infrastructure.TablesConfig;

public static class FolderConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return new ColumnProperties[]
        {
            new ColumnProperties { ColumnName = "Id" },
            new ColumnProperties { ColumnName = "Path" }
        };
    }

    public static Folder ReadFunc(string[] values)
    {
        return new()
        {
            Id = new Guid(values[0]),
            Path = values[1]
        };
    }

    public static object WriteFunc(Folder f, int i)
    {
        return i switch
        {
            0 => f.Id,
            1 => f.Path,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}
