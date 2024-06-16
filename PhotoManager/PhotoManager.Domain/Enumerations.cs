namespace PhotoManager.Domain;

public enum AppModeEnum
{
    Thumbnails,
    Viewer
}

public enum ReasonEnum
{
    AssetCreated,
    AssetNotCreated,
    AssetUpdated,
    AssetDeleted,
    FolderInspectionInProgress,
    FolderInspectionCompleted,
    FolderCreated,
    FolderDeleted,
    BackupCreationStarted,
    BackupUpdateStarted,
    NoBackupChangesDetected,
    BackupCompleted,
    CatalogProcessCancelled,
    CatalogProcessFailed,
    CatalogProcessEnded
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

public enum SortCriteriaEnum
{
    Undefined, // TODO: Remove that one, should be FileName the default one
    FileName,
    FileSize,
    FileCreationDateTime,
    FileModificationDateTime,
    ThumbnailCreationDateTime
}
