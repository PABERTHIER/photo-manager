namespace PhotoManager.TotoTests;

public static class ConfigurationMockExtensions
{
    public static Mock<IConfigurationRoot> MockGetValue(this Mock<IConfigurationRoot> configurationRootMock, string key, string value)
    {
        Mock<IConfigurationSection> configurationSectionMock = new();
        configurationSectionMock.SetupGet(s => s.Value).Returns(value);
        configurationRootMock.Setup(c => c.GetSection(key)).Returns(configurationSectionMock.Object);

        return configurationRootMock;
    }

    public static Mock<IConfigurationRoot> GetDefaultMockConfig(this Mock<IConfigurationRoot> configurationRootMock)
    {
        configurationRootMock
             .MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, "false")
             .MockGetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE, "The asset is corrupted")
             .MockGetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE, "The asset has been rotated")
             .MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, "100")
             .MockGetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES, "5")
             .MockGetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION, "10000")
             .MockGetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION, "1")
             .MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "false")
             .MockGetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES, "false")
             .MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150")
             .MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200")
             .MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, "40")
             .MockGetValue(UserConfigurationKeys.USING_DHASH, "false")
             .MockGetValue(UserConfigurationKeys.USING_MD5_HASH, "false")
             .MockGetValue(UserConfigurationKeys.USING_PHASH, "false")
             .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, "C:\\Path")
             .MockGetValue(UserConfigurationKeys.BACKUP_PATH, "C:\\Path\\To\\Backup")
             .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, "C:\\Path\\To\\FolderExempted")
             .MockGetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME, "OutputVideoFirstFrame")
             .MockGetValue(UserConfigurationKeys.PROJECT_NAME, "PhotoManager")
             .MockGetValue(UserConfigurationKeys.PROJECT_OWNER, "Toto")
             .MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP, "2")
             .MockGetValue(UserConfigurationKeys.BLOBS_FOLDER_NAME, "Blobs")
             .MockGetValue(UserConfigurationKeys.TABLES_FOLDER_NAME, "Tables")
             .MockGetValue(UserConfigurationKeys.SEPARATOR, "|")
             .MockGetValue(UserConfigurationKeys.STORAGE_VERSION, "1.0")
             .MockGetValue(UserConfigurationKeys.ASSETS_TABLE_NAME, "Assets")
             .MockGetValue(UserConfigurationKeys.FOLDERS_TABLE_NAME, "Folders")
             .MockGetValue(UserConfigurationKeys.RECENT_TARGET_PATHS_TABLE_NAME, "RecentTargetPaths")
             .MockGetValue(UserConfigurationKeys.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_TABLE_NAME, "SyncAssetsDirectoriesDefinitions")
             .MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "5");

        return configurationRootMock;
    }
}
