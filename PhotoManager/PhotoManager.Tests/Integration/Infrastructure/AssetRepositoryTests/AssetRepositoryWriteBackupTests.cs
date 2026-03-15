using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryWriteBackupTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

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
        _pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
            new BackupStorage(), new TestLogger<PhotoManager.Infrastructure.Database.Database>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _assetRepository = new(database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void WriteBackup_BackupWrittenAndMoreBackupsToKeep_WritesBackupAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_databasePath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            _assetRepository!.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(_assetRepository!.BackupExists(), Is.True);

            int filesInBackupDirectory = Directory.GetFiles(_databasePath! + "_Backups").Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenAndLessBackupsToKeep_WritesBackupAndDeletesOldBackups()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP,
            "0"); // 0 backups, so that, the new created is directly deleted

        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
            new BackupStorage(), new TestLogger<PhotoManager.Infrastructure.Database.Database>());
        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        AssetRepository assetRepository = new(database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_databasePath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(assetRepository.BackupExists(), Is.False);

            assetRepository.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(assetRepository.BackupExists(), Is.False);

            int filesInBackupDirectory = Directory.GetFiles(_databasePath! + "_Backups").Length;
            Assert.That(filesInBackupDirectory, Is.Zero);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
            string backupFilePath = Path.Combine(_databasePath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            _assetRepository!.WriteBackup();
            _assetRepository!.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(_assetRepository!.BackupExists(), Is.True);

            int filesInBackupDirectory = Directory.GetFiles(_databasePath! + "_Backups").Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_DatabaseThrowsException_LogsItAndThrowsException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_databasePath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");
            DirectoryNotFoundException expectedException =
                new($"Could not find a part of the path '{backupFilePath}'.");

            Directory.Delete(_databaseDirectory!, true);

            DirectoryNotFoundException? exception = Assert.Throws<DirectoryNotFoundException>(() =>
                _assetRepository!.WriteBackup());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assetsUpdatedEvents, Is.Empty);

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));
                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
            if (Directory.Exists(_databaseDirectory!))
            {
                Directory.Delete(_databaseDirectory!, true);
            }

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
            string backupFilePath = Path.Combine(_databasePath! + "_Backups", backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.WriteBackup(),
                () => _assetRepository!.WriteBackup(),
                () => _assetRepository!.WriteBackup()
            );

            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(_assetRepository!.BackupExists(), Is.True);

            int filesInBackupDirectory = Directory.GetFiles(_databasePath! + "_Backups").Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
