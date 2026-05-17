using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class FolderPersistenceTests
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
    public void Insert_ValidPath_ReturnsFolderWithGeneratedId()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Vacation");

        Assert.That(folder.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(folder.Path, Is.EqualTo(@"C:\Photos\Vacation"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Insert_MultipleFolders_EachGetsUniqueId()
    {
        Folder folder1 = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\A");
        Folder folder2 = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\B");

        Assert.That(folder1.Id, Is.Not.EqualTo(folder2.Id));
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_ExistingFolder_ReturnsTrueAndRemoves()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos");

        bool isDeleted = _sqlitePersistenceContext!.Folders.Delete(folder.Id);

        Assert.That(isDeleted, Is.True);
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_NonExistentFolder_ReturnsFalse()
    {
        bool isDeleted = _sqlitePersistenceContext!.Folders.Delete(Guid.NewGuid());

        Assert.That(isDeleted, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetById_ExistingFolder_ReturnsFolder()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Family");

        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetById(folder.Id);

        Assert.That(retrievedFolder, Is.Not.Null);
        Assert.That(retrievedFolder!.Id, Is.EqualTo(folder.Id));
        Assert.That(retrievedFolder.Path, Is.EqualTo(@"C:\Photos\Family"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetById_NonExistentFolder_ReturnsNull()
    {
        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetById(Guid.NewGuid());

        Assert.That(retrievedFolder, Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByPath_ExistingFolder_ReturnsFolder()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Work");

        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetByPath(@"C:\Photos\Work");

        Assert.That(retrievedFolder, Is.Not.Null);
        Assert.That(retrievedFolder!.Id, Is.EqualTo(folder.Id));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByPath_NonExistentPath_ReturnsNull()
    {
        Folder? retrieved = _sqlitePersistenceContext!.Folders.GetByPath(@"C:\DoesNotExist");

        Assert.That(retrieved, Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByPath_DuplicatePaths_ReturnsOneResult()
    {
        Folder folder1 = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Dup");
        Folder folder2 = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Dup");

        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetByPath(@"C:\Photos\Dup");

        Assert.That(retrievedFolder, Is.Not.Null);
        Assert.That(retrievedFolder!.Id, Is.EqualTo(folder1.Id).Or.EqualTo(folder2.Id));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_EmptyTable_ReturnsEmptyList()
    {
        IReadOnlyList<Folder> folders = _sqlitePersistenceContext!.Folders.GetAll();

        Assert.That(folders, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_WithFolders_ReturnsAllInInsertionOrder()
    {
        Folder folder1 = _sqlitePersistenceContext!.Folders.Insert(@"C:\A");
        Folder folder2 = _sqlitePersistenceContext!.Folders.Insert(@"C:\B");
        Folder folder3 = _sqlitePersistenceContext!.Folders.Insert(@"C:\C");

        IReadOnlyList<Folder> folders = _sqlitePersistenceContext!.Folders.GetAll();

        Assert.That(folders, Has.Count.EqualTo(3));
        Assert.That(folders[0].Id, Is.EqualTo(folder1.Id));
        Assert.That(folders[1].Id, Is.EqualTo(folder2.Id));
        Assert.That(folders[2].Id, Is.EqualTo(folder3.Id));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Count_EmptyTable_ReturnsZero()
    {
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Count_WithFolders_ReturnsCorrectCount()
    {
        _sqlitePersistenceContext!.Folders.Insert(@"C:\A");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\B");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\C");

        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(3));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_RemovesOnlyTargetFolder()
    {
        Folder folder1 = _sqlitePersistenceContext!.Folders.Insert(@"C:\A");
        Folder folder2 = _sqlitePersistenceContext!.Folders.Insert(@"C:\B");

        _sqlitePersistenceContext!.Folders.Delete(folder1.Id);

        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(1));
        Assert.That(_sqlitePersistenceContext!.Folders.GetById(folder2.Id), Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.Folders.GetById(folder1.Id), Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
