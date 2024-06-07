namespace PhotoManager.Tests.Unit;

[TestFixture]
public class ConfigurationMockExtensionsTests
{
    [Test]
    public void ConfigurationMockTest()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        IConfigurationRoot configuration = configurationRootMock.Object;

        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.ANALYSE_VIDEOS));
        Assert.AreEqual("The asset is corrupted", configuration.GetValue<string>(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE));
        Assert.AreEqual("The asset has been rotated", configuration.GetValue<string>(UserConfigurationKeys.ASSET_ROTATED_MESSAGE));
        Assert.AreEqual(100, configuration.GetValue<int>(UserConfigurationKeys.CATALOG_BATCH_SIZE));
        Assert.AreEqual(5, configuration.GetValue<ushort>(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES));
        Assert.AreEqual(10000, configuration.GetValue<int>(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION));
        Assert.AreEqual(1, configuration.GetValue<ushort>(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION));
        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.DETECT_THUMBNAILS));
        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES));
        Assert.AreEqual(150, configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT));
        Assert.AreEqual(200, configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH));
        Assert.AreEqual(40, configuration.GetValue<ushort>(UserConfigurationKeys.PHASH_THRESHOLD));
        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.USING_DHASH));
        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.USING_MD5_HASH));
        Assert.AreEqual(false, configuration.GetValue<bool>(UserConfigurationKeys.USING_PHASH));
        Assert.AreEqual("C:\\Path", configuration.GetValue<string>(UserConfigurationKeys.ASSETS_DIRECTORY));
        Assert.AreEqual("C:\\Path\\To\\Backup", configuration.GetValue<string>(UserConfigurationKeys.BACKUP_PATH));
        Assert.AreEqual("C:\\Path\\To\\FolderExempted", configuration.GetValue<string>(UserConfigurationKeys.EXEMPTED_FOLDER_PATH));
        Assert.AreEqual("C:\\Path\\To\\Ffmpeg.exe", configuration.GetValue<string>(UserConfigurationKeys.FFMPEG_PATH));
        Assert.AreEqual("OutputVideoFirstFrame", configuration.GetValue<string>(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME));
        Assert.AreEqual("PhotoManager", configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME));
        Assert.AreEqual("Toto", configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER));
        Assert.AreEqual(2, configuration.GetValue<ushort>(UserConfigurationKeys.BACKUPS_TO_KEEP));
        Assert.AreEqual("Blobs", configuration.GetValue<string>(UserConfigurationKeys.BLOBS_FOLDER_NAME));
        Assert.AreEqual("Tables", configuration.GetValue<string>(UserConfigurationKeys.TABLES_FOLDER_NAME));
        Assert.AreEqual("|", configuration.GetValue<string>(UserConfigurationKeys.SEPARATOR));
        Assert.AreEqual("1.0", configuration.GetValue<string>(UserConfigurationKeys.STORAGE_VERSION));
        Assert.AreEqual("Assets", configuration.GetValue<string>(UserConfigurationKeys.ASSETS_TABLE_NAME));
        Assert.AreEqual("Folders", configuration.GetValue<string>(UserConfigurationKeys.FOLDERS_TABLE_NAME));
        Assert.AreEqual("RecentTargetPaths", configuration.GetValue<string>(UserConfigurationKeys.RECENT_TARGET_PATHS_TABLE_NAME));
        Assert.AreEqual("SyncAssetsDirectoriesDefinitions", configuration.GetValue<string>(UserConfigurationKeys.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_TABLE_NAME));
        Assert.AreEqual(5, configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP));
    }
}
