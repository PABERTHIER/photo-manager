using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class ThumbnailPersistenceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private Folder? _testFolder;

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
        SqliteConnectionFactory factory = new();
        SqliteBackupService backupService = new(factory);
        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);

        _testFolder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos\Test");
    }

    [TearDown]
    public void TearDown()
    {
        _sqlitePersistenceContext!.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger.LoggingAssertTearDown();
    }

    [Test]
    public void GetByFolderId_WithThumbnails_ReturnsDictionary()
    {
        byte[] data1 = [0xFF, 0xD8, 0xFF, 0xE0, 0x01, 0x02];
        byte[] data2 = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A];

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "img1.jpg", data1);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "img2.png", data2);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(2));
        Assert.That(thumbnails["img1.jpg"], Is.EqualTo(data1));
        Assert.That(thumbnails["img2.png"], Is.EqualTo(data2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_NoThumbnails_ReturnsEmptyDictionary()
    {
        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(Guid.NewGuid());

        Assert.That(thumbnails, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ReplaceForFolder_WithThumbnails_ReplacesAll()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "old.jpg", [1, 2, 3]);

        Dictionary<string, byte[]> newThumbnails = new()
        {
            ["new1.jpg"] = [0xF0, 0xF1, 0xF2],
            ["new2.jpg"] = [0xA0, 0xA1, 0xA2]
        };

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(_testFolder!.Id, newThumbnails);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(2));
        Assert.That(thumbnails.ContainsKey("old.jpg"), Is.False);
        Assert.That(thumbnails["new1.jpg"], Is.EqualTo(new byte[] { 0xF0, 0xF1, 0xF2 }));
        Assert.That(thumbnails["new2.jpg"], Is.EqualTo(new byte[] { 0xA0, 0xA1, 0xA2 }));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ReplaceForFolder_EmptyDictionary_ClearsFolder()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "img.jpg", [1, 2]);

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(_testFolder!.Id, new Dictionary<string, byte[]>());

        Dictionary<string, byte[]> retrieved = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(retrieved, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ReplaceForFolder_OnlyAffectsTargetFolder()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Other");

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "a.jpg", [1]);
        _sqlitePersistenceContext!.Thumbnails.Upsert(folder.Id, "b.jpg", [2]);

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(
            _testFolder!.Id,
            new Dictionary<string, byte[]>
            {
                ["c.jpg"] = [3]
            });

        Dictionary<string, byte[]> testThumbs = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);
        Dictionary<string, byte[]> otherThumbs = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(folder.Id);

        Assert.That(testThumbs, Has.Count.EqualTo(1));
        Assert.That(testThumbs.ContainsKey("c.jpg"), Is.True);
        Assert.That(otherThumbs, Has.Count.EqualTo(1));
        Assert.That(otherThumbs.ContainsKey("b.jpg"), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_NewThumbnail_Inserts()
    {
        byte[] data = [0xFF, 0xD8, 0xFF, 0xE0];

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "photo.jpg", data);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails["photo.jpg"], Is.EqualTo(data));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_ExistingThumbnail_UpdatesData()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "photo.jpg", [1, 2, 3]);

        byte[] newData = [10, 20, 30, 40, 50];
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "photo.jpg", newData);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails["photo.jpg"], Is.EqualTo(newData));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_ExistingThumbnail_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "img.jpg", [1, 2]);

        bool isDeleted = _sqlitePersistenceContext!.Thumbnails.Delete(_testFolder!.Id, "img.jpg");

        Assert.That(isDeleted, Is.True);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_NonExistentThumbnail_ReturnsFalse()
    {
        bool isDeleted = _sqlitePersistenceContext!.Thumbnails.Delete(_testFolder!.Id, "nonexistent.jpg");

        Assert.That(isDeleted, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_WithThumbnails_ReturnsCount()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "a.jpg", [1]);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "b.jpg", [2]);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "c.jpg", [3]);

        int isDeleted = _sqlitePersistenceContext!.Thumbnails.DeleteByFolderId(_testFolder!.Id);

        Assert.That(isDeleted, Is.EqualTo(3));

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_NoThumbnails_ReturnsZero()
    {
        int isDeleted = _sqlitePersistenceContext!.Thumbnails.DeleteByFolderId(Guid.NewGuid());

        Assert.That(isDeleted, Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ExistsForFolder_WithThumbnails_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "img.jpg", [1, 2]);

        bool exists = _sqlitePersistenceContext!.Thumbnails.ExistsForFolder(_testFolder!.Id);

        Assert.That(exists, Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ExistsForFolder_NoThumbnails_ReturnsFalse()
    {
        bool exists = _sqlitePersistenceContext!.Thumbnails.ExistsForFolder(Guid.NewGuid());

        Assert.That(exists, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void LargeBlobData_RoundTripsCorrectly()
    {
        byte[] largeData = new byte[100_000];
        Random random = new(42);
        random.NextBytes(largeData);

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "large.jpg", largeData);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails["large.jpg"], Is.EqualTo(largeData));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_OnlyReturnsTargetFolderThumbnails()
    {
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Other");

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, "a.jpg", [1]);
        _sqlitePersistenceContext!.Thumbnails.Upsert(folder.Id, "b.jpg", [2]);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails.ContainsKey("a.jpg"), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
