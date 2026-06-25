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
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\Photos");

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY);

        Assert.That(retrieved, Is.EqualTo(@"C:\Photos"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValue_ExistingKey_OverwritesValue()
    {
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\Old");
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\New");

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY);

        Assert.That(retrieved, Is.EqualTo(@"C:\New"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValue_MultipleKeys_EachStoredIndependently()
    {
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\NewPhotos");
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.THEME_MODE, "Dark");

        string? value1 = _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY);
        string? value2 = _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.THEME_MODE);

        Assert.That(value1, Is.EqualTo(@"C:\NewPhotos"));
        Assert.That(value2, Is.EqualTo("Dark"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetValue_AfterReinit_ValuePersistsAcrossConnections()
    {
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY,
            @"C:\Photos\Persistent");
        _sqlitePersistenceContext!.Dispose();

        SqliteConnectionFactory factory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(factory);

        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);

        string? retrieved = _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY);

        Assert.That(retrieved, Is.EqualTo(@"C:\Photos\Persistent"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_NoValues_ReturnsEmpty()
    {
        IReadOnlyDictionary<string, string> values = _sqlitePersistenceContext!.Configuration.GetAll();

        Assert.That(values, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_ValuesStored_ReturnsEveryKeyValuePair()
    {
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\Photos");
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.THEME_MODE, "Dark");

        IReadOnlyDictionary<string, string> values = _sqlitePersistenceContext!.Configuration.GetAll();

        Assert.That(values, Has.Count.EqualTo(2));
        Assert.That(values[UserConfigurationKeys.ASSETS_DIRECTORY], Is.EqualTo(@"C:\Photos"));
        Assert.That(values[UserConfigurationKeys.THEME_MODE], Is.EqualTo("Dark"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValues_NewAndExistingKeys_InsertsAndOverwritesAtomically()
    {
        _sqlitePersistenceContext!.Configuration.SetValue(UserConfigurationKeys.ASSETS_DIRECTORY, @"C:\OldPhotos");

        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            [UserConfigurationKeys.ASSETS_DIRECTORY] = @"C:\NewPhotos",
            [UserConfigurationKeys.THEME_MODE] = "Dark"
        };
        _sqlitePersistenceContext!.Configuration.SetValues(values);

        IReadOnlyDictionary<string, string> stored = _sqlitePersistenceContext!.Configuration.GetAll();

        Assert.That(stored, Has.Count.EqualTo(2));
        Assert.That(stored[UserConfigurationKeys.ASSETS_DIRECTORY], Is.EqualTo(@"C:\NewPhotos"));
        Assert.That(stored[UserConfigurationKeys.THEME_MODE], Is.EqualTo("Dark"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValues_EmptyDictionary_StoresNothing()
    {
        _sqlitePersistenceContext!.Configuration.SetValues(new Dictionary<string, string>());

        Assert.That(_sqlitePersistenceContext!.Configuration.GetAll(), Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void SetValues_AfterReinit_ValuesPersistAcrossConnections()
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            [UserConfigurationKeys.ASSETS_DIRECTORY] = @"C:\Photos\Persistent",
            [UserConfigurationKeys.THEME_MODE] = "Dark"
        };
        _sqlitePersistenceContext!.Configuration.SetValues(values);
        _sqlitePersistenceContext!.Dispose();

        SqliteConnectionFactory factory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(factory);

        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);

        IReadOnlyDictionary<string, string> stored = _sqlitePersistenceContext!.Configuration.GetAll();

        Assert.That(stored, Has.Count.EqualTo(2));
        Assert.That(stored[UserConfigurationKeys.ASSETS_DIRECTORY], Is.EqualTo(@"C:\Photos\Persistent"));
        Assert.That(stored[UserConfigurationKeys.THEME_MODE], Is.EqualTo("Dark"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
