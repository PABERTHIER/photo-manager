using Microsoft.Data.Sqlite;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqlitePersistenceContextTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;

    private SqliteConnectionFactory? _factory;
    private SqliteBackupService? _backupService;
    private SqlitePersistenceContext? _sqlitePersistenceContext;
    private TestLogger<SqlitePersistenceContext> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _factory = new SqliteConnectionFactory();
        _backupService = new SqliteBackupService(_factory);
        _sqlitePersistenceContext = new(_factory, _backupService, _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _sqlitePersistenceContext!.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void Initialize_ValidDirectory_CreatesDatabaseFile()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(File.Exists(_sqlitePersistenceContext!.DatabaseFilePath), Is.True);
        Assert.That(_sqlitePersistenceContext!.DatabaseFilePath,
            Is.EqualTo(Path.Combine(_databaseDirectory!, SqlitePersistenceContext.DATABASE_FILE_NAME)));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_ValidDirectory_CreatesBackupDirectory()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        string backupsDirectory = _databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;

        Assert.That(Directory.Exists(backupsDirectory), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_ValidDirectory_ExposesRepositories()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.Folders, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.Assets, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.Thumbnails, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.RecentPaths, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.SyncDefinitions, Is.Not.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void Initialize_EmptyString_ThrowsArgumentException(string dataDirectory)
    {
        const string expectedMessage = "dataDirectory must not be empty.";

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _sqlitePersistenceContext!.Initialize(dataDirectory));

        Assert.That(exception?.Message, Is.EqualTo($"{expectedMessage} (Parameter 'dataDirectory')"));
        Assert.That(exception.ParamName, Is.EqualTo("dataDirectory"));

        _testLogger.AssertLogExceptions(
            [new ArgumentException(expectedMessage, nameof(dataDirectory))],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_CalledTwice_IsIdempotent()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos");
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(1));

        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(1));

        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetById(folder.Id);
        Assert.That(retrievedFolder, Is.Not.Null);
        Assert.That(retrievedFolder!.Path, Is.EqualTo(@"C:\Photos"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DatabaseFilePath_BeforeInitialize_ReturnsEmpty()
    {
        Assert.That(_sqlitePersistenceContext!.DatabaseFilePath, Is.EqualTo(string.Empty));
    }

    [Test]
    public void WriteBackup_AfterInitialize_CreatesZipFile()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 3, 15);

        bool result = _sqlitePersistenceContext!.WriteBackup(backupDate);

        Assert.That(result, Is.True);

        string expectedFilePath = Path.Combine(_databaseDirectory + Constants.DATABASE_BACKUP_END_PATH, "20240315.zip");
        Assert.That(File.Exists(expectedFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _sqlitePersistenceContext!.WriteBackup(DateTime.Now));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_SameDate_OverwritesPreviousBackup()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 8, 20);

        _sqlitePersistenceContext!.WriteBackup(backupDate);

        string backupPath = Path.Combine(_databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH, "20240820.zip");

        long firstBackupSize = new FileInfo(backupPath).Length;

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder1");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder2");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder3");
        _sqlitePersistenceContext!.WriteBackup(backupDate);

        long secondBackupSize = new FileInfo(backupPath).Length;

        Assert.That(File.Exists(backupPath), Is.True);
        Assert.That(secondBackupSize, Is.GreaterThan(firstBackupSize));

        string backupsDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string[] files = Directory.GetFiles(backupsDirectory, "*.zip");

        Assert.That(files, Has.Length.EqualTo(1));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_AfterWriteBackup_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 5, 10);

        _sqlitePersistenceContext!.WriteBackup(backupDate);

        Assert.That(_sqlitePersistenceContext!.BackupExists(backupDate), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_NoBackup_ReturnsFalse()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.BackupExists(new DateTime(2024, 12, 25)), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.BackupExists(DateTime.Now));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepsSpecifiedCount()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 3));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 4));

        _sqlitePersistenceContext!.DeleteOldBackups(2);

        string backupsDirectory = _databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;
        string[] files = Directory.GetFiles(backupsDirectory, "*.zip");

        Assert.That(files, Has.Length.EqualTo(2));
        Assert.That(files.Any(f => f.Contains("20240103")), Is.True);
        Assert.That(files.Any(f => f.Contains("20240104")), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_NoBackups_NothingDeleted()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.DeleteOldBackups(5);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.DeleteOldBackups(5));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepMoreThanExist_DeletesNothing()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 7, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 7, 2));

        _sqlitePersistenceContext!.DeleteOldBackups(10);

        string backupsDirectory = _databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;
        string[] files = Directory.GetFiles(backupsDirectory, "*.zip");

        Assert.That(files, Has.Length.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepZero_DeletesAll()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 3));

        _sqlitePersistenceContext!.DeleteOldBackups(0);

        string backupsDirectory = _databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;
        string[] files = Directory.GetFiles(backupsDirectory, "*.zip");

        Assert.That(files, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_AfterInsertingData_BackupContainsData()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Family");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Travel");

        DateTime backupDate = new(2024, 6, 15);
        _sqlitePersistenceContext!.WriteBackup(backupDate);

        string backupPath = Path.Combine(_databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH, "20240615.zip");

        Assert.That(File.Exists(backupPath), Is.True);
        Assert.That(new FileInfo(backupPath).Length, Is.GreaterThan(0));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
    }

    [Test]
    public void Dispose_BeforeInitialize_DoesNotThrow()
    {
        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
    }

    [Test]
    public void Initialize_ValidDirectory_SetsSchemaVersion()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA user_version;";
                long version = Convert.ToInt64(command.ExecuteScalar());

                Assert.That(version, Is.EqualTo(1));
            }
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepOne_DeletesAllButNewest()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 3));

        _sqlitePersistenceContext!.DeleteOldBackups(1);

        string backupsDirectory = _databaseDirectory + Constants.DATABASE_BACKUP_END_PATH;
        string[] files = Directory.GetFiles(backupsDirectory, "*.zip");

        Assert.That(files, Has.Length.EqualTo(1));
        Assert.That(files[0], Does.Contain("20240303"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
