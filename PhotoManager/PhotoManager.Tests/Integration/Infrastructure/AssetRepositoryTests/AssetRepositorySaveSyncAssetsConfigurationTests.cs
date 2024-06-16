using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveSyncAssetsConfigurationTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void SaveSyncAssetsConfiguration_SyncAssetsConfiguration_SaveConfiguration()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Tutu\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Tutu",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });

            _assetRepository!.SaveSyncAssetsConfiguration(syncAssetsConfigurationToSave);
            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.AreEqual(2, syncAssetsConfiguration.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration.Definitions[1].DestinationDirectory);

            Assert.IsTrue(_assetRepository.HasChanges());

            _assetRepository!.SaveSyncAssetsConfiguration(new SyncAssetsConfiguration());
            syncAssetsConfiguration = _assetRepository.GetSyncAssetsConfiguration();

            Assert.AreEqual(0, syncAssetsConfiguration.Definitions.Count);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SyncAssetsConfiguration syncAssetsConfigurationToSave = new();
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
                {
                    SourceDirectory = "C:\\Toto\\Screenshots",
                    DestinationDirectory = "C:\\Images\\Toto",
                    IncludeSubFolders = false,
                    DeleteAssetsNotInSource = false
                });
            syncAssetsConfigurationToSave.Definitions.Add(
                new SyncAssetsDirectoriesDefinition
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

            Assert.AreEqual(2, syncAssetsConfiguration.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration.Definitions[1].DestinationDirectory);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
