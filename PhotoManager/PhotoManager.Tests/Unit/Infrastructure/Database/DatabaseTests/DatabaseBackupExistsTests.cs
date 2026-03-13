using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseBackupExistsTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database>? _testLogger;

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
        _testLogger = new();
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void BackupExists_BackupExists_ReturnsTrue()
    {
        DateTime backupDate = DateTime.Now;

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated = _database!.WriteBackup(backupDate);
            bool backupExists = _database!.BackupExists(backupDate);

            Assert.That(backupCreated, Is.True);
            Assert.That(backupExists, Is.True);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void BackupExists_BackupDoesNotExist_ReturnsFalse()
    {
        DateTime backupDate = DateTime.Now;

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupExists = _database!.BackupExists(backupDate);

            Assert.That(backupExists, Is.False);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
