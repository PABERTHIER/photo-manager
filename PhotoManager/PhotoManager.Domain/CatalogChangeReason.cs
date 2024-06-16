namespace PhotoManager.Domain;

public enum CatalogChangeReason
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
