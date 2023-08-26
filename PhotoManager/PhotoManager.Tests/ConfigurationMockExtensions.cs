using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests;

public static class ConfigurationMockExtensions
{
    public static Mock<IConfigurationRoot> MockGetValue(this Mock<IConfigurationRoot> configurationMock, string key, string value)
    {
        Mock<IConfigurationSection> configurationSectionMock = new();
        configurationSectionMock.SetupGet(s => s.Value).Returns(value);
        configurationMock.Setup(c => c.GetSection(key)).Returns(configurationSectionMock.Object);

        return configurationMock;
    }
}
