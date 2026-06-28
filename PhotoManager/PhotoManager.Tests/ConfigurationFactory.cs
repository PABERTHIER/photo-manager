namespace PhotoManager.Tests;

public static class ConfigurationFactory
{
    public static EditableUserConfiguration CreateEditableConfiguration()
    {
        return new(
            new(true, "Corrupted", "Rotated", 42, 3, 999, 1, true, true, 320, 640),
            new(6, true, true, true),
            new(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets"),
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
                "Frames"),
            new(5, 1, 10, 20, 30, 25),
            new(4, 12),
            new("Dark"));
    }

    public static IConfigurationRoot CreateMockConfiguration(
        string catalogBatchSize = "100",
        string projectName = "PhotoManager",
        string projectOwner = "Toto",
        string themeMode = "Light")
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize)
            .MockGetValue(UserConfigurationKeys.PROJECT_NAME, projectName)
            .MockGetValue(UserConfigurationKeys.PROJECT_OWNER, projectOwner)
            .MockGetValue(UserConfigurationKeys.THEME_MODE, themeMode);

        return configurationRootMock;
    }
}
