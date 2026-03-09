using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteOldBackupsTests
{
    private string? _dataDirectory;

    private UserConfigurationService? _userConfigurationService;
    private Mock<IBackupStorage>? _backupStorageMock;
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
        _backupStorageMock = new();
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void DeleteOldBackups_ExcessBackups_SuccessfullyDeletesBackups()
    {
        const ushort backupsToKeep = 3;
        string path1 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230404.zip");
        string path2 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230304.zip");
        string path3 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230204.zip");
        string path4 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230104.zip");
        string path5 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230504.zip");

        string[] filesPath =
        [
            path1,
            path2,
            path3,
            path4,
            path5
        ];

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _backupStorageMock!.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns(filesPath);

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
                _backupStorageMock.Object, _testLogger);
            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            database.DeleteOldBackups(backupsToKeep);

            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(It.IsAny<string>()), Times.Exactly(2));
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path1), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path2), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path5), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path3), Times.Once);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path4), Times.Once);

            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths, Is.Not.Null);
            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths!, Has.Length.EqualTo(2));
            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths[0], Is.EqualTo(path4));
            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths[1], Is.EqualTo(path3));

            _testLogger.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void DeleteOldBackups_NoExcessBackups_NothingDeleted()
    {
        const ushort backupsToKeep = 3;
        string[] filesPath =
        [
            Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230404.zip"),
            Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230304.zip"),
            Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230204.zip")
        ];

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _backupStorageMock!.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns(filesPath);

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(),
                _backupStorageMock.Object, _testLogger);
            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            database.DeleteOldBackups(backupsToKeep);

            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(It.IsAny<string>()), Times.Never);

            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths, Is.Not.Null);
            Assert.That(database.Diagnostics.LastDeletedBackupFilePaths!, Is.Empty);

            _testLogger.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void DeleteOldBackups_ExceptionThrown_LogsItAndThrowsException()
    {
        const ushort backupsToKeep = 1;
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string[] filesPaths =
        [
            Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230404.zip"),
            Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230304.zip")
        ];

        string logMessage = $"Error while trying to delete old backups. Backups to keep: {backupsToKeep}";
        string exceptionMessage =
            $"The process cannot access the file '{filesPaths[1]}' because it is being used by another process.";
        IOException expectedException = new(exceptionMessage);

        try
        {
            _backupStorageMock!.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns(filesPaths);
            _backupStorageMock.Setup(x => x.DeleteBackupFile(It.IsAny<string>())).Throws(expectedException);

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(),
                new BlobStorage(), _backupStorageMock.Object, _testLogger);

            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            IOException? exception = Assert.Throws<IOException>(() => database.DeleteOldBackups(backupsToKeep));
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
