using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class ThumbnailPersistenceTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private Folder? _testFolder;

    private byte[]? _thumbnailData1;
    private byte[]? _thumbnailData2;
    private byte[]? _thumbnailData3;

    private SqlitePersistenceContext? _sqlitePersistenceContext;
    private TestLogger<SqlitePersistenceContext> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);

        _thumbnailData1 = File.ReadAllBytes(Path.Combine(_dataDirectory, FileNames.IMAGE_1_JPG));
        _thumbnailData2 = File.ReadAllBytes(Path.Combine(_dataDirectory, FileNames.IMAGE_11_90_DEG_HEIC));
        _thumbnailData3 = File.ReadAllBytes(Path.Combine(_dataDirectory, FileNames.IMAGE_9_PNG));
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        SqliteConnectionFactory factory = new();
        SqliteBackupService backupService = new(factory);
        _sqlitePersistenceContext = new(factory, backupService, _testLogger);
        _sqlitePersistenceContext.Initialize(_databaseDirectory!);

        _testFolder = _sqlitePersistenceContext.Folders.Insert(_dataDirectory!);
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
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_9_PNG, _thumbnailData3!);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(2));
        Assert.That(thumbnails[FileNames.IMAGE_1_JPG], Is.EqualTo(_thumbnailData1));
        Assert.That(thumbnails[FileNames.IMAGE_9_PNG], Is.EqualTo(_thumbnailData3));

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
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

        Dictionary<string, byte[]> newThumbnails = new()
        {
            [FileNames.IMAGE_11_90_DEG_HEIC] = _thumbnailData2!,
            [FileNames.IMAGE_9_PNG] = _thumbnailData3!
        };

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(_testFolder!.Id, newThumbnails);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(2));
        Assert.That(thumbnails.ContainsKey(FileNames.IMAGE_1_JPG), Is.False);
        Assert.That(thumbnails[FileNames.IMAGE_11_90_DEG_HEIC], Is.EqualTo(_thumbnailData2));
        Assert.That(thumbnails[FileNames.IMAGE_9_PNG], Is.EqualTo(_thumbnailData3));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ReplaceForFolder_EmptyDictionary_ClearsFolder()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(_testFolder!.Id, new Dictionary<string, byte[]>());

        Dictionary<string, byte[]> retrieved = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(retrieved, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void ReplaceForFolder_OnlyAffectsTargetFolder()
    {
        string duplicatesFolderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(duplicatesFolderPath);

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);
        _sqlitePersistenceContext!.Thumbnails.Upsert(folder.Id, FileNames.IMAGE_11_90_DEG_HEIC, _thumbnailData2!);

        _sqlitePersistenceContext!.Thumbnails.ReplaceForFolder(
            _testFolder!.Id,
            new Dictionary<string, byte[]>
            {
                [FileNames.IMAGE_9_PNG] = _thumbnailData3!
            });

        Dictionary<string, byte[]> testThumbs = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);
        Dictionary<string, byte[]> otherThumbs = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(folder.Id);

        Assert.That(testThumbs, Has.Count.EqualTo(1));
        Assert.That(testThumbs.ContainsKey(FileNames.IMAGE_9_PNG), Is.True);
        Assert.That(otherThumbs, Has.Count.EqualTo(1));
        Assert.That(otherThumbs.ContainsKey(FileNames.IMAGE_11_90_DEG_HEIC), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_NewThumbnail_Inserts()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails[FileNames.IMAGE_1_JPG], Is.EqualTo(_thumbnailData1));
        Assert.That(thumbnails[FileNames.IMAGE_1_JPG], Has.Length.EqualTo(_thumbnailData1!.Length));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_ExistingThumbnail_UpdatesData()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData2!);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails[FileNames.IMAGE_1_JPG], Is.EqualTo(_thumbnailData2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_ExistingThumbnail_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

        bool isDeleted = _sqlitePersistenceContext!.Thumbnails.Delete(_testFolder!.Id, FileNames.IMAGE_1_JPG);

        Assert.That(isDeleted, Is.True);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_NonExistentThumbnail_ReturnsFalse()
    {
        bool isDeleted = _sqlitePersistenceContext!.Thumbnails.Delete(_testFolder!.Id, FileNames.NON_EXISTENT_FILE_JPG);

        Assert.That(isDeleted, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_WithThumbnails_ReturnsCount()
    {
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_11_90_DEG_HEIC, _thumbnailData2!);
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_9_PNG, _thumbnailData3!);

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
        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);

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

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, largeData);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails[FileNames.IMAGE_1_JPG], Is.EqualTo(largeData));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_OnlyReturnsTargetFolderThumbnails()
    {
        string duplicatesFolderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        Folder folder = _sqlitePersistenceContext!.Folders.Insert(duplicatesFolderPath);

        _sqlitePersistenceContext!.Thumbnails.Upsert(_testFolder!.Id, FileNames.IMAGE_1_JPG, _thumbnailData1!);
        _sqlitePersistenceContext!.Thumbnails.Upsert(folder.Id, FileNames.IMAGE_11_90_DEG_HEIC, _thumbnailData2!);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext!.Thumbnails.GetByFolderId(_testFolder!.Id);

        Assert.That(thumbnails, Has.Count.EqualTo(1));
        Assert.That(thumbnails.ContainsKey(FileNames.IMAGE_1_JPG), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }
}
