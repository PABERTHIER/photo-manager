using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryWriteBackupTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath!);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void WriteBackup_BackupWrittenAndMoreBackupsToKeep_WritesBackupAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(_assetRepository!.BackupExists());

            _assetRepository!.WriteBackup();

            Assert.IsTrue(File.Exists(backupFilePath));
            Assert.IsTrue(_assetRepository!.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(_backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenAndLessBackupsToKeep_WritesBackupAndDeletesOldBackups()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP, "0"); // 0 backups, so that, the new created is directly deleted

        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);
        AssetRepository assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            assetRepository.WriteBackup();

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(_backupPath! + "_Backups").Length;
            Assert.AreEqual(0, filesInBackupDirectory);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenTwiceAndMoreBackupsToKeep_WritesBackupOnceAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(_assetRepository!.BackupExists());

            _assetRepository!.WriteBackup();
            _assetRepository!.WriteBackup();

            Assert.IsTrue(File.Exists(backupFilePath));
            Assert.IsTrue(_assetRepository!.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(_backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_ConcurrentAccess_BackupsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

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

            int filesInBackupDirectory = Directory.GetFiles(_backupPath! + "_Backups").Length;
            Assert.AreEqual(1, filesInBackupDirectory);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
