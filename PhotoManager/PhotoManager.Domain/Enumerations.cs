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
    AssetNotCreated,
    AssetUpdated,
    AssetDeleted,
    FolderInspectionInProgress,
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
    Undefined, // TODO: Remove that one, should be FileName the default one
    FileName,
    FileSize,
    FileCreationDateTime,
    FileModificationDateTime,
    ThumbnailCreationDateTime
}
