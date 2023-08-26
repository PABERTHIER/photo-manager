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
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "2")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5")
            .MockGetValue("appsettings:Repository.Owner", "toto")
            .MockGetValue("appsettings:Repository.Name", "photo-manager");

        IConfigurationRoot configuration = configurationMock.Object;

        Assert.AreEqual("100", configuration.GetValue<string>("appsettings:CatalogBatchSize"));
        Assert.AreEqual("2", configuration.GetValue<string>("appsettings:CatalogCooldownMinutes"));
        Assert.AreEqual("2", configuration.GetValue<string>("appsettings:BackupsToKeep"));
        Assert.AreEqual("5", configuration.GetValue<string>("appsettings:ThumbnailsDictionaryEntriesToKeep"));
        Assert.AreEqual("toto", configuration.GetValue<string>("appsettings:Repository.Owner"));
        Assert.AreEqual("photo-manager", configuration.GetValue<string>("appsettings:Repository.Name"));
    }
}
