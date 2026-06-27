using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests;

public static class ConfigurationMockExtensions
{
    extension(IConfigurationRoot configurationRoot)
    {
        public IConfigurationRoot MockGetValue(string key, string value)
        {
            IConfigurationSection configurationSection = Substitute.For<IConfigurationSection>();
            configurationSection.Value.Returns(value);
            configurationRoot.GetSection(key).Returns(configurationSection);

            return configurationRoot;
        }

        public void GetDefaultMockConfig()
        {
            configurationRoot
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
                .MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, "10")
                .MockGetValue(UserConfigurationKeys.USING_DHASH, "false")
                .MockGetValue(UserConfigurationKeys.USING_MD5_HASH, "false")
                .MockGetValue(UserConfigurationKeys.USING_PHASH, "false")
                .MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY,
                    PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets"))
                .MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH,
                    PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted"))
                .MockGetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME,
                    Directories.OUTPUT_VIDEO_FIRST_FRAME)
                .MockGetValue(UserConfigurationKeys.READ_CONCURRENCY, "3")
                .MockGetValue(UserConfigurationKeys.PROCESS_CONCURRENCY, "2")
                .MockGetValue(UserConfigurationKeys.READ_BUFFER_SIZE, "8")
                .MockGetValue(UserConfigurationKeys.PROCESS_BUFFER_SIZE, "16")
                .MockGetValue(UserConfigurationKeys.PERSIST_BUFFER_SIZE, "24")
                .MockGetValue(UserConfigurationKeys.PERSIST_BATCH_SIZE, "20")
                .MockGetValue(UserConfigurationKeys.PROJECT_NAME, "PhotoManager")
                .MockGetValue(UserConfigurationKeys.PROJECT_OWNER, "Toto")
                .MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP, "2")
                .MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "5")
                .MockGetValue(UserConfigurationKeys.THEME_MODE, "Light");

        }
    }
}
