namespace PhotoManager.Domain.UserConfigurationSettings;

public record class StorageSettings(
    ushort BackupsToKeep,
    FoldersNameSettings FoldersNameSettings,
    char? Separator,
    string? StorageVersion,
    TablesSettings TablesSettings,
    ushort ThumbnailsDictionaryEntriesToKeep);

public record class FoldersNameSettings(
    string? Blobs,
    string? Tables);

public record class TablesSettings(
    string? AssetsTableName,
    string? FoldersTableName,
    string? RecentTargetPathsTableName,
    string? SyncAssetsDirectoriesDefinitionsTableName);
