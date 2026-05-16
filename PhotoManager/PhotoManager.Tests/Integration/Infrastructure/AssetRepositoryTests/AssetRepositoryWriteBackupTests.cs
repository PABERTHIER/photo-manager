using Microsoft.Data.Sqlite;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryWriteBackupTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _backupsDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _backupsDirectory = Path.Combine(_databaseDirectory, Constants.DATABASE_BACKUP_END_PATH);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databaseDirectory);
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
    public void WriteBackup_BackupWrittenAndMoreBackupsToKeep_WritesBackupAndDoesNotDeleteOldBackups()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupsDirectory!, backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            _assetRepository!.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(_assetRepository!.BackupExists(), Is.True);

            int filesInBackupDirectory = Directory.GetFiles(_backupsDirectory!).Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_BackupWrittenAndLessBackupsToKeep_WritesBackupAndDeletesOldBackups()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        // 0 backups, so that, the new created is directly deleted
        configurationRootMock.MockGetValue(UserConfigurationKeys.BACKUPS_TO_KEEP, "0");

        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(configurationRootMock);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        AssetRepository assetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, _testLogger!);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            DateTime backupDate = DateTime.Now;
            string backupFilePath = Path.Combine(_backupsDirectory!, backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(assetRepository.BackupExists(), Is.False);

            assetRepository.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(assetRepository.BackupExists(), Is.False);

            int filesInBackupDirectory = Directory.GetFiles(_backupsDirectory!).Length;
            Assert.That(filesInBackupDirectory, Is.Zero);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetRepository.Dispose();
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
            string backupFilePath = Path.Combine(_backupsDirectory!, backupDate.ToString("yyyyMMdd") + ".zip");

            Assert.That(File.Exists(backupFilePath), Is.False);
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            _assetRepository!.WriteBackup();
            _assetRepository!.WriteBackup();

            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(_assetRepository!.BackupExists(), Is.True);

            int filesInBackupDirectory = Directory.GetFiles(_backupsDirectory!).Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void WriteBackup_DatabaseThrowsException_LogsItAndThrowsSqliteException()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            SqliteException expectedException = new("SQLite Error 14: 'unable to open database file'.", 14);

            SqliteConnection.ClearAllPools();
            Directory.Delete(_databaseDirectory!, true);

            using (Assert.EnterMultipleScope())
            {
                SqliteException? exception = Assert.Throws<SqliteException>(() => _assetRepository!.WriteBackup());

                Assert.That(assetsUpdatedEvents, Is.Empty);

                Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

                _testLogger!.AssertLogExceptions([expectedException], typeof(AssetRepository));
            }
        }
        finally
        {
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
            string backupFilePath = Path.Combine(_backupsDirectory!, backupDate.ToString("yyyyMMdd") + ".zip");

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

            int filesInBackupDirectory = Directory.GetFiles(_backupsDirectory!).Length;
            Assert.That(filesInBackupDirectory, Is.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
