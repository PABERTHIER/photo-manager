using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit;

[TestFixture]
public class ConfigurationMockExtensionsTests
{
    [Test]
    public void ConfigurationMockTest()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        IConfigurationRoot configuration = configurationRootMock;

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
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.ASSETS_DIRECTORY),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
            Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.READ_CONCURRENCY), Is.EqualTo(3));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.PROCESS_CONCURRENCY), Is.EqualTo(2));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.READ_BUFFER_SIZE), Is.EqualTo(8));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.PROCESS_BUFFER_SIZE), Is.EqualTo(16));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.PERSIST_BUFFER_SIZE), Is.EqualTo(24));
        Assert.That(configuration.GetValue<int>(UserConfigurationKeys.PERSIST_BATCH_SIZE), Is.EqualTo(20));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME), Is.EqualTo("PhotoManager"));
        Assert.That(configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER), Is.EqualTo("Toto"));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.BACKUPS_TO_KEEP), Is.EqualTo(2));
        Assert.That(configuration.GetValue<ushort>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
            Is.EqualTo(5));
    }
}
