using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryWriteBackupTests
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
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath!);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _assetRepository = new AssetRepository(database, _storageService!.Object, userConfigurationService);
    }

    [Test]
    public void WriteBackup_BackupWrittenAndMoreBackupsToKeep_WritesBackupAndDoesNotDeleteOldBackups()
    {
        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(_assetRepository!.BackupExists());

            _assetRepository!.WriteBackup();

            Assert.IsTrue(File.Exists(backupFilePath));
            Assert.IsTrue(_assetRepository!.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenAndLessBackupsToKeep_WritesBackupAndDeletesOldBackups()
    {
        try
        {
            Mock<IConfigurationRoot> configurationRoot = new();
            configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "0") // 0 backups to delete the new created
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
            UserConfigurationService userConfigurationService = new(configurationRoot.Object);
            AssetRepository assetRepository = new(database, _storageService!.Object, userConfigurationService);

            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            assetRepository.WriteBackup();

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(backupPath! + "_Backups").Length;
            Assert.AreEqual(0, filesInBackupDirectory);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenTwiceAndMoreBackupsToKeep_WritesBackupOnceAndDoesNotDeleteOldBackups()
    {
        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(_assetRepository!.BackupExists());

            _assetRepository!.WriteBackup();
            _assetRepository!.WriteBackup();

            Assert.IsTrue(File.Exists(backupFilePath));
            Assert.IsTrue(_assetRepository!.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void WriteBackup_ConcurrentAccess_BackupsAreHandledSafely()
    {
        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(_assetRepository!.BackupExists());

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.WriteBackup(),
                () => _assetRepository!.WriteBackup(),
                () => _assetRepository!.WriteBackup()
            );

            Assert.IsTrue(File.Exists(backupFilePath));
            Assert.IsTrue(_assetRepository!.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
