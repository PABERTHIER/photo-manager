﻿namespace PhotoManager.TotoTests;

public static class FolderConfigs
{
    public static ColumnProperties[] ConfigureDataTable()
    {
        return new ColumnProperties[]
        {
            new ColumnProperties { ColumnName = "FolderId" },
            new ColumnProperties { ColumnName = "Path" }
        };
    }

    public static Folder ReadFunc(string[] values)
    {
        return new Folder
        {
            FolderId = new Guid(values[0]),
            Path = values[1]
        };
    }

    public static object WriteFunc(Folder f, int i)
    {
        return i switch
        {
            0 => f.FolderId,
            1 => f.Path,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
    }
}