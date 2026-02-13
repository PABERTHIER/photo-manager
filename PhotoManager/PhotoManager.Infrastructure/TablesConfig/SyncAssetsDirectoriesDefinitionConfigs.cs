namespace PhotoManager.Infrastructure.TablesConfig;

public static class SyncAssetsDirectoriesDefinitionConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return
        [
            new() { ColumnName = "SourceDirectory" },
            new() { ColumnName = "DestinationDirectory" },
            new() { ColumnName = "IncludeSubFolders" },
            new() { ColumnName = "DeleteAssetsNotInSource" }
        ];
    }

    public static SyncAssetsDirectoriesDefinition ReadFunc(string[] values)
    {
        return new SyncAssetsDirectoriesDefinition
        {
            SourceDirectory = values[0],
            DestinationDirectory = values[1],
            IncludeSubFolders = bool.Parse(values[2]),
            DeleteAssetsNotInSource = bool.Parse(values[3])
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
