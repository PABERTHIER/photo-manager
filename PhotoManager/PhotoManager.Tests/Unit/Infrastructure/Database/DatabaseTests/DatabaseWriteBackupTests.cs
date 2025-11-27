using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBackupTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
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
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
