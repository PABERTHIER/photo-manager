using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.Tests.Helpers;
using PhotoManager.UI;
using System.IO;
using Xunit;

namespace PhotoManager.Tests.Unit
{
    public class UserConfigurationServiceTests
    {
        private string dataDirectory;
        private IConfigurationRoot configuration;

        public UserConfigurationServiceTests()
        {
            dataDirectory = Path.GetDirectoryName(typeof(UserConfigurationServiceTests).Assembly.Location);
            dataDirectory = Path.Combine(dataDirectory, "TestFiles");

            Mock<IConfigurationRoot> configurationMock = new();
            configurationMock
                .MockGetValue("appsettings:InitialDirectory", dataDirectory)
                .MockGetValue("appsettings:ApplicationDataDirectory", dataDirectory)
                .MockGetValue("appsettings:CatalogBatchSize", "100")
                .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
                .MockGetValue("appsettings:BackupsToKeep", "2");

            configuration = configurationMock.Object;
        }

        [Fact]
        public void GetPicturesDirectoryTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            string result = userConfigurationService.GetPicturesDirectory();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void GetAboutInformationTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            AboutInformation result = userConfigurationService.GetAboutInformation(typeof(App).Assembly);
            result.Product.Should().Be("PhotoManager");
            result.Version.Should().NotBeEmpty();
            result.Version.Should().StartWith("v");
        }

        [Fact]
        public void GetCatalogBatchSizeTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            int result = userConfigurationService.GetCatalogBatchSize();
            result.Should().Be(100);
        }

        [Fact]
        public void GetCatalogCooldownMinutesTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            int result = userConfigurationService.GetCatalogCooldownMinutes();
            result.Should().Be(5);
        }

        [Fact]
        public void GetBackupsToKeepTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            int result = userConfigurationService.GetBackupsToKeep();
            result.Should().Be(2);
        }

        [Fact]
        public void GetInitialFolderConfiguredTest()
        {
            UserConfigurationService userConfigurationService = new(configuration);
            string result = userConfigurationService.GetPicturesDirectory();
            result.Should().Be(Constants.PathLocation);
        }

        [Fact]
        public void GetApplicationDataFolderConfiguredTest()
        {
            Mock<IConfigurationRoot> configurationMock = new();
            configurationMock
                .MockGetValue("appsettings:InitialDirectory", dataDirectory)
                .MockGetValue("appsettings:ApplicationDataDirectory", dataDirectory)
                .MockGetValue("appsettings:CatalogBatchSize", "100")
                .MockGetValue("appsettings:CatalogCooldownMinutes", "5");

            string result = UserConfigurationHelper.GetApplicationBackUpTestsFolder();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void GetApplicationDataFolderNotConfiguredTest()
        {
            Mock<IConfigurationRoot> configurationMock = new();
            configurationMock
                .MockGetValue("appsettings:InitialDirectory", dataDirectory)
                .MockGetValue("appsettings:ApplicationDataDirectory", "")
                .MockGetValue("appsettings:CatalogBatchSize", "100")
                .MockGetValue("appsettings:CatalogCooldownMinutes", "5");

            string result = UserConfigurationHelper.GetApplicationBackUpTestsFolder();
            result.Should().NotBeEmpty();
        }
    }
}
