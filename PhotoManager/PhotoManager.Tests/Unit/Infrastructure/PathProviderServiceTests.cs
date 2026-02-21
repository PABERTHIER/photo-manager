using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class PathProviderServiceTests
{
    private string? _dataDirectory;

    private PathProviderService? _pathProviderService;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory);

        _userConfigurationService = new(configurationRootMock.Object);
        _pathProviderService = new(_userConfigurationService);
    }

    [Test]
    [TestCase("1.0", "v1.0")]
    [TestCase("1.1", "v1.1")]
    [TestCase("2.0", "v2.0")]
    public void ResolveDataDirectory_ValidStorageVersion_ReturnsCorrectPath(string storageVersion,
        string storageVersionPath)
    {
        string expected = Path.Combine(_userConfigurationService!.PathSettings.BackupPath, storageVersionPath);

        string result = _pathProviderService!.ResolveDataDirectory(storageVersion);

        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
        Assert.That(result, Is.EqualTo(expected));
    }
}
