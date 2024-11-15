﻿using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryWriteBackupTests
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
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(backupPath!);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void WriteBackup_BackupWrittenAndMoreBackupsToKeep_WritesBackupAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
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

        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(backupPath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            assetRepository.WriteBackup();

            Assert.IsFalse(File.Exists(backupFilePath));
            Assert.IsFalse(assetRepository.BackupExists());

            int filesInBackupDirectory = Directory.GetFiles(backupPath! + "_Backups").Length;
            Assert.AreEqual(0, filesInBackupDirectory);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenTwiceAndMoreBackupsToKeep_WritesBackupOnceAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_ConcurrentAccess_BackupsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
