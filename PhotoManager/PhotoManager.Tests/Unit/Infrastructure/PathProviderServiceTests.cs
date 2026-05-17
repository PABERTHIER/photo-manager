using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    private string? _assetsDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [Test]
    public void ResolveDatabaseDirectory_ValidPath_ReturnsCorrectPath()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _assetsDirectory!);

        UserConfigurationService userConfigurationService = new(configurationRootMock);
        PathProviderService pathProviderService = new(userConfigurationService);

        string expected = userConfigurationService.PathSettings.DatabasePath;

        string result = pathProviderService.ResolveDatabaseDirectory();

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
