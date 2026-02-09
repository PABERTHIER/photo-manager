using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteOldBackupsTests
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

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void DeleteOldBackups_DeletesExcessBackups_SuccessfullyDeletesBackups()
    {
        const ushort backupsToKeep = 3;
        string path1 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230404.zip");
        string path2 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230304.zip");
        string path3 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230204.zip");
        string path4 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230104.zip");
        string path5 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230504.zip");

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated1 = _database!.WriteBackup(new(2023, 04, 04));
            bool backupCreated2 = _database!.WriteBackup(new(2023, 03, 04));
            bool backupCreated3 = _database!.WriteBackup(new(2023, 02, 04));
            bool backupCreated4 = _database!.WriteBackup(new(2023, 01, 04));
            bool backupCreated5 = _database!.WriteBackup(new(2023, 05, 04));

            Assert.That(backupCreated1, Is.True);
            Assert.That(backupCreated2, Is.True);
            Assert.That(backupCreated3, Is.True);
            Assert.That(backupCreated4, Is.True);
            Assert.That(backupCreated5, Is.True);

            _database!.DeleteOldBackups(backupsToKeep);

            Assert.That(File.Exists(path1), Is.True);
            Assert.That(File.Exists(path2), Is.True);
            Assert.That(File.Exists(path3), Is.False);
            Assert.That(File.Exists(path4), Is.False);
            Assert.That(File.Exists(path5), Is.True);

            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths, Is.Not.Null);
            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths!, Has.Length.EqualTo(2));
            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths![0], Is.EqualTo(path4));
            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths[1], Is.EqualTo(path3));
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
        string path1 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230404.zip");
        string path2 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230304.zip");
        string path3 = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS, "20230204.zip");

        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated1 = _database!.WriteBackup(new(2023, 04, 04));
            bool backupCreated2 = _database!.WriteBackup(new(2023, 03, 04));
            bool backupCreated3 = _database!.WriteBackup(new(2023, 02, 04));

            Assert.That(backupCreated1, Is.True);
            Assert.That(backupCreated2, Is.True);
            Assert.That(backupCreated3, Is.True);

            _database!.DeleteOldBackups(backupsToKeep);

            Assert.That(File.Exists(path1), Is.True);
            Assert.That(File.Exists(path2), Is.True);
            Assert.That(File.Exists(path3), Is.True);

            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths, Is.Not.Null);
            Assert.That(_database!.Diagnostics.LastDeletedBackupFilePaths!, Is.Empty);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
