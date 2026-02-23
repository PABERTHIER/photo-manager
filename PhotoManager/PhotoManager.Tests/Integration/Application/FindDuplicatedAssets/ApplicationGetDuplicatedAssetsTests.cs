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
    private AssetRepository? _assetRepository;
    private Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

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

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsBasicHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
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

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(hash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(hash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsDHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, true, false, false, false);

        try
        {
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

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];
            List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets[4];
            List<Asset> sixthDuplicatedAssetsSet = duplicatedAssets[5];
            List<Asset> seventhDuplicatedAssetsSet = duplicatedAssets[6];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(4));
            Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(sixthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(seventhDuplicatedAssetsSet, Has.Count.EqualTo(2));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_11_HEIC));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_11_HEIC)));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1337_JPG));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1337_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames.IMAGE_1336_SHIT_QUALITY_JPG));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_SHIT_QUALITY_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(dHash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_SMALL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_SMALL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(dHash3));

            Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
            Assert.That(fourthDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
            Assert.That(fourthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
            Assert.That(fourthDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
            Assert.That(fourthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
            Assert.That(fourthDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
            Assert.That(fourthDuplicatedAssetsSet[2].Hash, Is.EqualTo(dHash4));
            Assert.That(fourthDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
            Assert.That(fourthDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
            Assert.That(fourthDuplicatedAssetsSet[3].Hash, Is.EqualTo(dHash4));

            Assert.That(fifthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_BOTTOM_LEFT_PART_JPG));
            Assert.That(fifthDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_BOTTOM_LEFT_PART_JPG)));
            Assert.That(fifthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash5));
            Assert.That(fifthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_BOTTOM_PART_JPG));
            Assert.That(fifthDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_BOTTOM_PART_JPG)));
            Assert.That(fifthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash5));

            Assert.That(sixthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_LEFT_PART_JPG));
            Assert.That(sixthDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_LEFT_PART_JPG)));
            Assert.That(sixthDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash6));
            Assert.That(sixthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_TOP_PART_JPG));
            Assert.That(sixthDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_TOP_PART_JPG)));
            Assert.That(sixthDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash6));

            Assert.That(seventhDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_RIGHT_PART_JPG));
            Assert.That(seventhDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_RIGHT_PART_JPG)));
            Assert.That(seventhDuplicatedAssetsSet[0].Hash, Is.EqualTo(dHash7));
            Assert.That(seventhDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_TOP_RIGHT_PART_JPG));
            Assert.That(seventhDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_TOP_RIGHT_PART_JPG)));
            Assert.That(seventhDuplicatedAssetsSet[1].Hash, Is.EqualTo(dHash7));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsMD5Hash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, true, false, false);

        try
        {
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

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(mD5Hash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(mD5Hash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHash_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, true, false);

        try
        {
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

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash1));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));
            Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash1));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
            Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash2));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));
            Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash2));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(pHash3));
            Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
            Assert.That(thirdDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
            Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(pHash3));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssetsPHashAndDetectThumbnails_ReturnsListOfDuplicatedSets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, "100");
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, "200");
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, "150");
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, "true");
        configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, "true");

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);

        try
        {
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

                List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
                List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
                List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
                List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];

                Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(2));
                Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
                Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(4));
                Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(2));

                Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
                Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
                Assert.That(firstDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash1));
                Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
                Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));
                Assert.That(firstDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash1));

                Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
                Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
                Assert.That(secondDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash2));
                Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
                Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));
                Assert.That(secondDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash2));

                Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
                Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
                Assert.That(thirdDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash3));
                Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
                Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
                Assert.That(thirdDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash3));
                Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
                Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
                Assert.That(thirdDuplicatedAssetsSet[2].Hash, Is.EqualTo(pHash3));
                Assert.That(thirdDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
                Assert.That(thirdDuplicatedAssetsSet[3].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
                Assert.That(thirdDuplicatedAssetsSet[3].Hash, Is.EqualTo(pHash3));

                Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_1_K_JPG));
                Assert.That(fourthDuplicatedAssetsSet[0].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_1_K_JPG)));
                Assert.That(fourthDuplicatedAssetsSet[0].Hash, Is.EqualTo(pHash4First));
                Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_2_K_JPG));
                Assert.That(fourthDuplicatedAssetsSet[1].FullPath,
                    Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_2_K_JPG)));
                Assert.That(fourthDuplicatedAssetsSet[1].Hash, Is.EqualTo(pHash4Second));
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetDuplicatedAssets_CataloguedAssets_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];
            List<Asset> thirdDuplicatedAssetsSet = duplicatedAssets[2];
            List<Asset> fourthDuplicatedAssetsSet = duplicatedAssets[3];
            List<Asset> fifthDuplicatedAssetsSet = duplicatedAssets[4];

            Assert.That(firstDuplicatedAssetsSet, Has.Count.EqualTo(3));
            Assert.That(secondDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(thirdDuplicatedAssetsSet, Has.Count.EqualTo(3));
            Assert.That(fourthDuplicatedAssetsSet, Has.Count.EqualTo(2));
            Assert.That(fifthDuplicatedAssetsSet, Has.Count.EqualTo(4));

            Assert.That(firstDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(firstDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG)));
            Assert.That(firstDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames.IMAGE_1_DUPLICATE_JPG));
            Assert.That(firstDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_1_DUPLICATE_JPG)));

            Assert.That(secondDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_2_DUPLICATED_JPG));
            Assert.That(secondDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(_dataDirectory!, FileNames.IMAGE_2_DUPLICATED_JPG)));
            Assert.That(secondDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_2_JPG));
            Assert.That(secondDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(_dataDirectory!, FileNames.IMAGE_2_JPG)));

            Assert.That(thirdDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(thirdDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG)));
            Assert.That(thirdDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(thirdDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_PNG)));
            Assert.That(thirdDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames.IMAGE_9_DUPLICATE_PNG));
            Assert.That(thirdDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_9_DUPLICATE_PNG)));

            Assert.That(fourthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames.IMAGE_11_HEIC));
            Assert.That(fourthDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC)));
            Assert.That(fourthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames.IMAGE_11_HEIC));
            Assert.That(fourthDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNewFolder2Directory, FileNames.IMAGE_11_HEIC)));

            Assert.That(fifthDuplicatedAssetsSet[0].FileName, Is.EqualTo(FileNames._1336_JPG));
            Assert.That(fifthDuplicatedAssetsSet[0].FullPath,
                Is.EqualTo(Path.Combine(duplicatesNotDuplicateSample1Directory, FileNames._1336_JPG)));
            Assert.That(fifthDuplicatedAssetsSet[1].FileName, Is.EqualTo(FileNames._1336_ORIGINAL_JPG));
            Assert.That(fifthDuplicatedAssetsSet[1].FullPath,
                Is.EqualTo(Path.Combine(duplicatesPartDirectory, FileNames._1336_ORIGINAL_JPG)));
            Assert.That(fifthDuplicatedAssetsSet[2].FileName, Is.EqualTo(FileNames._1336_4_K_ORIGINAL_JPG));
            Assert.That(fifthDuplicatedAssetsSet[2].FullPath,
                Is.EqualTo(Path.Combine(duplicatesResolutionDirectory, FileNames._1336_4_K_ORIGINAL_JPG)));
            Assert.That(fifthDuplicatedAssetsSet[3].FileName, Is.EqualTo(FileNames.IMAGE_1336_ORIGINAL_JPG));
            Assert.That(fifthDuplicatedAssetsSet[3].FullPath,
                Is.EqualTo(Path.Combine(duplicatesThumbnailDirectory, FileNames.IMAGE_1336_ORIGINAL_JPG)));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatedAssetsFound_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Not.Empty);
            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));

            List<Asset> firstDuplicatedAssetsSet = duplicatedAssets[0];
            List<Asset> secondDuplicatedAssetsSet = duplicatedAssets[1];

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_MultiplesAssetsSameHash_ReturnsListOfDuplicatedSets()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            const string hash = Hashes.IMAGE_9_PNG;
            string sourcePath = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

            Folder folder = new() { Id = Guid.NewGuid(), Path = sourcePath };

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset2!, assetData);
            _assetRepository.AddAsset(_asset3!, assetData);
            _assetRepository.AddAsset(_asset4!, assetData);
            _assetRepository.AddAsset(_asset5!, assetData);

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
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesButOneFileDoesNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER_2);

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1);
            _asset3 = _asset3!.WithFolder(folder2);

            byte[] assetData1 = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset3!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_FilesDoNotExist_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
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

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);
            _assetRepository.AddAsset(_asset3!, assetData1);
            _assetRepository.AddAsset(_asset4!, assetData2);
            _assetRepository.AddAsset(_asset5!, assetData1);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatedAssets_ReturnsEmptyList()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);

            Folder folder = new() { Id = Guid.NewGuid(), Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            _assetRepository!.AddAsset(_asset1!, assetData1);
            _assetRepository.AddAsset(_asset2!, assetData2);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
