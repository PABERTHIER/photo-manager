namespace PhotoManager.Infrastructure.TablesConfig;

public static class SyncDefinitionConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return new ColumnProperties[]
        {
            new ColumnProperties { ColumnName = "SourceDirectory" },
            new ColumnProperties { ColumnName = "DestinationDirectory" },
            new ColumnProperties { ColumnName = "IncludeSubFolders" },
            new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
        };
    }

    public static SyncAssetsDirectoriesDefinition ReadFunc(string[] values)
    {
        return new SyncAssetsDirectoriesDefinition
        {
            SourceDirectory = values[0],
            DestinationDirectory = values[1],
            IncludeSubFolders = bool.Parse(values[2]),
            DeleteAssetsNotInSource = values.Length > 3 && bool.Parse(values[3])
        };
    }

    public static object WriteFunc(SyncAssetsDirectoriesDefinition d, int i)
    {
        return i switch
        {
            0 => d.SourceDirectory,
            1 => d.DestinationDirectory,
            2 => d.IncludeSubFolders,
            3 => d.DeleteAssetsNotInSource,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}
