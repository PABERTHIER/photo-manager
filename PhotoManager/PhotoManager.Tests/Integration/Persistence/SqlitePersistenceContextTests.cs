using Microsoft.Data.Sqlite;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class SqlitePersistenceContextTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private string? _backupsDirectory;

    private SqliteConnectionFactory? _factory;
    private SqliteBackupService? _backupService;
    private SqlitePersistenceContext? _sqlitePersistenceContext;
    private TestLogger<SqlitePersistenceContext> _testLogger = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
        _backupsDirectory = Path.Combine(_databaseDirectory, Constants.DATABASE_BACKUP_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _factory = new(new TestLogger<SqliteConnectionFactory>());
        _backupService = new(_factory);
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
    public void Initialize_ValidDirectory_CreatesDatabaseFile()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(File.Exists(_sqlitePersistenceContext!.DatabaseFilePath), Is.True);
        Assert.That(_sqlitePersistenceContext!.DatabaseFilePath,
            Is.EqualTo(Path.Combine(_databaseDirectory!, SqlitePersistenceContext.DATABASE_FILE_NAME)));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_ValidDirectory_CreatesBackupDirectory()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(Directory.Exists(_backupsDirectory), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_ValidDirectory_ExposesRepositories()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.Folders, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.Assets, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.Thumbnails, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.RecentPaths, Is.Not.Null);
        Assert.That(_sqlitePersistenceContext!.SyncDefinitions, Is.Not.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    public void Initialize_EmptyString_ThrowsArgumentException(string databaseDirectory)
    {
        const string expectedMessage = "databaseDirectory must not be empty.";

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _sqlitePersistenceContext!.Initialize(databaseDirectory));

        Assert.That(exception?.Message, Is.EqualTo($"{expectedMessage} (Parameter 'databaseDirectory')"));
        Assert.That(exception.ParamName, Is.EqualTo("databaseDirectory"));

        _testLogger.AssertLogExceptions(
            [new ArgumentException(expectedMessage, nameof(databaseDirectory))],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_CalledTwice_IsIdempotent()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos");
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(1));

        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(1));

        Folder? retrievedFolder = _sqlitePersistenceContext!.Folders.GetById(folder.Id);
        Assert.That(retrievedFolder, Is.Not.Null);
        Assert.That(retrievedFolder!.Path, Is.EqualTo(@"C:\Photos"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DatabaseFilePath_BeforeInitialize_ReturnsEmpty()
    {
        Assert.That(_sqlitePersistenceContext!.DatabaseFilePath, Is.EqualTo(string.Empty));
    }

    [Test]
    public void WriteBackup_AfterInitialize_CreatesZipFile()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 3, 15);

        bool result = _sqlitePersistenceContext!.WriteBackup(backupDate);

        Assert.That(result, Is.True);

        string expectedFilePath = Path.Combine(_backupsDirectory!, "20240315.zip");
        Assert.That(File.Exists(expectedFilePath), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _sqlitePersistenceContext!.WriteBackup(DateTime.Now));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_SameDate_OverwritesPreviousBackup()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 8, 20);

        _sqlitePersistenceContext!.WriteBackup(backupDate);

        string backupPath = Path.Combine(_backupsDirectory!, "20240820.zip");

        long firstBackupSize = new FileInfo(backupPath).Length;

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder1");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder2");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\NewFolder3");
        _sqlitePersistenceContext!.WriteBackup(backupDate);

        long secondBackupSize = new FileInfo(backupPath).Length;

        Assert.That(File.Exists(backupPath), Is.True);
        Assert.That(secondBackupSize, Is.GreaterThan(firstBackupSize));

        string[] files = Directory.GetFiles(_backupsDirectory!, "*.zip");

        Assert.That(files, Has.Length.EqualTo(1));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_AfterWriteBackup_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        DateTime backupDate = new(2024, 5, 10);

        _sqlitePersistenceContext!.WriteBackup(backupDate);

        Assert.That(_sqlitePersistenceContext!.BackupExists(backupDate), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_NoBackup_ReturnsFalse()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.That(_sqlitePersistenceContext!.BackupExists(new DateTime(2024, 12, 25)), Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void BackupExists_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.BackupExists(DateTime.Now));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepsSpecifiedCount()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 3));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 1, 4));

        _sqlitePersistenceContext!.DeleteOldBackups(2);

        string[] files = Directory.GetFiles(_backupsDirectory!, "*.zip");

        Assert.That(files, Has.Length.EqualTo(2));
        Assert.That(files.Any(f => f.Contains("20240103")), Is.True);
        Assert.That(files.Any(f => f.Contains("20240104")), Is.True);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_NoBackups_NothingDeleted()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.DeleteOldBackups(5);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetWithThumbnail_ValidAsset_PersistsAssetAndThumbnail()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        byte[] thumbnailData = [1, 2, 3];

        _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, thumbnailData);

        Asset? persistedAsset = _sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName);
        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(persistedAsset, Is.Not.Null);
            Assert.That(persistedAsset!.FolderId, Is.EqualTo(folder.Id));
            Assert.That(persistedAsset.FileName, Is.EqualTo(asset.FileName));
            Assert.That(thumbnails, Contains.Key(asset.FileName));
            Assert.That(thumbnails[asset.FileName], Is.EqualTo(thumbnailData));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetWithThumbnail_ThumbnailInsertFails_RollsBackAsset()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        byte[] thumbnailData = [1, 2, 3];

        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = """
                                      CREATE TRIGGER FailThumbnailInsert
                                      BEFORE INSERT ON Thumbnails
                                      BEGIN
                                          SELECT RAISE(ABORT, 'thumbnail failure');
                                      END;
                                      """;
                command.ExecuteNonQuery();
            }
        }

        SqliteException? exception = Assert.Throws<SqliteException>(() =>
            _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, thumbnailData));

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.Message, Does.Contain("thumbnail failure"));
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName), Is.Null);
            Assert.That(thumbnails, Is.Empty);
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetsWithThumbnails_ValidAssets_PersistsAssetsAndThumbnails()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset1 = CreateAsset(folder);
        Asset asset2 = CreateAsset(folder, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG);
        Asset asset3 = CreateAsset(folder, FileNames.HOMER_GIF, Hashes.HOMER_GIF, true, true);
        byte[] thumbnailData1 = [1, 2, 3];
        byte[] thumbnailData2 = [4, 5, 6];
        byte[] thumbnailData3 = [7, 8, 9];

        _sqlitePersistenceContext.UpsertAssetsWithThumbnails(
            [new(asset1, thumbnailData1), new(asset2, thumbnailData2), new(asset3, thumbnailData3)]);

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset1.FileName), Is.Not.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset2.FileName), Is.Not.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset3.FileName), Is.Not.Null);
            Assert.That(thumbnails[asset1.FileName], Is.EqualTo(thumbnailData1));
            Assert.That(thumbnails[asset2.FileName], Is.EqualTo(thumbnailData2));
            Assert.That(thumbnails[asset3.FileName], Is.EqualTo(thumbnailData3));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetsWithThumbnails_EmptyList_DoesNothing()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.DoesNotThrow(() => _sqlitePersistenceContext.UpsertAssetsWithThumbnails([]));

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetsWithThumbnails_ThumbnailInsertFails_RollsBackAssets()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset1 = CreateAsset(folder);
        Asset asset2 = CreateAsset(folder, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG);

        CreateAbortTrigger("FailBatchThumbnailInsert", "Thumbnails", "INSERT", "thumbnail batch failure");

        SqliteException? exception = Assert.Throws<SqliteException>(() =>
            _sqlitePersistenceContext.UpsertAssetsWithThumbnails([new(asset1, [1]), new(asset2, [2])]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.Message, Does.Contain("thumbnail batch failure"));
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset1.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset2.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id), Is.Empty);
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetsWithThumbnails_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.UpsertAssetsWithThumbnails([]));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertAssetWithThumbnail_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        Folder folder = new() { Id = Guid.NewGuid(), Path = @"C:\Photos" };
        Asset asset = CreateAsset(folder);

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.UpsertAssetWithThumbnail(asset, [1, 2, 3]));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetWithThumbnail_ExistingAsset_DeletesAssetAndThumbnail()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, [1, 2, 3]);

        _sqlitePersistenceContext.DeleteAssetWithThumbnail(folder.Id, asset.FileName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id), Is.Empty);
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetsWithThumbnails_ExistingAssets_DeletesAssetsAndThumbnails()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset1 = CreateAsset(folder);
        Asset asset2 = CreateAsset(folder, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG);
        _sqlitePersistenceContext.UpsertAssetsWithThumbnails([new(asset1, [1]), new(asset2, [2])]);

        _sqlitePersistenceContext.DeleteAssetsWithThumbnails(folder.Id, [asset1.FileName, asset2.FileName]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset1.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset2.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id), Is.Empty);
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetsWithThumbnails_EmptyList_DoesNothing()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.DoesNotThrow(() => _sqlitePersistenceContext.DeleteAssetsWithThumbnails(Guid.NewGuid(), []));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetsWithThumbnails_AssetDeleteFails_RollsBackAssetsAndThumbnails()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset1 = CreateAsset(folder);
        Asset asset2 = CreateAsset(folder, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG);
        byte[] thumbnailData1 = [1];
        byte[] thumbnailData2 = [2];
        _sqlitePersistenceContext.UpsertAssetsWithThumbnails(
            [new(asset1, thumbnailData1), new(asset2, thumbnailData2)]);

        CreateAbortTrigger("FailBatchAssetDelete", "Assets", "DELETE", "asset batch delete failure");

        SqliteException? exception = Assert.Throws<SqliteException>(() =>
            _sqlitePersistenceContext.DeleteAssetsWithThumbnails(folder.Id, [asset1.FileName, asset2.FileName]));
        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.Message, Does.Contain("asset batch delete failure"));
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset1.FileName), Is.Not.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset2.FileName), Is.Not.Null);
            Assert.That(thumbnails[asset1.FileName], Is.EqualTo(thumbnailData1));
            Assert.That(thumbnails[asset2.FileName], Is.EqualTo(thumbnailData2));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetWithThumbnail_AssetDeleteFails_RollsBackThumbnail()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        byte[] thumbnailData = [1, 2, 3];
        _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, thumbnailData);

        CreateAbortTrigger("FailAssetDelete", "Assets", "DELETE", "asset delete failure");

        SqliteException? exception = Assert.Throws<SqliteException>(() =>
            _sqlitePersistenceContext.DeleteAssetWithThumbnail(folder.Id, asset.FileName));

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.Message, Does.Contain("asset delete failure"));
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName), Is.Not.Null);
            Assert.That(thumbnails, Contains.Key(asset.FileName));
            Assert.That(thumbnails[asset.FileName], Is.EqualTo(thumbnailData));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteFolderWithAssetsAndThumbnails_ExistingFolder_DeletesFolderAssetsAndThumbnails()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, [1, 2, 3]);

        _sqlitePersistenceContext.DeleteFolderWithAssetsAndThumbnails(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_sqlitePersistenceContext.Folders.GetById(folder.Id), Is.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName), Is.Null);
            Assert.That(_sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id), Is.Empty);
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteFolderWithAssetsAndThumbnails_FolderDeleteFails_RollsBackAssetsAndThumbnails()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Folder folder = _sqlitePersistenceContext.Folders.Insert(@"C:\Photos");
        Asset asset = CreateAsset(folder);
        byte[] thumbnailData = [1, 2, 3];
        _sqlitePersistenceContext.UpsertAssetWithThumbnail(asset, thumbnailData);

        CreateAbortTrigger("FailFolderDelete", "Folders", "DELETE", "folder delete failure");

        SqliteException? exception = Assert.Throws<SqliteException>(() =>
            _sqlitePersistenceContext.DeleteFolderWithAssetsAndThumbnails(folder.Id));

        Dictionary<string, byte[]> thumbnails = _sqlitePersistenceContext.Thumbnails.GetByFolderId(folder.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception?.Message, Does.Contain("folder delete failure"));
            Assert.That(_sqlitePersistenceContext.Folders.GetById(folder.Id), Is.Not.Null);
            Assert.That(_sqlitePersistenceContext.Assets.Get(folder.Id, asset.FileName), Is.Not.Null);
            Assert.That(thumbnails, Contains.Key(asset.FileName));
            Assert.That(thumbnails[asset.FileName], Is.EqualTo(thumbnailData));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetWithThumbnail_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.DeleteAssetWithThumbnail(Guid.NewGuid(), "image.jpg"));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteAssetsWithThumbnails_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.DeleteAssetsWithThumbnails(Guid.NewGuid(), []));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteFolderWithAssetsAndThumbnails_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.DeleteFolderWithAssetsAndThumbnails(Guid.NewGuid()));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
            _sqlitePersistenceContext!.DeleteOldBackups(5));

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions(
            [new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepMoreThanExist_DeletesNothing()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 7, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 7, 2));

        _sqlitePersistenceContext!.DeleteOldBackups(10);

        string[] files = Directory.GetFiles(_backupsDirectory!, "*.zip");

        Assert.That(files, Has.Length.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepZero_DeletesAll()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 2, 3));

        _sqlitePersistenceContext!.DeleteOldBackups(0);

        string[] files = Directory.GetFiles(_backupsDirectory!, "*.zip");

        Assert.That(files, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void WriteBackup_AfterInsertingData_BackupContainsData()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Family");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Travel");

        DateTime backupDate = new(2024, 6, 15);
        _sqlitePersistenceContext!.WriteBackup(backupDate);

        string backupPath = Path.Combine(_backupsDirectory!, "20240615.zip");

        Assert.That(File.Exists(backupPath), Is.True);
        Assert.That(new FileInfo(backupPath).Length, Is.GreaterThan(0));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
    }

    [Test]
    public void Dispose_BeforeInitialize_DoesNotThrow()
    {
        Assert.DoesNotThrow(_sqlitePersistenceContext!.Dispose);
    }

    [Test]
    public void Initialize_ValidDirectory_SetsSchemaVersion()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA user_version;";
                long version = Convert.ToInt64(command.ExecuteScalar());

                Assert.That(version, Is.EqualTo(2));
            }
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Initialize_ExistingV1Schema_MigratesToV2AndCreatesConfigurationTable()
    {
        // Arrange: manually create a v1 database (no Configuration table)
        string databasePath = Path.Combine(_databaseDirectory!, SqlitePersistenceContext.DATABASE_FILE_NAME);
        Directory.CreateDirectory(_databaseDirectory!);

        using (SqliteConnection connection = new($"Data Source={databasePath}"))
        {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS Folders (Id TEXT PRIMARY KEY NOT NULL, Path TEXT NOT NULL);";
                command.ExecuteNonQuery();
            }

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA user_version = 1;";
                command.ExecuteNonQuery();
            }
        }

        // Act: Initialize detects v1 and migrates to v2
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        // Assert: schema version bumped to 2 and Configuration table created
        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA user_version;";
                long version = Convert.ToInt64(command.ExecuteScalar());
                Assert.That(version, Is.EqualTo(2));
            }

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'Configuration';";
                object? tableName = command.ExecuteScalar();
                Assert.That(tableName, Is.EqualTo("Configuration"),
                    "The Configuration table must be created during migration from schema v1 to v2.");
            }
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteOldBackups_KeepOne_DeletesAllButNewest()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 1));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 2));
        _sqlitePersistenceContext!.WriteBackup(new DateTime(2024, 3, 3));

        _sqlitePersistenceContext!.DeleteOldBackups(1);

        string[] files = Directory.GetFiles(_backupsDirectory!, "*.zip");

        Assert.That(files, Has.Length.EqualTo(1));
        Assert.That(files[0], Does.Contain("20240303"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Vacuum_EmptyDatabase_CompletesWithoutError()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        Assert.DoesNotThrow(() => _sqlitePersistenceContext!.Vacuum());

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Vacuum_DatabaseWithData_CompletesWithoutError()
    {
        _sqlitePersistenceContext!.Initialize(_databaseDirectory!);

        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Family");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Travel");
        _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Work");

        Assert.DoesNotThrow(() => _sqlitePersistenceContext!.Vacuum());

        // Folders must still be intact after VACUUM
        Assert.That(_sqlitePersistenceContext!.Folders.Count(), Is.EqualTo(3));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Vacuum_NotInitialized_ThrowsInvalidOperationException()
    {
        const string expectedMessage = "Db context has not been initialized.";

        InvalidOperationException? exception =
            Assert.Throws<InvalidOperationException>(() => _sqlitePersistenceContext!.Vacuum());

        Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

        _testLogger.AssertLogExceptions([new InvalidOperationException(expectedMessage)],
            typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Vacuum_FactoryThrowsDuringOpen_LogsErrorAndRethrows()
    {
        const string expectedMessage = "disk I/O error during VACUUM";

        ISqliteConnectionFactory factoryMock = Substitute.For<ISqliteConnectionFactory>();
        factoryMock.DatabasePath.Returns(Path.Combine(_databaseDirectory!,
            SqlitePersistenceContext.DATABASE_FILE_NAME));
        factoryMock.Open().Throws(new InvalidOperationException(expectedMessage));

        TestLogger<SqlitePersistenceContext> testLogger = new();
        SqlitePersistenceContext context = new(factoryMock, Substitute.For<ISqliteBackupService>(), testLogger);

        try
        {
            InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(context.Vacuum);

            Assert.That(exception?.Message, Is.EqualTo(expectedMessage));

            testLogger.AssertLogExceptions([new InvalidOperationException(expectedMessage)],
                typeof(SqlitePersistenceContext));
        }
        finally
        {
            context.Dispose();
            testLogger.LoggingAssertTearDown();
        }
    }

    private void CreateAbortTrigger(string triggerName, string tableName, string operation, string message)
    {
        using (SqliteConnection connection = _factory!.Open())
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = $"""
                                       CREATE TRIGGER {triggerName}
                                       BEFORE {operation} ON {tableName}
                                       BEGIN
                                           SELECT RAISE(ABORT, '{message}');
                                       END;
                                       """;
                command.ExecuteNonQuery();
            }
        }
    }

    private static Asset CreateAsset(Folder folder, string fileName = FileNames.IMAGE_1_JPG,
        string hash = Hashes.IMAGE_1_JPG, bool isCorrupted = false, bool isRotated = false)
    {
        return new()
        {
            FolderId = folder.Id,
            Folder = folder,
            FileName = fileName,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = ImageRotation.Rotate0,
            Hash = hash,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = isCorrupted, Message = isCorrupted ? "The asset is corrupted" : null },
                Rotated = new() { IsTrue = isRotated, Message = isRotated ? "The asset has been rotated" : null }
            }
        };
    }
}
