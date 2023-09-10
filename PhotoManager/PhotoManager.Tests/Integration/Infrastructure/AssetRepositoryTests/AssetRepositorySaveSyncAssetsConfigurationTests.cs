using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveSyncAssetsConfigurationTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRoot = new Mock<IConfigurationRoot>();
        _configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _assetRepository = new AssetRepository(database, _storageService!.Object, userConfigurationService);
    }

    [Test]
    public void SaveSyncAssetsConfiguration_SyncAssetsConfiguration_SaveConfiguration()
    {
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
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void SaveSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
    {
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
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
