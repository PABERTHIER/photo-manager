using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [Test]
    public void ResolveDataDirectory_ValidPath_ReturnsCorrectPath()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!);

        UserConfigurationService userConfigurationService = new(configurationRootMock);
        PathProviderService pathProviderService = new(userConfigurationService);

        string expected = userConfigurationService.PathSettings.BackupPath;

        string result = pathProviderService.ResolveDataDirectory();

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
