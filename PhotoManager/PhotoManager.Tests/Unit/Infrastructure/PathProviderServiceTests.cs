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
    [TestCase("1.0", "v1.0")]
    [TestCase("1.1", "v1.1")]
    [TestCase("2.0", "v2.0")]
    public void ResolveDataDirectory_ValidStorageVersion_ReturnsCorrectPath(string storageVersion,
        string storageVersionPath)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!);
        configurationRootMock.MockGetValue(UserConfigurationKeys.STORAGE_VERSION, storageVersion);

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        PathProviderService pathProviderService = new(userConfigurationService);

        string expected = Path.Combine(userConfigurationService.PathSettings.BackupPath, storageVersionPath);

        string result = pathProviderService.ResolveDataDirectory();

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
