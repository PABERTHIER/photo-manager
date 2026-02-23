using Directories = PhotoManager.Tests.Unit.Constants.Directories;

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

        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.ANALYSE_VIDEOS), Is.False);
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
            Is.EqualTo("The asset is corrupted"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
            Is.EqualTo("The asset has been rotated"));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.CATALOG_BATCH_SIZE), Is.EqualTo(100));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES), Is.EqualTo(5));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION), Is.EqualTo(10000));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION), Is.EqualTo(1));
        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.DETECT_THUMBNAILS), Is.False);
        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES), Is.False);
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT), Is.EqualTo(150));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH), Is.EqualTo(200));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.PHASH_THRESHOLD), Is.EqualTo(10));
        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.USING_DHASH), Is.False);
        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.USING_MD5_HASH), Is.False);
        Assert.That(configuration.GetValue<bool>(UserConfigurationKeys.USING_PHASH), Is.False);
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.ASSETS_DIRECTORY), Is.EqualTo("C:\\Path"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.BACKUP_PATH),
            Is.EqualTo("C:\\Path\\To\\Backup"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            Is.EqualTo("C:\\Path\\To\\FolderExempted"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
            Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME), Is.EqualTo("PhotoManager"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER), Is.EqualTo("Toto"));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.BACKUPS_TO_KEEP), Is.EqualTo(2));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.BLOBS_FOLDER_NAME),
            Is.EqualTo(Directories.BLOBS));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.TABLES_FOLDER_NAME),
            Is.EqualTo(Directories.TABLES));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.SEPARATOR), Is.EqualTo("|"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.STORAGE_VERSION), Is.EqualTo("1.0"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.ASSETS_TABLE_NAME), Is.EqualTo("Assets"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.FOLDERS_TABLE_NAME), Is.EqualTo("Folders"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.RECENT_TARGET_PATHS_TABLE_NAME),
            Is.EqualTo("RecentTargetPaths"));
        Assert.That(
            configuration.GetValue<string>(UserConfigurationKeys.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_TABLE_NAME),
            Is.EqualTo("SyncAssetsDirectoriesDefinitions"));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
            Is.EqualTo(5));
    }
}
