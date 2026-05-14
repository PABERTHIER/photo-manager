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
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void GetRecentTargetPaths_RecentTargetPaths_ReturnsRecentTargetPaths()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string[] recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);
            string[] recentTargetPaths = _assetRepository.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Has.Length.EqualTo(2));
            Assert.That(recentTargetPaths[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
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
            string[] recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.That(recentTargetPaths, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
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
            string[] recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);

            string[] recentTargetPaths1 = [];
            string[] recentTargetPaths2 = [];
            string[] recentTargetPaths3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => recentTargetPaths1 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths2 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths3 = _assetRepository.GetRecentTargetPaths()
            );

            Assert.That(recentTargetPaths1, Has.Length.EqualTo(2));
            Assert.That(recentTargetPaths1[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths1[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(recentTargetPaths2, Has.Length.EqualTo(2));
            Assert.That(recentTargetPaths2[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths2[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(recentTargetPaths3, Has.Length.EqualTo(2));
            Assert.That(recentTargetPaths3[0], Is.EqualTo(recentTargetPathsToSave[0]));
            Assert.That(recentTargetPaths3[1], Is.EqualTo(recentTargetPathsToSave[1]));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
