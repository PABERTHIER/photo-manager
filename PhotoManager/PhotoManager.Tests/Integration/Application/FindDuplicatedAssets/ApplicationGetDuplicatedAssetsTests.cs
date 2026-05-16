using DHashes = PhotoManager.Tests.Integration.Constants.DHashes;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PHashes = PhotoManager.Tests.Integration.Constants.PHashes;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Application.FindDuplicatedAssets;

[TestFixture]
public class ApplicationGetDuplicatedAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    private IPathProviderService? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
        {
            FolderId = new("876283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            ImageRotation = Rotation.Rotate0,
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
            Hash = Hashes.IMAGE_1_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = new("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            ImageRotation = Rotation.Rotate90,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = new("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new("886283c6-780e-4ad5-975c-be63044c087a"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = true, Message = "The asset is corrupted" },
                Rotated = new() { IsTrue = true, Message = "The asset has been rotated" }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(_pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsBasicHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        const string hash1 = Hashes.IMAGE_1_JPG;
        const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
        const string hash3 = Hashes._1336_JPG;

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == hash1));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == hash2));
        List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == hash3));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)
            && a.Hash == hash1), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)
            && a.Hash == hash1), Is.True);

        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)
            && a.Hash == hash2), Is.True);
        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)
            && a.Hash == hash2), Is.True);

        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)
            && a.Hash == hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)
            && a.Hash == hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)
            && a.Hash == hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)
            && a.Hash == hash3), Is.True);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsDHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, true, false, false, false);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        const string dHash1 = DHashes.IMAGE_1_JPG;
        const string dHash2 = DHashes.IMAGE_9_PNG;
        const string dHash3 = DHashes.IMAGE_11_HEIC;
        const string dHash4 = DHashes._1336_JPG;
        const string dHash5 = DHashes._1336_BOTTOM_LEFT_PART_JPG;
        const string dHash6 = DHashes._1336_LEFT_PART_JPG;
        const string dHash7 = DHashes._1336_RIGHT_PART_JPG;

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(7));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash1));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash2));
        List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash3));
        List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash4));
        List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash5));
        List<Asset> sixthDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash6));
        List<Asset> seventhDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == dHash7));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
        Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(4));
        Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(sixthDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(seventhDuplicatedAssetsSet, Has.Count.EqualTo(2));

        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)
            && a.Hash == dHash1), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)
            && a.Hash == dHash1), Is.True);

        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)
            && a.Hash == dHash2), Is.True);
        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)
            && a.Hash == dHash2), Is.True);

        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_11_HEIC
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_11_HEIC)
            && a.Hash == dHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1337_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1337_JPG)
            && a.Hash == dHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_SHIT_QUALITY_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_SHIT_QUALITY_JPG)
            && a.Hash == dHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_SMALL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_SMALL_JPG)
            && a.Hash == dHash3), Is.True);

        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)
            && a.Hash == dHash4), Is.True);
        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)
            && a.Hash == dHash4), Is.True);
        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)
            && a.Hash == dHash4), Is.True);
        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)
            && a.Hash == dHash4), Is.True);

        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_BOTTOM_LEFT_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_BOTTOM_LEFT_PART_JPG)
            && a.Hash == dHash5), Is.True);
        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_BOTTOM_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_BOTTOM_PART_JPG)
            && a.Hash == dHash5), Is.True);

        Assert.That(sixthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_LEFT_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_LEFT_PART_JPG)
            && a.Hash == dHash6), Is.True);
        Assert.That(sixthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_TOP_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_TOP_PART_JPG)
            && a.Hash == dHash6), Is.True);

        Assert.That(seventhDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_RIGHT_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_RIGHT_PART_JPG)
            && a.Hash == dHash7), Is.True);
        Assert.That(seventhDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_TOP_RIGHT_PART_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_TOP_RIGHT_PART_JPG)
            && a.Hash == dHash7), Is.True);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsMD5Hash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, true, false, false);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        const string mD5Hash1 = MD5Hashes.IMAGE_1_JPG;
        const string mD5Hash2 = MD5Hashes.IMAGE_9_PNG;
        const string mD5Hash3 = MD5Hashes._1336_JPG;

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == mD5Hash1));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == mD5Hash2));
        List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == mD5Hash3));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)
            && a.Hash == mD5Hash1), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)
            && a.Hash == mD5Hash1), Is.True);

        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)
            && a.Hash == mD5Hash2), Is.True);
        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)
            && a.Hash == mD5Hash2), Is.True);

        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)
            && a.Hash == mD5Hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)
            && a.Hash == mD5Hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)
            && a.Hash == mD5Hash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)
            && a.Hash == mD5Hash3), Is.True);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, true, false);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        const string pHash1 = PHashes.IMAGE_1_JPG;
        const string pHash2 = PHashes.IMAGE_9_PNG;
        const string pHash3 = PHashes._1336_JPG;

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(3));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash1));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash2));
        List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash3));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)
            && a.Hash == pHash1), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)
            && a.Hash == pHash1), Is.True);

        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)
            && a.Hash == pHash2), Is.True);
        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)
            && a.Hash == pHash2), Is.True);

        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)
            && a.Hash == pHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)
            && a.Hash == pHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)
            && a.Hash == pHash3), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)
            && a.Hash == pHash3), Is.True);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHashAndDetectThumbnails_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, "100");
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200");
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");
        configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath!);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(_pathProviderServiceMock!, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        const string pHash1 = PHashes.IMAGE_1_JPG;
        const string pHash2 = PHashes.IMAGE_9_PNG;
        const string pHash3 = PHashes._1336_JPG;

        const string pHash4First = PHashes._1336_1_K_JPG;
        const string pHash4Second = PHashes._1336_2_K_JPG;

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(duplicatedAssets, Has.Count.EqualTo(4));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash1));
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash2));
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s => s.All(a => a.Hash == pHash3));
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets.First(s =>
                s.Any(a => a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_1_K_JPG)));

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
                && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)
                && a.Hash == pHash1), Is.True);
            Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
                && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)
                && a.Hash == pHash1), Is.True);

            Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
                && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)
                && a.Hash == pHash2), Is.True);
            Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
                && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)
                && a.Hash == pHash2), Is.True);

            Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
                && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)
                && a.Hash == pHash3), Is.True);
            Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
                && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)
                && a.Hash == pHash3), Is.True);
            Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
                && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)
                && a.Hash == pHash3), Is.True);
            Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
                && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)
                && a.Hash == pHash3), Is.True);

            Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_1_K_JPG
                && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_1_K_JPG)
                && a.Hash == pHash4First), Is.True);
            Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_2_K_JPG
                && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_2_K_JPG)
                && a.Hash == pHash4Second), Is.True);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssets_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string duplicatesDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string duplicatesNotDuplicateSample1Directory =
            Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string duplicatesPartDirectory = Path.Combine(duplicatesDirectory, Directories.PART);
        string duplicatesResolutionDirectory = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string duplicatesThumbnailDirectory = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        await _application!.CatalogAssetsAsync(_ => { });

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(5));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s =>
            s.Any(a => a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG)));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s =>
            s.Any(a => a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_2_DUPLICATED_JPG)));
        List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets.First(s =>
            s.Any(a => a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG)));
        List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets.First(s =>
            s.Any(a => a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC)));
        List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets.First(s =>
            s.Any(a => a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(3));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(3));
        Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(4));

        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG)), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)), Is.True);
        Assert.That(firstDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1_DUPLICATE_JPG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)),
            Is.True);

        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_2_DUPLICATED_JPG
            && a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_2_DUPLICATED_JPG)), Is.True);
        Assert.That(secondDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_2_JPG
            && a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_2_JPG)), Is.True);

        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG)), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)), Is.True);
        Assert.That(thirdDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_9_DUPLICATE_PNG
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)),
            Is.True);

        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_11_HEIC
            && a.FullPath == Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC)), Is.True);
        Assert.That(fourthDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_11_HEIC
            && a.FullPath == Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_11_HEIC)), Is.True);

        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_JPG
            && a.FullPath == Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)),
            Is.True);
        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)), Is.True);
        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames._1336_4_K_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)),
            Is.True);
        Assert.That(fifthDuplicatedAssetsSet.Any(a => a.FileName == FileNames.IMAGE_1336_ORIGINAL_JPG
            && a.FullPath == Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)),
            Is.True);
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string sourcePath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
        string sourcePath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = sourcePath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = sourcePath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset2 = _asset2!.WithFolder(folder2);
        _asset3 = _asset3!.WithFolder(folder2);
        _asset4 = _asset4!.WithFolder(folder2);
        _asset5 = _asset5!.WithFolder(folder2);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);
        _testableAssetRepository.AddAsset(_asset3!, assetData1);
        _testableAssetRepository.AddAsset(_asset4!, assetData2);
        _testableAssetRepository.AddAsset(_asset5!, assetData1);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Not.Empty);
        Assert.That(duplicatedAssets, Has.Count.EqualTo(2));

        List<Asset> firstDuplicatedAssetsSet = duplicatedAssets.First(s => s.Any(a => a.FileName == _asset1!.FileName));
        List<Asset> secondDuplicatedAssetsSet = duplicatedAssets.First(s => s.Any(a => a.FileName == _asset2!.FileName));

        Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
        Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));

        Asset? duplicatedAsset1 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset1.FileName);
        Asset? duplicatedAsset3 = firstDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);

        Assert.That(duplicatedAsset1, Is.Not.Null);
        Assert.That(duplicatedAsset3, Is.Not.Null);
        Assert.That(duplicatedAsset1!.FileName, Is.EqualTo(_asset1.FileName));
        Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));

        Asset? duplicatedAsset2 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
        Asset? duplicatedAsset4 = secondDuplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);

        Assert.That(duplicatedAsset2, Is.Not.Null);
        Assert.That(duplicatedAsset4, Is.Not.Null);
        Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
        Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        const string hash = Hashes.IMAGE_9_PNG;
        string sourcePath = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

        _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
        _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
        _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
        _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset2!, assetData);
        _testableAssetRepository.AddAsset(_asset3!, assetData);
        _testableAssetRepository.AddAsset(_asset4!, assetData);
        _testableAssetRepository.AddAsset(_asset5!, assetData);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Not.Empty);
        Assert.That(duplicatedAssets, Has.Count.EqualTo(1));

        List<Asset> duplicatedAssetsSet = duplicatedAssets[0];

        Assert.That(duplicatedAssetsSet, Has.Count.EqualTo(4));

        Asset? duplicatedAsset2 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset2.FileName);
        Asset? duplicatedAsset3 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset3.FileName);
        Asset? duplicatedAsset4 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset4.FileName);
        Asset? duplicatedAsset5 = duplicatedAssetsSet.FirstOrDefault(x => x.FileName == _asset5.FileName);

        Assert.That(duplicatedAsset2, Is.Not.Null);
        Assert.That(duplicatedAsset3, Is.Not.Null);
        Assert.That(duplicatedAsset4, Is.Not.Null);
        Assert.That(duplicatedAsset5, Is.Not.Null);

        Assert.That(duplicatedAsset2!.FileName, Is.EqualTo(_asset2.FileName));
        Assert.That(duplicatedAsset3!.FileName, Is.EqualTo(_asset3.FileName));
        Assert.That(duplicatedAsset4!.FileName, Is.EqualTo(_asset4.FileName));
        Assert.That(duplicatedAsset5!.FileName, Is.EqualTo(_asset5.FileName));
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
        string folderPath2 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_2);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset3 = _asset3!.WithFolder(folder2);

        byte[] assetData1 = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset3!, assetData1);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string folderPath1 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_1);
        string folderPath2 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_2);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1);
        _asset2 = _asset2!.WithFolder(folder2);
        _asset3 = _asset3!.WithFolder(folder2);
        _asset4 = _asset4!.WithFolder(folder2);
        _asset5 = _asset5!.WithFolder(folder2);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);
        _testableAssetRepository.AddAsset(_asset3!, assetData1);
        _testableAssetRepository.AddAsset(_asset4!, assetData2);
        _testableAssetRepository.AddAsset(_asset5!, assetData1);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);

        Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

        _asset1 = _asset1!.WithFolder(folder);
        _asset2 = _asset2!.WithFolder(folder);

        byte[] assetData1 = [1, 2, 3];
        byte[] assetData2 = [];

        _testableAssetRepository!.AddAsset(_asset1!, assetData1);
        _testableAssetRepository.AddAsset(_asset2!, assetData2);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Is.Empty);
    }
}
