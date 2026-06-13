using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Persistence.Repositories;

[TestFixture]
public class AssetPersistenceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private Folder? _testFolder;

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

        _testFolder = _sqlitePersistenceContext.Folders.Insert(_assetsDirectory!);
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
        Asset asset = CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null);

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(retrievedAsset, Is.Not.Null);
        Assert.That(retrievedAsset!.FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
        Assert.That(retrievedAsset.Hash, Is.EqualTo(Hashes.IMAGE_1_JPG));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Upsert_ExistingAsset_UpdatesFields()
    {
        Asset asset = CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null);

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset updatedAsset = CreateAsset(
            _testFolder.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_DUPLICATE_JPG,
            PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
            ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG,
            ImageRotation.Rotate0, false, null, false, null);

        _sqlitePersistenceContext.Assets.Upsert(updatedAsset);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(1));

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(retrievedAsset!.Hash, Is.EqualTo(Hashes.IMAGE_1_DUPLICATE_JPG));

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
            CreateAsset(
                _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
                PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
                ImageRotation.Rotate0, false, null, false, null),
            CreateAsset(
                _testFolder.Id, FileNames.IMAGE_11_90_DEG_HEIC, Hashes.IMAGE_11_90_DEG_HEIC,
                PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC,
                ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC,
                ImageRotation.Rotate90, false, null, true, "The asset has been rotated"),
            CreateAsset(
                _testFolder.Id, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG,
                PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
                ImageRotation.Rotate0, true, "The asset is corrupted", false, null)
        ];

        _sqlitePersistenceContext!.Assets.UpsertMany(assets);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(3));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void UpsertMany_WithExistingAssets_UpdatesThem()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));

        List<Asset> assets =
        [
            CreateAsset(
                _testFolder.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_DUPLICATE_JPG,
                PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
                ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG,
                ImageRotation.Rotate0, false, null, false, null),
            CreateAsset(
                _testFolder.Id, FileNames.IMAGE_2_JPG, Hashes.IMAGE_2_JPG,
                PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG,
                ImageRotation.Rotate0, false, null, false, null)
        ];

        _sqlitePersistenceContext.Assets.UpsertMany(assets);

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(2));

        Asset? updatedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(updatedAsset!.Hash, Is.EqualTo(Hashes.IMAGE_1_DUPLICATE_JPG));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_ExistingAsset_ReturnsTrue()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));

        bool isDeleted = _sqlitePersistenceContext.Assets.Delete(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(isDeleted, Is.True);
        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.Zero);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Delete_NonExistentAsset_ReturnsFalse()
    {
        bool isDeleted = _sqlitePersistenceContext!.Assets.Delete(
            _testFolder!.Id, FileNames.NON_EXISTENT_FILE_JPG);

        Assert.That(isDeleted, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void DeleteByFolderId_WithAssets_ReturnsDeletedCount()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder.Id, FileNames.IMAGE_2_JPG, Hashes.IMAGE_2_JPG,
            PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
            ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG,
            ImageRotation.Rotate0, false, null, false, null));

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
        Asset asset = CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_90_DEG_JPG, Hashes.IMAGE_1_90_DEG_JPG,
            PixelWidthAsset.IMAGE_1_90_DEG_JPG, PixelHeightAsset.IMAGE_1_90_DEG_JPG,
            ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG, ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG,
            ImageRotation.Rotate90, true, "Bad EXIF data", true, "The asset has been rotated");

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(
            _testFolder.Id, FileNames.IMAGE_1_90_DEG_JPG);

        Assert.That(retrievedAsset, Is.Not.Null);
        Assert.That(retrievedAsset!.FolderId, Is.EqualTo(_testFolder.Id));
        Assert.That(retrievedAsset.FileName, Is.EqualTo(FileNames.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.ImageRotation, Is.EqualTo(ImageRotation.Rotate90));
        Assert.That(retrievedAsset.Pixel.Asset.Width, Is.EqualTo(PixelWidthAsset.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.Pixel.Asset.Height, Is.EqualTo(PixelHeightAsset.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.Pixel.Thumbnail.Width, Is.EqualTo(ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.Pixel.Thumbnail.Height, Is.EqualTo(ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.Hash, Is.EqualTo(Hashes.IMAGE_1_90_DEG_JPG));
        Assert.That(retrievedAsset.Metadata.Corrupted.IsTrue, Is.True);
        Assert.That(retrievedAsset.Metadata.Corrupted.Message, Is.EqualTo("Bad EXIF data"));
        Assert.That(retrievedAsset.Metadata.Rotated.IsTrue, Is.True);
        Assert.That(retrievedAsset.Metadata.Rotated.Message, Is.EqualTo("The asset has been rotated"));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_AssetWithNullMessages_RoundTripsCorrectly()
    {
        Asset asset = CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null);

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(retrievedAsset!.Metadata.Corrupted.Message, Is.Null);
        Assert.That(retrievedAsset.Metadata.Corrupted.IsTrue, Is.False);
        Assert.That(retrievedAsset.Metadata.Rotated.Message, Is.Null);
        Assert.That(retrievedAsset.Metadata.Rotated.IsTrue, Is.False);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_AssetWithInvalidRotation_MapsToRotate0()
    {
        Asset asset = CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            (ImageRotation)4, false, null, false, null);

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, FileNames.IMAGE_1_JPG);

        Assert.That(retrievedAsset!.ImageRotation, Is.EqualTo(ImageRotation.Rotate0));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void Get_NonExistentAsset_ReturnsNull()
    {
        Asset? asset = _sqlitePersistenceContext!.Assets.Get(
            _testFolder!.Id, FileNames.NON_EXISTENT_FILE_JPG);

        Assert.That(asset, Is.Null);

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByFolderId_WithAssets_ReturnsAll()
    {
        _sqlitePersistenceContext!.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder.Id, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG,
            PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
            ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
            ImageRotation.Rotate0, false, null, false, null));

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
        string otherFolderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES);
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(otherFolderPath);

        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            otherFolder.Id, FileNames.IMAGE_2_JPG, Hashes.IMAGE_2_JPG,
            PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
            ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG,
            ImageRotation.Rotate0, false, null, false, null));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetByFolderId(_testFolder.Id);

        Assert.That(assets, Has.Count.EqualTo(1));
        Assert.That(assets[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));

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
        string otherFolderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES);
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(otherFolderPath);

        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            otherFolder.Id, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG,
            PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
            ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
            ImageRotation.Rotate0, false, null, false, null));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetAll();

        Assert.That(assets, Has.Count.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    public void GetByHash_ExistingHash_ReturnsMatchingAssets()
    {
        string otherFolderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES);
        Folder otherFolder = _sqlitePersistenceContext!.Folders.Insert(otherFolderPath);

        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            otherFolder.Id, FileNames.IMAGE_1_DUPLICATE_JPG, Hashes.IMAGE_1_DUPLICATE_JPG,
            PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
            ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder.Id, FileNames.IMAGE_9_PNG, Hashes.IMAGE_9_PNG,
            PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
            ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG,
            ImageRotation.Rotate0, false, null, false, null));

        IReadOnlyList<Asset> assets = _sqlitePersistenceContext.Assets.GetByHash(Hashes.IMAGE_1_JPG);

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
        _sqlitePersistenceContext!.Assets.Upsert(CreateAsset(
            _testFolder!.Id, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            ImageRotation.Rotate0, false, null, false, null));
        _sqlitePersistenceContext.Assets.Upsert(CreateAsset(
            _testFolder.Id, FileNames.IMAGE_2_JPG, Hashes.IMAGE_2_JPG,
            PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
            ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG,
            ImageRotation.Rotate0, false, null, false, null));

        Assert.That(_sqlitePersistenceContext.Assets.Count(), Is.EqualTo(2));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    [Test]
    [TestCase(ImageRotation.Rotate0, FileNames.IMAGE_1_JPG, Hashes.IMAGE_1_JPG)]
    [TestCase(ImageRotation.Rotate90, FileNames.IMAGE_1_90_DEG_JPG, Hashes.IMAGE_1_90_DEG_JPG)]
    [TestCase(ImageRotation.Rotate180, FileNames.IMAGE_1_180_DEG_JPG, Hashes.IMAGE_1_180_DEG_JPG)]
    [TestCase(ImageRotation.Rotate270, FileNames.IMAGE_1_270_DEG_JPG, Hashes.IMAGE_1_270_DEG_JPG)]
    public void Upsert_AllRotationValues_RoundTripCorrectly(
        ImageRotation rotation, string fileName, string hash)
    {
        Asset asset = CreateAsset(
            _testFolder!.Id, fileName, hash,
            PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
            ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG,
            rotation, false, null, rotation != ImageRotation.Rotate0,
            rotation != ImageRotation.Rotate0 ? "The asset has been rotated" : null);

        _sqlitePersistenceContext!.Assets.Upsert(asset);

        Asset? retrievedAsset = _sqlitePersistenceContext.Assets.Get(_testFolder.Id, fileName);

        Assert.That(retrievedAsset!.ImageRotation, Is.EqualTo(rotation));

        _testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
    }

    private static Asset CreateAsset(
        Guid folderId, string fileName, string hash,
        int pixelWidth, int pixelHeight,
        int thumbnailWidth, int thumbnailHeight,
        ImageRotation rotation,
        bool isCorrupted, string? corruptedMessage,
        bool isRotated, string? rotatedMessage)
    {
        return new()
        {
            FolderId = folderId,
            Folder = new() { Id = Guid.Empty, Path = string.Empty },
            FileName = fileName,
            ImageRotation = rotation,
            Pixel = new()
            {
                Asset = new() { Width = pixelWidth, Height = pixelHeight },
                Thumbnail = new() { Width = thumbnailWidth, Height = thumbnailHeight }
            },
            ThumbnailCreationDateTime = new(2024, 6, 7, 8, 54, 37),
            Hash = hash,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = isCorrupted, Message = corruptedMessage },
                Rotated = new() { IsTrue = isRotated, Message = rotatedMessage }
            }
        };
    }
}
