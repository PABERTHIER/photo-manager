using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Unit.Persistence.Sqlite;

[TestFixture]
public class SqlitePersistenceContextTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ISqliteConnectionFactory? _factory;
    private ISqliteBackupService? _backupService;
    private SqlitePersistenceContext? _sqlitePersistenceContext;
    private TestLogger<SqlitePersistenceContext> _testLogger = new();

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
        _factory = Substitute.For<ISqliteConnectionFactory>();
        _backupService = Substitute.For<ISqliteBackupService>();
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
    public void Initialize_FactoryOpenThrows_LogsErrorAndRethrows()
    {
        InvalidOperationException thrownException = new("Connection failed");

        _factory!.DatabasePath.Returns(Path.Combine(_databaseDirectory!, "photomanager.db"));
        _factory!.Open().Throws(thrownException);

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _sqlitePersistenceContext!.Initialize(_databaseDirectory!));

        Assert.That(exception, Is.SameAs(thrownException));

        _factory.Received(1).Initialize(Arg.Any<string>());
        _factory.Received(1).Open();

        _testLogger.AssertLogExceptions([thrownException], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_BackupServiceThrows_LogsErrorAndRethrows()
    {
        string databasePath = Path.Combine(_databaseDirectory!, "photomanager.db");
        _factory!.DatabasePath.Returns(databasePath);

        IOException thrownException = new("Disk full");
        _backupService!.WriteBackup(Arg.Any<string>()).Throws(thrownException);

        IOException? exception =
            Assert.Throws<IOException>(() => _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 5, 1)));

        Assert.That(exception, Is.SameAs(thrownException));

        _backupService.Received(1).WriteBackup(Arg.Any<string>());

        _testLogger.AssertLogExceptions([thrownException], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_BackupServiceGetFilesThrows_LogsErrorAndRethrows()
    {
        string databasePath = Path.Combine(_databaseDirectory!, "photomanager.db");
        _factory!.DatabasePath.Returns(databasePath);

        UnauthorizedAccessException thrownException = new("Access denied");
        _backupService!.GetBackupFilesPaths(Arg.Any<string>()).Throws(thrownException);

        UnauthorizedAccessException? exception =
            Assert.Throws<UnauthorizedAccessException>(() => _sqlitePersistenceContext!.DeleteOldBackups(2));

        Assert.That(exception, Is.SameAs(thrownException));

        _backupService.Received(1).GetBackupFilesPaths(Arg.Any<string>());

        _testLogger.AssertLogExceptions([thrownException], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_BackupServiceDeleteThrows_LogsErrorAndRethrows()
    {
        string databasePath = Path.Combine(_databaseDirectory!, "photomanager.db");
        _factory!.DatabasePath.Returns(databasePath);

        _backupService!.GetBackupFilesPaths(Arg.Any<string>())
            .Returns(["20240101.zip", "20240102.zip", "20240103.zip"]);

        IOException thrownException = new("File locked");
        _backupService!.When(x => x.DeleteBackupFile(Arg.Any<string>()))
            .Do(_ => throw thrownException);

        IOException? exception = Assert.Throws<IOException>(() => _sqlitePersistenceContext!.DeleteOldBackups(1));

        Assert.That(exception, Is.SameAs(thrownException));

        _testLogger.AssertLogExceptions([thrownException], typeof(SqlitePersistenceContext));
    }
}
