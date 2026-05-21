using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class ConfigurationPersistenceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

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
    public void GetValue_KeyNotSet_ReturnsNull()
    {
        string? value = _sqlitePersistenceContext!.Configuration.GetValue("UnknownKey");

        Assert.That(value, Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValue_NewKey_StoresValue()
    {
        _sqlitePersistenceContext!.Configuration.SetValue("AssetsDirectory", @"C:\Photos");

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue("AssetsDirectory");

        Assert.That(retrieved, Is.EqualTo(@"C:\Photos"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValue_ExistingKey_OverwritesValue()
    {
        _sqlitePersistenceContext!.Configuration.SetValue("AssetsDirectory", @"C:\Old");
        _sqlitePersistenceContext!.Configuration.SetValue("AssetsDirectory", @"C:\New");

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue("AssetsDirectory");

        Assert.That(retrieved, Is.EqualTo(@"C:\New"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValue_MultipleKeys_EachStoredIndependently()
    {
        _sqlitePersistenceContext!.Configuration.SetValue("Key1", "ValueA");
        _sqlitePersistenceContext!.Configuration.SetValue("Key2", "ValueB");

        string? value1 = _sqlitePersistenceContext!.Configuration.GetValue("Key1");
        string? value2 = _sqlitePersistenceContext!.Configuration.GetValue("Key2");

        Assert.That(value1, Is.EqualTo("ValueA"));
        Assert.That(value2, Is.EqualTo("ValueB"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetValue_AfterReinit_ValuePersistsAcrossConnections()
    {
        _sqlitePersistenceContext!.Configuration.SetValue("AssetsDirectory", @"C:\Photos\Persistent");
        _sqlitePersistenceContext!.Dispose();

        SqliteConnectionFactory factory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(factory);

        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue("AssetsDirectory");

        Assert.That(retrieved, Is.EqualTo(@"C:\Photos\Persistent"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
