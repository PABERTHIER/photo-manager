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
    public void WriteBackup_BackupFilePathHasNoDirectoryComponent_SkipsDirectoryCreationAndCreatesBackup()
    {
        const string backupFilePath = "test_no_dir_backup.zip";
        const string snapshotPath = backupFilePath + ".tmp.db";

        try
        {
            bool result = _backupService!.WriteBackup(backupFilePath);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(backupFilePath), Is.True);
        }
        finally
        {
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }

            if (File.Exists(snapshotPath))
            {
                File.Delete(snapshotPath);
            }
        }
    }

    [Test]
    [Platform("Win", Reason = "Only Windows blocks deleting a file that another stream holds open")]
    public void WriteBackup_SnapshotDeleteThrowsIOException_RetriesAndSucceeds()
    {
        string backupDirectory = Path.Combine(_databaseDirectory!, Constants.DATABASE_BACKUP_END_PATH);
        Directory.CreateDirectory(backupDirectory);
        InsertBackupPayload();

        string backupFilePath = Path.Combine(backupDirectory, "20240502.zip");
        string snapshotPath = backupFilePath + ".tmp.db";

        CancellationTokenSource lockCancellation = new();
        CancellationToken lockToken = lockCancellation.Token;
        ManualResetEventSlim lockAcquired = new(false);
        ManualResetEventSlim releaseLock = new(false);

        Task lockTask = Task.Run(() =>
        {
            while (!lockToken.IsCancellationRequested)
            {
                if (!File.Exists(snapshotPath))
                {
                    Thread.Sleep(1);
                    continue;
                }

                try
                {
                    using FileStream lockStream = new(snapshotPath, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite);
                    // ReSharper disable once AccessToDisposedClosure
                    lockAcquired.Set();
                    // ReSharper disable once AccessToDisposedClosure
                    releaseLock.Wait(lockToken);
                    return;
                }
                catch (Exception ex) when (ex is IOException or OperationCanceledException)
                {
                    Thread.Sleep(1);
                }
            }
        }, lockToken);

        Task backupTask = Task.Run(() => _backupService!.WriteBackup(backupFilePath), lockToken);

        try
        {
            Assert.That(lockAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            IOException? caughtException = Assert.Throws<IOException>(() => backupTask.GetAwaiter().GetResult());

            Assert.That(caughtException, Is.Not.Null);
            Assert.That(File.Exists(backupFilePath), Is.True);
        }
        finally
        {
            releaseLock.Set();
            lockCancellation.Cancel();

            try
            {
                backupTask.Wait(5000);
            }
            catch (AggregateException)
            {
            }

            lockTask.Wait(1000);

            lockCancellation.Dispose();
            lockAcquired.Dispose();
            releaseLock.Dispose();
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

    private void InsertBackupPayload()
    {
        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT INTO Folders (Id, Path) VALUES ($id, $path);";

                    SqliteParameter idParameter = command.Parameters.Add("$id", SqliteType.Text);
                    SqliteParameter pathParameter = command.Parameters.Add("$path", SqliteType.Text);
                    string payload = new('x', 4096);

                    for (int i = 0; i < 1000; i++)
                    {
                        idParameter.Value = Guid.NewGuid().ToString();
                        pathParameter.Value = $"{payload}-{i}";
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }
    }
}
