using Microsoft.Data.Sqlite;
using System.IO.Compression;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqliteBackupServiceTests
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
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);
    }

    [TearDown]
    public void TearDown()
    {
        _sqlitePersistenceContext!.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void WriteBackup_ValidPath_CreatesZipFile()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240101.zip");

        bool result = _backupService!.WriteBackup(backupFilePath);

        Assert.That(result, Is.True);
        Assert.That(File.Exists(backupFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_ValidPath_ZipContainsPhotomanagerDb()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240102.zip");

        _backupService!.WriteBackup(backupFilePath);

        using (ZipArchive archive = ZipFile.OpenRead(backupFilePath))
        {
            Assert.That(archive.Entries, Has.Count.EqualTo(1));
            Assert.That(archive.Entries[0].Name, Is.EqualTo("photomanager.db"));
            Assert.That(archive.Entries[0].Length, Is.GreaterThan(0));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_ExistingBackupFile_OverwritesPrevious()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240103.zip");

        _backupService!.WriteBackup(backupFilePath);

        long firstSize = new FileInfo(backupFilePath).Length;

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\One");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Two");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Three");

        _backupService!.WriteBackup(backupFilePath);

        long secondSize = new FileInfo(backupFilePath).Length;

        Assert.That(secondSize, Is.GreaterThan(firstSize));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_DirectoryDoesNotExist_CreatesDirectory()
    {
        string newDirectory = Path.Combine(_databaseDirectory!, "NewBackupDir");
        string backupFilePath = Path.Combine(newDirectory, "20240104.zip");

        Assert.That(Directory.Exists(newDirectory), Is.False);

        _backupService!.WriteBackup(backupFilePath);

        Assert.That(Directory.Exists(newDirectory), Is.True);
        Assert.That(File.Exists(backupFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_CleansUpTmpFile()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240105.zip");

        _backupService!.WriteBackup(backupFilePath);

        string snapshotPath = backupFilePath + ".tmp.db";

        Assert.That(File.Exists(snapshotPath), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_BackupContainsValidSqliteDatabase()
    {
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Vacation");

        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240106.zip");

        _backupService!.WriteBackup(backupFilePath);

        string extractDirectory = Path.Combine(_databaseDirectory!, "extract_test");
        ZipFile.ExtractToDirectory(backupFilePath, extractDirectory);

        string extractedDbFile = Path.Combine(extractDirectory, "photomanager.db");
        Assert.That(File.Exists(extractedDbFile), Is.True);

        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = extractedDbFile,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        };

        using (SqliteConnection extractedConnection = new(builder.ConnectionString))
        {
            extractedConnection.Open();

            using (SqliteCommand command = extractedConnection.CreateCommand())
            {
                command.CommandText = "SELECT Path FROM Folders WHERE Path = 'C:\\Photos\\Vacation';";
                string? path = command.ExecuteScalar()?.ToString();

                Assert.That(path, Is.EqualTo(@"C:\Photos\Vacation"));
            }
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_DirectoryWithZipFiles_ReturnsFilePaths()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;

        _backupService!.WriteBackup(Path.Combine(backupDirectory, "20240101.zip"));
        _backupService!.WriteBackup(Path.Combine(backupDirectory, "20240102.zip"));
        _backupService!.WriteBackup(Path.Combine(backupDirectory, "20240103.zip"));

        string[] paths = _backupService!.GetBackupFilesPaths(backupDirectory);

        Assert.That(paths, Has.Length.EqualTo(3));
        Assert.That(paths.Any(p => p.Contains("20240101.zip")), Is.True);
        Assert.That(paths.Any(p => p.Contains("20240102.zip")), Is.True);
        Assert.That(paths.Any(p => p.Contains("20240103.zip")), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_NonExistentDirectory_ReturnsEmptyArray()
    {
        string nonExistentDirectory = Path.Combine(_databaseDirectory!, "NonExistentDir");

        string[] paths = _backupService!.GetBackupFilesPaths(nonExistentDirectory);

        Assert.That(paths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_EmptyDirectory_ReturnsEmptyArray()
    {
        string emptyDirectory = Path.Combine(_databaseDirectory!, "EmptyBackupsDir");
        Directory.CreateDirectory(emptyDirectory);

        string[] paths = _backupService!.GetBackupFilesPaths(emptyDirectory);

        Assert.That(paths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetBackupFilesPaths_DirectoryWithNonZipFiles_ReturnsOnlyZipFiles()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;

        _backupService!.WriteBackup(Path.Combine(backupDirectory, "20240101.zip"));

        File.WriteAllText(Path.Combine(backupDirectory, "notes.txt"), "Some notes");
        File.WriteAllText(Path.Combine(backupDirectory, "data.db"), "Not a zip");

        string[] paths = _backupService!.GetBackupFilesPaths(backupDirectory);

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Does.Contain("20240101.zip"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteBackupFile_ExistingFile_RemovesFile()
    {
        string backupDirectory = _databaseDirectory! + Constants.DATABASE_BACKUP_END_PATH;
        string backupFilePath = Path.Combine(backupDirectory, "20240201.zip");

        _backupService!.WriteBackup(backupFilePath);

        Assert.That(File.Exists(backupFilePath), Is.True);

        _backupService!.DeleteBackupFile(backupFilePath);

        Assert.That(File.Exists(backupFilePath), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteBackupFile_NonExistentFile_DoesNotThrow()
    {
        string nonExistentFile = Path.Combine(_databaseDirectory!, "nonexistent.zip");

        Assert.DoesNotThrow(() => _backupService!.DeleteBackupFile(nonExistentFile));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
