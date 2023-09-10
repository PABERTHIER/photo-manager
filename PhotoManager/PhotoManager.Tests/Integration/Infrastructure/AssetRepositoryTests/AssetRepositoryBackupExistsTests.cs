using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryBackupExistsTests
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
    public void BackupExists_BackupExists_ReturnsTrue()
    {
        try
        {
            _assetRepository!.WriteBackup();

            Assert.IsTrue(_assetRepository!.BackupExists());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void BackupExists_BackupDoesNotExist_ReturnsTrue()
    {
        try
        {
            Assert.IsFalse(_assetRepository!.BackupExists());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void BackupExists_ConcurrentAccess_CheckExistenceIsHandledSafely()
    {
        try
        {
            _assetRepository!.WriteBackup();

            bool backupExists1 = false;
            bool backupExists2 = false;
            bool backupExists3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => backupExists1 = _assetRepository!.BackupExists(),
                () => backupExists2 = _assetRepository!.BackupExists(),
                () => backupExists3 = _assetRepository!.BackupExists()
            );

            Assert.IsTrue(backupExists1);
            Assert.IsTrue(backupExists2);
            Assert.IsTrue(backupExists3);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
