using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBackupTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new TestLogger<PhotoManager.Infrastructure.Database.Database>();
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void WriteBackup_BackupDoesNotExist_SuccessfullyWritesBackup()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, backupName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated = _database!.WriteBackup(backupDate);

            Assert.That(backupCreated, Is.True);
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(filePath));

            _testLogger.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteBackup_BackupExists_OverwritesBackup()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, backupName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated1 = _database!.WriteBackup(backupDate);
            bool backupCreated2 = _database!.WriteBackup(backupDate);

            Assert.That(backupCreated1, Is.True);
            Assert.That(backupCreated2, Is.True);
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(filePath));

            _testLogger.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteBackup_ExceptionIsThrown_LogsItAndThrowsException()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string filePath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, backupName);

        string logMessage =
            $"Error while trying to write backup for date {backupDate:yyyyMMdd}. LastWriteFilePath: {filePath}";
        const string exceptionMessage = "Backup write error";
        IOException expectedException = new(exceptionMessage);

        try
        {
            Mock<IBackupStorage> backupStorageMock = new();
            backupStorageMock.Setup(x => x.WriteFolderToZipFile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(expectedException);
            backupStorageMock.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns([]);

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
                backupStorageMock.Object, _testLogger);

            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            IOException? exception = Assert.Throws<IOException>(() => database.WriteBackup(backupDate));
            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

            _testLogger.AssertLogErrors([logMessage], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            string backupsPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS);

            if (Directory.Exists(backupsPath))
            {
                Directory.Delete(backupsPath, true);
            }
        }
    }
}
