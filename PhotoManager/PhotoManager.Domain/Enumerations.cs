namespace PhotoManager.Domain;

public enum AppModeEnum
{
    Thumbnails,
    Viewer
}

public enum ReasonEnum
{
    // TODO: Add default value when 100% of code coverage, because AssetCreated should not be the default one
    AssetCreated,
    AssetUpdated,
    AssetDeleted,
    FolderCreated,
    FolderDeleted
}

public enum WallpaperStyle
{
    Center,
    Fill,
    Fit,
    Span,
    Stretch,
    Tile
}

public enum ProcessStepEnum
{
    ViewDescription,
    Configure,
    Run,
    ViewResults
}

public enum SortCriteriaEnum
{
    Undefined,
    FileName,
    FileSize,
    FileCreationDateTime,
    FileModificationDateTime,
    ThumbnailCreationDateTime
}
