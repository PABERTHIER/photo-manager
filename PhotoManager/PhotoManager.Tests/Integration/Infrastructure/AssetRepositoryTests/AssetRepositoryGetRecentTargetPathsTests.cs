using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetRecentTargetPathsTests
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
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
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
    public void GetRecentTargetPaths_RecentTargetPaths_ReturnsRecentTargetPaths()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);
            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetRecentTargetPaths_NoRecentTargetPaths_ReturnsEmptyList()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetRecentTargetPaths_ConcurrentAccess_RecentTargetPathsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);

            List<string> recentTargetPaths1 = [];
            List<string> recentTargetPaths2 = [];
            List<string> recentTargetPaths3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => recentTargetPaths1 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths2 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths3 = _assetRepository.GetRecentTargetPaths()
            );

            Assert.That(recentTargetPaths1, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths1[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths1[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(recentTargetPaths2, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths2[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths2[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(recentTargetPaths3, Has.Count.EqualTo(2));
            Assert.That(recentTargetPaths3[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths3[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
