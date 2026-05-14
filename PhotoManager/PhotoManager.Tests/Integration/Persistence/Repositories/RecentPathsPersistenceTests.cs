using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class RecentPathsPersistenceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;

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
        SqliteConnectionFactory factory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(factory);
        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
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
    public void GetAll_EmptyTable_ReturnsEmptyList()
    {
        IReadOnlyList<string> paths = _sqlitePersistenceContext!.RecentPaths.GetAll();

        Assert.That(paths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_WithPaths_StoresInOrder()
    {
        List<string> paths =
        [
            @"C:\Photos\Recent1",
            @"C:\Photos\Recent2",
            @"C:\Photos\Recent3"
        ];

        _sqlitePersistenceContext!.RecentPaths.Replace(paths);

        IReadOnlyList<string> retrievedPaths = _sqlitePersistenceContext!.RecentPaths.GetAll();

        Assert.That(retrievedPaths, Has.Count.EqualTo(3));
        Assert.That(retrievedPaths[0], Is.EqualTo(@"C:\Photos\Recent1"));
        Assert.That(retrievedPaths[1], Is.EqualTo(@"C:\Photos\Recent2"));
        Assert.That(retrievedPaths[2], Is.EqualTo(@"C:\Photos\Recent3"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_EmptyList_ClearsTable()
    {
        _sqlitePersistenceContext!.RecentPaths.Replace([@"C:\Photos\A"]);

        _sqlitePersistenceContext!.RecentPaths.Replace([]);

        IReadOnlyList<string> retrievedPaths = _sqlitePersistenceContext!.RecentPaths.GetAll();

        Assert.That(retrievedPaths, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_CalledTwice_SecondReplacesFirst()
    {
        _sqlitePersistenceContext!.RecentPaths.Replace([@"C:\Old1", @"C:\Old2"]);

        _sqlitePersistenceContext!.RecentPaths.Replace([@"C:\New1", @"C:\New2", @"C:\New3"]);

        IReadOnlyList<string> retrievedPaths = _sqlitePersistenceContext!.RecentPaths.GetAll();

        Assert.That(retrievedPaths, Has.Count.EqualTo(3));
        Assert.That(retrievedPaths[0], Is.EqualTo(@"C:\New1"));
        Assert.That(retrievedPaths[1], Is.EqualTo(@"C:\New2"));
        Assert.That(retrievedPaths[2], Is.EqualTo(@"C:\New3"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_SinglePath_WorksCorrectly()
    {
        _sqlitePersistenceContext!.RecentPaths.Replace([@"C:\Photos\Only"]);

        IReadOnlyList<string> retrievedPaths = _sqlitePersistenceContext!.RecentPaths.GetAll();

        Assert.That(retrievedPaths, Has.Count.EqualTo(1));
        Assert.That(retrievedPaths[0], Is.EqualTo(@"C:\Photos\Only"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
