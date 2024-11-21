namespace PhotoManager.Domain.UserConfigurationSettings;

public record StorageSettings(
    ushort BackupsToKeep,
    FoldersNameSettings FoldersNameSettings,
    char Separator,
    string StorageVersion,
    TablesSettings TablesSettings,
    ushort ThumbnailsDictionaryEntriesToKeep);

public record FoldersNameSettings(
    string Blobs,
    string Tables);

public record TablesSettings(
    string AssetsTableName,
    string FoldersTableName,
    string RecentTargetPathsTableName,
    string SyncAssetsDirectoriesDefinitionsTableName);
