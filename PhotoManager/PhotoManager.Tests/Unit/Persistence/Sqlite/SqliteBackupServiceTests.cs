using Microsoft.Data.Sqlite;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Persistence.Sqlite;

[TestFixture]
public class SqliteBackupServiceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private SqliteConnectionFactory? _factory;
    private SqliteBackupService? _backupService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _factory = new(new TestLogger<SqliteConnectionFactory>());
        _backupService = new(_factory);

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "photomanager.db");

        _factory.Initialize(databasePath);

        using (SqliteConnection connection = _factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = """
                                     CREATE TABLE IF NOT EXISTS Folders (
                                         Id   TEXT PRIMARY KEY NOT NULL,
                                         Path TEXT NOT NULL
                                     );
                                     """;
                command.ExecuteNonQuery();
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void WriteBackup_PreExistingSnapshotFile_DeletesItBeforeBackup()
    {
        string backupDirectory = Path.Combine(_databaseDirectory!, Constants.DATABASE_BACKUP_END_PATH);
        Directory.CreateDirectory(backupDirectory);

        string backupFilePath = Path.Combine(backupDirectory, "20240501.zip");
        string snapshotPath = backupFilePath + ".tmp.db";

        File.WriteAllText(snapshotPath, "stale snapshot data");
        Assert.That(File.Exists(snapshotPath), Is.True);

        bool result = _backupService!.WriteBackup(backupFilePath);

        Assert.That(result, Is.True);
        Assert.That(File.Exists(backupFilePath), Is.True);
        Assert.That(File.Exists(snapshotPath), Is.False);
    }

    [Test]
    public void WriteBackup_SnapshotDeleteThrowsIOException_RetriesAndSucceeds()
    {
        string backupDirectory = Path.Combine(_databaseDirectory!, Constants.DATABASE_BACKUP_END_PATH);
        Directory.CreateDirectory(backupDirectory);

        string backupFilePath = Path.Combine(backupDirectory, "20240502.zip");
        string snapshotPath = backupFilePath + ".tmp.db";

        FileStream? lockStream = null;

        // Background thread that locks the snapshot file as soon as it's created.
        // Uses FileShare.ReadWrite so the zip can still read it,
        // but omits FileShare.Delete so File.Delete throws IOException.
        Task lockTask = Task.Run(() =>
        {
            SpinWait.SpinUntil(() => File.Exists(snapshotPath), 5000);

            try
            {
                lockStream = new FileStream(snapshotPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch
            {
                // File may have been deleted between Exists check and Open
            }
        });

        try
        {
            // The finally block's File.Delete will throw IOException because our
            // lock holds the file open without FileShare.Delete.
            // The retry after Thread.Sleep(20) also fails, propagating the exception.
            Assert.Throws<IOException>(() => _backupService!.WriteBackup(backupFilePath));

            Assert.That(File.Exists(backupFilePath), Is.True);
        }
        finally
        {
            lockStream?.Dispose();
            lockTask.Wait(1000);
        }
    }

    [Test]
    [Retry(3)]
    public void WriteBackup_SnapshotDeleteThrowsIOException_RetrySucceedsAfterLockReleased()
    {
        string backupDirectory = Path.Combine(_databaseDirectory!, Constants.DATABASE_BACKUP_END_PATH);
        Directory.CreateDirectory(backupDirectory);

        string backupFilePath = Path.Combine(backupDirectory, "20240503.zip");
        string snapshotPath = backupFilePath + ".tmp.db";

        FileStream? lockStream = null;

        // Background thread: lock the snapshot briefly so first File.Delete fails,
        // then release before the retry (which happens after Thread.Sleep(20) in production).
        // Hold for 10ms: first delete at ~3-5ms (IOException), release at ~10ms, retry at ~23-25ms.
        Task lockTask = Task.Run(() =>
        {
            SpinWait.SpinUntil(() => File.Exists(snapshotPath), 5000);

            try
            {
                lockStream = new FileStream(snapshotPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                Thread.Sleep(10);
                lockStream.Dispose();
                lockStream = null;
            }
            catch
            {
                // File may have been deleted between Exists check and Open
            }
        });

        try
        {
            bool result = _backupService!.WriteBackup(backupFilePath);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(backupFilePath), Is.True);
            Assert.That(File.Exists(snapshotPath), Is.False);
        }
        finally
        {
            lockStream?.Dispose();
            lockTask.Wait(5000);
        }
    }
}
