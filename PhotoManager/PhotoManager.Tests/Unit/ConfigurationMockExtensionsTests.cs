using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Unit;

[TestFixture]
public class ConfigurationMockExtensionsTests
{
    [Test]
    public void ConfigurationMockTest()
    {
        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock
            .MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, "100")
            .MockGetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES, "2")
            .MockGetValue(UserConfigurationKeys.PROJECT_NAME, "photo-manager")
            .MockGetValue(UserConfigurationKeys.PROJECT_OWNER, "toto")
            .MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP, "2")
            .MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "5");

        IConfigurationRoot configuration = configurationMock.Object;

        Assert.AreEqual(100, configuration.GetValue<int>(UserConfigurationKeys.CATALOG_BATCH_SIZE));
        Assert.AreEqual(2, configuration.GetValue<int>(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES));
        Assert.AreEqual("photo-manager", configuration.GetValue<string>(UserConfigurationKeys.PROJECT_NAME));
        Assert.AreEqual("toto", configuration.GetValue<string>(UserConfigurationKeys.PROJECT_OWNER));
        Assert.AreEqual(2, configuration.GetValue<int>(UserConfigurationKeys.BACKUPS_TO_KEEP));
        Assert.AreEqual(5, configuration.GetValue<int>(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP));
    }
}
