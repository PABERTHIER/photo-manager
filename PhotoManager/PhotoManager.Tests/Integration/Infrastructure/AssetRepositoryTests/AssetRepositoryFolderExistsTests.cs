using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryFolderExistsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;

    private Mock<IPathProviderService>? _pathProviderServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = new();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
    }

    [Test]
    public void FolderExists_FolderExists_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            _assetRepository!.AddFolder(folderPath);

            bool folderExists = _assetRepository!.FolderExists(folderPath);

            Assert.That(folderExists, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void FolderExists_FolderDoesNotExist_ReturnsFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_2);

            _assetRepository!.AddFolder(folderPath1);

            bool folderExists = _assetRepository!.FolderExists(folderPath2);

            Assert.That(folderExists, Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void FolderExists_ConcurrentAccess_FoldersAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER_1);

            _assetRepository!.AddFolder(folderPath);

            bool folderExists1 = false;
            bool folderExists2 = false;
            bool folderExists3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => folderExists1 = _assetRepository!.FolderExists(folderPath),
                () => folderExists2 = _assetRepository!.FolderExists(folderPath),
                () => folderExists3 = _assetRepository!.FolderExists(folderPath)
            );

            Assert.That(folderExists1, Is.True);
            Assert.That(folderExists2, Is.True);
            Assert.That(folderExists3, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
