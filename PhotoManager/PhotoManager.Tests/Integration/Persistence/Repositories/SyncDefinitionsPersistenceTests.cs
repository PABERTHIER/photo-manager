using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class SyncDefinitionsPersistenceTests
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
    public void GetAll_EmptyTable_ReturnsEmptyList()
    {
        IReadOnlyList<SyncAssetsDirectoriesDefinition> definitions =
            _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(definitions, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_WithDefinitions_StoresInOrder()
    {
        List<SyncAssetsDirectoriesDefinition> definitions =
        [
            new()
            {
                SourceDirectory = @"C:\Source1",
                DestinationDirectory = @"D:\Dest1",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = false
            },
            new()
            {
                SourceDirectory = @"C:\Source2",
                DestinationDirectory = @"D:\Dest2",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = true
            }
        ];

        _sqlitePersistenceContext!.SyncDefinitions.Replace(definitions);

        IReadOnlyList<SyncAssetsDirectoriesDefinition> retrieved = _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(retrieved, Has.Count.EqualTo(2));

        Assert.That(retrieved[0].SourceDirectory, Is.EqualTo(@"C:\Source1"));
        Assert.That(retrieved[0].DestinationDirectory, Is.EqualTo(@"D:\Dest1"));
        Assert.That(retrieved[0].IncludeSubFolders, Is.True);
        Assert.That(retrieved[0].DeleteAssetsNotInSource, Is.False);

        Assert.That(retrieved[1].SourceDirectory, Is.EqualTo(@"C:\Source2"));
        Assert.That(retrieved[1].DestinationDirectory, Is.EqualTo(@"D:\Dest2"));
        Assert.That(retrieved[1].IncludeSubFolders, Is.False);
        Assert.That(retrieved[1].DeleteAssetsNotInSource, Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_EmptyList_ClearsTable()
    {
        _sqlitePersistenceContext!.SyncDefinitions.Replace(
        [
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\S",
                DestinationDirectory = @"D:\D",
            }
        ]);

        _sqlitePersistenceContext!.SyncDefinitions.Replace([]);

        IReadOnlyList<SyncAssetsDirectoriesDefinition> retrieved = _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(retrieved, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_CalledTwice_SecondReplacesFirst()
    {
        _sqlitePersistenceContext!.SyncDefinitions.Replace(
        [
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\Old",
                DestinationDirectory = @"D:\Old",
            }
        ]);

        _sqlitePersistenceContext!.SyncDefinitions.Replace(
        [
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\New1",
                DestinationDirectory = @"D:\New1",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = true
            },
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\New2",
                DestinationDirectory = @"D:\New2",
            }
        ]);

        IReadOnlyList<SyncAssetsDirectoriesDefinition> retrieved = _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(retrieved, Has.Count.EqualTo(2));
        Assert.That(retrieved[0].SourceDirectory, Is.EqualTo(@"C:\New1"));
        Assert.That(retrieved[1].SourceDirectory, Is.EqualTo(@"C:\New2"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Replace_BooleanFlags_RoundTripCorrectly()
    {
        _sqlitePersistenceContext!.SyncDefinitions.Replace(
        [
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\Src",
                DestinationDirectory = @"D:\Dst",
                IncludeSubFolders = true,
                DeleteAssetsNotInSource = true
            }
        ]);

        IReadOnlyList<SyncAssetsDirectoriesDefinition> retrieved = _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(retrieved[0].IncludeSubFolders, Is.True);
        Assert.That(retrieved[0].DeleteAssetsNotInSource, Is.True);

        _sqlitePersistenceContext!.SyncDefinitions.Replace(
        [
            new SyncAssetsDirectoriesDefinition
            {
                SourceDirectory = @"C:\Src",
                DestinationDirectory = @"D:\Dst",
                IncludeSubFolders = false,
                DeleteAssetsNotInSource = false
            }
        ]);

        retrieved = _sqlitePersistenceContext!.SyncDefinitions.GetAll();

        Assert.That(retrieved[0].IncludeSubFolders, Is.False);
        Assert.That(retrieved[0].DeleteAssetsNotInSource, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
