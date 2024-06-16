namespace PhotoManager.Domain;

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
