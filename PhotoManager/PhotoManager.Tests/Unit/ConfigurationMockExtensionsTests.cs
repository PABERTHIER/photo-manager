using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace PhotoManager.Tests.Unit;

public class ConfigurationMockExtensionsTests
{
    [Fact]
    public void ConfigurationMockTest()
    {
        Mock<IConfigurationRoot> configurationMock = new();
        configurationMock.MockGetValue("appsettings:CatalogBatchSize", "100");

        IConfigurationRoot configuration = configurationMock.Object;
        configuration.GetValue<string>("appsettings:CatalogBatchSize").Should().Be("100");
    }
}
