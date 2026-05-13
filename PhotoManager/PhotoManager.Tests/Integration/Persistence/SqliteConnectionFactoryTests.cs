using Microsoft.Data.Sqlite;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqliteConnectionFactoryTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void DatabasePath_BeforeInitialize_ReturnsEmpty()
    {
        SqliteConnectionFactory factory = new();

        Assert.That(factory.DatabasePath, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Initialize_ValidPath_SetsDatabasePath()
    {
        SqliteConnectionFactory factory = new();
        string databasePath = Path.Combine(_databaseDirectory!, "test.db");

        factory.Initialize(databasePath);

        Assert.That(factory.DatabasePath, Is.EqualTo(databasePath));
    }

    [Test]
    public void Open_BeforeInitialize_ThrowsInvalidOperationException()
    {
        SqliteConnectionFactory factory = new();

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => factory.Open());

        Assert.That(exception?.Message, Is.EqualTo(
            "The Db has not been initialized properly, the directory  does not exist."));
    }

    [Test]
    public void Open_AfterInitialize_ReturnsOpenConnection()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);

        string databasePath = Path.Combine(_databaseDirectory!, "test_open.db");
        factory.Initialize(databasePath);

        using (SqliteConnection connection = factory.Open())
        {
            Assert.That(connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
            Assert.That(connection.DataSource, Is.EqualTo(databasePath));
        }
    }

    [Test]
    public void Open_AfterInitialize_AppliesWalJournalMode()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_wal.db");

        factory.Initialize(databasePath);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode;";
                string? journalMode = command.ExecuteScalar()?.ToString();

                Assert.That(journalMode, Is.EqualTo("wal"));
            }
        }
    }

    [Test]
    public void Open_AfterInitialize_EnablesForeignKeys()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_fk.db");

        factory.Initialize(databasePath);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys;";
                long? foreignKeys = (long?)command.ExecuteScalar();

                Assert.That(foreignKeys, Is.EqualTo(1));
            }
        }
    }

    [Test]
    public void Open_AfterInitialize_SetsSynchronousToNormal()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_sync.db");

        factory.Initialize(databasePath);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA synchronous;";
                long? synchronous = (long?)command.ExecuteScalar();

                // NORMAL = 1
                Assert.That(synchronous, Is.EqualTo(1));
            }
        }
    }

    [Test]
    public void Open_AfterInitialize_SetsTempStoreToMemory()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_temp.db");

        factory.Initialize(databasePath);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA temp_store;";
                long? tempStore = (long?)command.ExecuteScalar();

                // MEMORY = 2
                Assert.That(tempStore, Is.EqualTo(2));
            }
        }
    }

    [Test]
    public void Open_MultipleCalls_ReturnsIndependentConnections()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "test_multi.db");

        factory.Initialize(databasePath);

        using (SqliteConnection connection1 = factory.Open())
        {
            using (SqliteConnection connection2 = factory.Open())
            {
                Assert.That(connection1, Is.Not.SameAs(connection2));
                Assert.That(connection1.State, Is.EqualTo(System.Data.ConnectionState.Open));
                Assert.That(connection2.State, Is.EqualTo(System.Data.ConnectionState.Open));
            }
        }
    }

    [Test]
    public void Initialize_CalledTwice_OverwritesPreviousPath()
    {
        SqliteConnectionFactory factory = new();

        string firstPath = Path.Combine(_databaseDirectory!, "first.db");
        string secondPath = Path.Combine(_databaseDirectory!, "second.db");

        factory.Initialize(firstPath);
        factory.Initialize(secondPath);

        Assert.That(factory.DatabasePath, Is.EqualTo(secondPath));
    }

    [Test]
    public void Open_CreatesDbFileIfNotExists()
    {
        SqliteConnectionFactory factory = new();

        Directory.CreateDirectory(_databaseDirectory!);
        string databasePath = Path.Combine(_databaseDirectory!, "new_db.db");

        factory.Initialize(databasePath);

        Assert.That(File.Exists(databasePath), Is.False);

        using (SqliteConnection connection = factory.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Test (Id INTEGER);";
                command.ExecuteNonQuery();
            }
        }

        Assert.That(File.Exists(databasePath), Is.True);
    }
}
