using Microsoft.Data.Sqlite;
using System.Data;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqliteConnectionFactoryTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private SqliteConnectionFactory? _sqliteConnectionFactory;
    private TestLogger<SqliteConnectionFactory>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _sqliteConnectionFactory = new(_testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void DatabasePath_BeforeInitialize_ReturnsEmpty()
    {
        Assert.That(_sqliteConnectionFactory!.DatabasePath, Is.EqualTo(string.Empty));

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Initialize_ValidPath_SetsDatabasePath()
    {
        string databasePath = Path.Combine(_databaseDirectory!, "test.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        Assert.That(_sqliteConnectionFactory!.DatabasePath, Is.EqualTo(databasePath));

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_BeforeInitialize_ThrowsInvalidOperationException()
    {
        InvalidOperationException expectedException = new(
            "The Db has not been initialized properly, the directory  does not exist.");

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqliteConnectionFactory!.Open());

        Assert.That(exception?.Message, Is.EqualTo(expectedException.Message));

        _testLogger!.AssertLogExceptions([expectedException], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_AfterInitialize_ReturnsOpenConnection()
    {
        Directory.CreateDirectory(_databaseDirectory!);

        string databasePath = Path.Combine(_databaseDirectory!, "test_open.db");
        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
            Assert.That(connection.DataSource, Is.EqualTo(databasePath));
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_AfterInitialize_AppliesWalJournalMode()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_wal.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode;";
                string? journalMode = command.ExecuteScalar()?.ToString();

                Assert.That(journalMode, Is.EqualTo("wal"));
            }
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_AfterInitialize_EnablesForeignKeys()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_fk.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys;";
                long? foreignKeys = (long?)command.ExecuteScalar();

                Assert.That(foreignKeys, Is.EqualTo(1));
            }
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_AfterInitialize_SetsSynchronousToNormal()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_sync.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA synchronous;";
                long? synchronous = (long?)command.ExecuteScalar();

                // NORMAL = 1
                Assert.That(synchronous, Is.EqualTo(1));
            }
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_AfterInitialize_SetsTempStoreToMemory()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_temp.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA temp_store;";
                long? tempStore = (long?)command.ExecuteScalar();

                // MEMORY = 2
                Assert.That(tempStore, Is.EqualTo(2));
            }
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_MultipleCalls_ReturnsIndependentConnections()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_multi.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        using (SqliteConnection connection1 = _sqliteConnectionFactory!.Open())
        {
            using (SqliteConnection connection2 = _sqliteConnectionFactory!.Open())
            {
                Assert.That(connection1, Is.Not.SameAs(connection2));
                Assert.That(connection1.State, Is.EqualTo(ConnectionState.Open));
                Assert.That(connection2.State, Is.EqualTo(ConnectionState.Open));
            }
        }

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Initialize_CalledTwice_OverwritesPreviousPath()
    {
        string firstPath = Path.Combine(_databaseDirectory!, "first.db");
        string secondPath = Path.Combine(_databaseDirectory!, "second.db");

        _sqliteConnectionFactory!.Initialize(firstPath);
        _sqliteConnectionFactory!.Initialize(secondPath);

        Assert.That(_sqliteConnectionFactory!.DatabasePath, Is.EqualTo(secondPath));

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }

    [Test]
    public void Open_CreatesDbFileIfNotExists()
    {
        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "new_db.db");

        _sqliteConnectionFactory!.Initialize(databasePath);

        Assert.That(File.Exists(databasePath), Is.False);

        using (SqliteConnection connection = _sqliteConnectionFactory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Test (Id INTEGER);";
                command.ExecuteNonQuery();
            }
        }

        Assert.That(File.Exists(databasePath), Is.True);

        _testLogger!.AssertLogExceptions([], typeof(SqliteConnectionFactory));
    }
}
