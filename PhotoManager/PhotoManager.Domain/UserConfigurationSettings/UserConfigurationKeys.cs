namespace PhotoManager.Domain.UserConfigurationSettings;

public static class UserConfigurationKeys
{
    public const string ANALYSE_VIDEOS = "appsettings:Asset:AnalyseVideos";
    public const string ASSET_CORRUPTED_MESSAGE = "appsettings:Asset:AssetCorruptedMessage";
    public const string ASSET_ROTATED_MESSAGE = "appsettings:Asset:AssetRotatedMessage";
    public const string CATALOG_BATCH_SIZE = "appsettings:Asset:CatalogBatchSize";
    public const string CATALOG_COOLDOWN_MINUTES = "appsettings:Asset:CatalogCooldownMinutes";
    public const string CORRUPTED_IMAGE_ORIENTATION = "appsettings:Asset:CorruptedImageOrientation";
    public const string DEFAULT_EXIF_ORIENTATION = "appsettings:Asset:DefaultExifOrientation";
    public const string DETECT_THUMBNAILS = "appsettings:Asset:DetectThumbnails";
    public const string SYNC_ASSETS_EVERY_X_MINUTES = "appsettings:Asset:SyncAssetsEveryXMinutes";
    public const string THUMBNAIL_MAX_HEIGHT = "appsettings:Asset:ThumbnailMaxHeight";
    public const string THUMBNAIL_MAX_WIDTH = "appsettings:Asset:ThumbnailMaxWidth";

    public const string PHASH_THRESHOLD = "appsettings:Hash:PHashThreshold";
    public const string USING_DHASH = "appsettings:Hash:UsingDHash";
    public const string USING_MD5_HASH = "appsettings:Hash:UsingMD5Hash";
    public const string USING_PHASH = "appsettings:Hash:UsingPHash";

    public const string ASSETS_DIRECTORY = "appsettings:Paths:AssetsDirectory";
    public const string BACKUP_PATH = "appsettings:Paths:BackupPath";
    public const string EXEMPTED_FOLDER_PATH = "appsettings:Paths:ExemptedFolderPath";
    public const string FFMPEG_PATH = "appsettings:Paths:FfmpegPath";
    public const string FIRST_FRAME_VIDEOS_FOLDER_NAME = "appsettings:Paths:FirstFrameVideosFolderName";

    public const string PROJECT_NAME = "appsettings:Project:Name";
    public const string PROJECT_OWNER = "appsettings:Project:Owner";

    public const string BACKUPS_TO_KEEP = "appsettings:Storage:BackupsToKeep";
    public const string BLOBS_FOLDER_NAME = "appsettings:Storage:FoldersName:Blobs";
    public const string TABLES_FOLDER_NAME = "appsettings:Storage:FoldersName:Tables";
    public const string SEPARATOR = "appsettings:Storage:Separator";
    public const string STORAGE_VERSION = "appsettings:Storage:StorageVersion";
    public const string ASSETS_TABLE_NAME = "appsettings:Storage:Tables:AssetsTableName";
    public const string FOLDERS_TABLE_NAME = "appsettings:Storage:Tables:FoldersTableName";
    public const string RECENT_TARGET_PATHS_TABLE_NAME = "appsettings:Storage:Tables:RecentTargetPathsTableName";
    public const string SYNC_ASSETS_DIRECTORIES_DEFINITIONS_TABLE_NAME = "appsettings:Storage:Tables:SyncAssetsDirectoriesDefinitionsTableName";
    public const string THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP = "appsettings:Storage:ThumbnailsDictionaryEntriesToKeep";
}
