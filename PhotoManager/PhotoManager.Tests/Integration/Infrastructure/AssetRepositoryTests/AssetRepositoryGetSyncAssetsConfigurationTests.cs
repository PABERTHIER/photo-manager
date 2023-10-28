using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetSyncAssetsConfigurationTests
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
    public void GetSyncAssetsConfiguration_SyncAssetsConfiguration_ReturnsSyncAssetsConfiguration()
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
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetSyncAssetsConfiguration_NoSyncAssetsConfiguration_ReturnsEmptyList()
    {
        try
        {
            SyncAssetsConfiguration syncAssetsConfiguration = _assetRepository!.GetSyncAssetsConfiguration();

            Assert.AreEqual(0, syncAssetsConfiguration.Definitions.Count);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetSyncAssetsConfiguration_ConcurrentAccess_SyncAssetsConfigurationIsHandledSafely()
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

            SyncAssetsConfiguration syncAssetsConfiguration1 = new();
            SyncAssetsConfiguration syncAssetsConfiguration2 = new();
            SyncAssetsConfiguration syncAssetsConfiguration3 = new();

            // Simulate concurrent access
            Parallel.Invoke(
                () => syncAssetsConfiguration1 = _assetRepository.GetSyncAssetsConfiguration(),
                () => syncAssetsConfiguration2 = _assetRepository.GetSyncAssetsConfiguration(),
                () => syncAssetsConfiguration3 = _assetRepository.GetSyncAssetsConfiguration()
            );

            Assert.AreEqual(2, syncAssetsConfiguration1.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration1.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration1.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration1.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration1.Definitions[1].DestinationDirectory);

            Assert.AreEqual(2, syncAssetsConfiguration2.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration2.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration2.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration2.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration2.Definitions[1].DestinationDirectory);

            Assert.AreEqual(2, syncAssetsConfiguration3.Definitions.Count);
            Assert.AreEqual("C:\\Toto\\Screenshots", syncAssetsConfiguration3.Definitions[0].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Toto", syncAssetsConfiguration3.Definitions[0].DestinationDirectory);
            Assert.AreEqual("C:\\Tutu\\Screenshots", syncAssetsConfiguration3.Definitions[1].SourceDirectory);
            Assert.AreEqual("C:\\Images\\Tutu", syncAssetsConfiguration3.Definitions[1].DestinationDirectory);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
