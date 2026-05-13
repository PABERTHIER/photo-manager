using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class AssetPersistenceTests
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
    public void Upsert_NewAsset_InsertsSuccessfully()
    {
        Asset asset = CreateTestAsset(_testFolder!.Id, "image1.jpg", "hash1");

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, "image1.jpg");

        Assert.That(retrievedAsset, Is.Not.Null);
        Assert.That(retrievedAsset!.FileName, Is.EqualTo("image1.jpg"));
        Assert.That(retrievedAsset.Hash, Is.EqualTo("hash1"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_ExistingAsset_UpdatesFields()
    {
        Asset asset = CreateTestAsset(_testFolder!.Id, "image1.jpg", "hashOriginal");
        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset updatedAsset = CreateTestAsset(_testFolder.Id, "image1.jpg", "hashUpdated");
        _sqlitePersistenceContext.Assets.Upsert(updatedAsset);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(1));

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, "image1.jpg");

        Assert.That(retrievedAsset!.Hash, Is.EqualTo("hashUpdated"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertMany_EmptyList_DoesNothing()
    {
        _sqlitePersistenceContext!.Assets.UpsertMany([]);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertMany_MultipleAssets_InsertsAll()
    {
        List<Asset> assets =
        [
            CreateTestAsset(_testFolder!.Id, "img1.jpg", "h1"),
            CreateTestAsset(_testFolder.Id, "img2.jpg", "h2"),
            CreateTestAsset(_testFolder.Id, "img3.jpg", "h3")
        ];

        _sqlitePersistenceContext!.Assets.UpsertMany(assets);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(3));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertMany_WithExistingAssets_UpdatesThem()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "img1.jpg", "oldHash"));

        List<Asset> assets =
        [
            CreateTestAsset(_testFolder.Id, "img1.jpg", "newHash"),
            CreateTestAsset(_testFolder.Id, "img2.jpg", "h2")
        ];

        _sqlitePersistenceContext.Assets.UpsertMany(assets);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(2));

        Asset? updatedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, "img1.jpg");

        Assert.That(updatedAsset!.Hash, Is.EqualTo("newHash"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_ExistingAsset_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "img.jpg", "h"));

        bool isDeleted = _sqlitePersistenceContext.Assets.Delete(_testFolder.Id, "img.jpg");

        Assert.That(isDeleted, Is.True);
        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_NonExistentAsset_ReturnsFalse()
    {
        bool isDeleted = _sqlitePersistenceContext!.Assets.Delete(_testFolder!.Id, "nonexistent.jpg");

        Assert.That(isDeleted, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_WithAssets_ReturnsDeletedCount()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "img1.jpg", "h1"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder.Id, "img2.jpg", "h2"));

        int result = _sqlitePersistenceContext.Assets.DeleteByFolderId(_testFolder.Id);

        Assert.That(result, Is.EqualTo(2));
        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_NoAssets_ReturnsZero()
    {
        int result = _sqlitePersistenceContext!.Assets.DeleteByFolderId(Guid.NewGuid());

        Assert.That(result, Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_ExistingAsset_ReturnsWithAllFields()
    {
        Asset asset = new()
        {
            FolderId = _testFolder!.Id,
            Folder = new() { Id = Guid.Empty, Path = string.Empty },
            FileName = "test.jpg",
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = 3840, Height = 2160 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            ThumbnailCreationDateTime = new(2024, 6, 15, 10, 30, 0),
            Hash = "abc123def456",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "Bad EXIF data" },
                Rotated = new() { IsTrue = true, Message = "Auto-rotated 90°" }
            }
        };

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? assetRetrieved = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, "test.jpg");

        Assert.That(assetRetrieved, Is.Not.Null);
        Assert.That(assetRetrieved!.FolderId, Is.EqualTo(_testFolder.Id));
        Assert.That(assetRetrieved.FileName, Is.EqualTo("test.jpg"));
        Assert.That(assetRetrieved.ImageRotation, Is.EqualTo(Rotation.Rotate90));
        Assert.That(assetRetrieved.Pixel.Asset.Width, Is.EqualTo(3840));
        Assert.That(assetRetrieved.Pixel.Asset.Height, Is.EqualTo(2160));
        Assert.That(assetRetrieved.Pixel.Thumbnail.Width, Is.EqualTo(200));
        Assert.That(assetRetrieved.Pixel.Thumbnail.Height, Is.EqualTo(112));
        Assert.That(assetRetrieved.ThumbnailCreationDateTime,
            Is.EqualTo(new DateTime(2024, 6, 15, 10, 30, 0)));
        Assert.That(assetRetrieved.Hash, Is.EqualTo("abc123def456"));
        Assert.That(assetRetrieved.Metadata.Corrupted.IsTrue, Is.True);
        Assert.That(assetRetrieved.Metadata.Corrupted.Message, Is.EqualTo("Bad EXIF data"));
        Assert.That(assetRetrieved.Metadata.Rotated.IsTrue, Is.True);
        Assert.That(assetRetrieved.Metadata.Rotated.Message, Is.EqualTo("Auto-rotated 90°"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_AssetWithNullMessages_RoundTripsCorrectly()
    {
        Asset asset = CreateTestAsset(_testFolder!.Id, "clean.jpg", "cleanHash");
        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, "clean.jpg");

        Assert.That(retrievedAsset!.Metadata.Corrupted.Message, Is.Null);
        Assert.That(retrievedAsset.Metadata.Corrupted.IsTrue, Is.False);
        Assert.That(retrievedAsset.Metadata.Rotated.Message, Is.Null);
        Assert.That(retrievedAsset.Metadata.Rotated.IsTrue, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_NonExistentAsset_ReturnsNull()
    {
        Asset? asset = _sqlitePersistenceContext!.Assets.Get(_testFolder!.Id, "nonexistent.jpg");

        Assert.That(asset, Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_WithAssets_ReturnsAll()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "a.jpg", "h1"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder.Id, "b.jpg", "h2"));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetByFolderId(_testFolder.Id);

        Assert.That(assets, Has.Count.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_NoAssets_ReturnsEmptyList()
    {
        IReadOnlyList<Asset> assets = _sqlitePersistenceContext!.Assets.GetByFolderId(Guid.NewGuid());

        Assert.That(assets, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_OnlyReturnsAssetsForSpecifiedFolder()
    {
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Other");

        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "a.jpg", "h1"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(otherFolder.Id, "b.jpg", "h2"));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetByFolderId(_testFolder.Id);

        Assert.That(assets, Has.Count.EqualTo(1));
        Assert.That(assets[0].FileName, Is.EqualTo("a.jpg"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_EmptyTable_ReturnsEmptyList()
    {
        IReadOnlyList<Asset> assets = _sqlitePersistenceContext!.Assets.GetAll();

        Assert.That(assets, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetAll_WithAssets_ReturnsAll()
    {
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Other");

        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "a.jpg", "h1"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(otherFolder.Id, "b.jpg", "h2"));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetAll();

        Assert.That(assets, Has.Count.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByHash_ExistingHash_ReturnsMatchingAssets()
    {
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(@"C:\Photos\Other");

        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "a.jpg", "sameHash"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(otherFolder.Id, "b.jpg", "sameHash"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder.Id, "c.jpg", "differentHash"));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetByHash("sameHash");

        Assert.That(assets, Has.Count.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByHash_NonExistentHash_ReturnsEmptyList()
    {
        IReadOnlyList<Asset> assets = _sqlitePersistenceContext!.Assets.GetByHash("nonexistent");

        Assert.That(assets, Is.Empty);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Count_EmptyTable_ReturnsZero()
    {
        Assert.That(_sqlitePersistenceContext!.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Count_WithAssets_ReturnsCorrectCount()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateTestAsset(_testFolder!.Id, "a.jpg", "h1"));
        _sqlitePersistenceContext.Assets.Upsert(CreateTestAsset(_testFolder.Id, "b.jpg", "h2"));

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_AllRotationValues_RoundTripCorrectly()
    {
        Rotation[] rotations =
        [
            Rotation.Rotate0,
            Rotation.Rotate90,
            Rotation.Rotate180,
            Rotation.Rotate270
        ];

        for (int i = 0; i < rotations.Length; i++)
        {
            Asset asset = new()
            {
                FolderId = _testFolder!.Id,
                Folder = new() { Id = Guid.Empty, Path = string.Empty },
                FileName = $"rot{i}.jpg",
                ImageRotation = rotations[i],
                Pixel = new()
                {
                    Asset = new() { Width = 100, Height = 100 },
                    Thumbnail = new() { Width = 50, Height = 50 }
                },
                ThumbnailCreationDateTime = DateTime.Now,
                Hash = $"hash{i}"
            };

            _sqlitePersistenceContext!.Assets.Upsert(asset);

            Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, $"rot{i}.jpg");

            Assert.That(retrievedAsset!.ImageRotation, Is.EqualTo(rotations[i]));
        }

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    private static Asset CreateTestAsset(Guid folderId, string fileName, string hash)
    {
        return new()
        {
            FolderId = folderId,
            Folder = new() { Id = Guid.Empty, Path = string.Empty },
            FileName = fileName,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            ThumbnailCreationDateTime = new(2024, 1, 1),
            Hash = hash,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }
}
