using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveSyncAssetsConfigurationTests
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
    public void SaveSyncAssetsConfiguration_SyncAssetsConfiguration_SavesConfiguration()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave);
            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));


            _assetRepository!.SaveSyncAssetsConfiguration(new());
            syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Is.Empty);


            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new()
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave),
                () => _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave)
            );

            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository!.GetSyncAssetsConfiguration();

            Assert.That(syncAssetsConfiguration.Definitions, Has.Count.EqualTo(2));
            Assert.That(syncAssetsConfiguration.Definitions[0].SourceDirectory, Is.EqualTo("C:\\Toto\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[0].DestinationDirectory, Is.EqualTo("C:\\Images\\Toto"));
            Assert.That(syncAssetsConfiguration.Definitions[1].SourceDirectory, Is.EqualTo("C:\\Tutu\\Screenshots"));
            Assert.That(syncAssetsConfiguration.Definitions[1].DestinationDirectory, Is.EqualTo("C:\\Images\\Tutu"));


            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
