using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using DHashes = PhotoManager.Tests.Integration.Constants.DHashes;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using PHashes = PhotoManager.Tests.Integration.Constants.PHashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Integration.Application.FindDuplicatedAssets;

[TestFixture]
public class ApplicationGetDuplicatedAssetsThumbnailPartTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;
    private Asset? _asset9;
    private Asset? _asset10;

    private const string ASSET1336_BOTTOM_LEFT_PART_HASH = Hashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_HASH = Hashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_HASH = Hashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_HASH = Hashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_HASH = Hashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_HASH = Hashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_HASH = Hashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_HASH = Hashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_HASH = Hashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_MD5_HASH = MD5Hashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_MD5_HASH = MD5Hashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_MD5_HASH = MD5Hashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_MD5_HASH = MD5Hashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_MD5_HASH = MD5Hashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_MD5_HASH = MD5Hashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_MD5_HASH = MD5Hashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_D_HASH = DHashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_D_HASH = DHashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_D_HASH = DHashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_D_HASH = DHashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_D_HASH = DHashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_D_HASH = DHashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_D_HASH = DHashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_D_HASH = DHashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_D_HASH = DHashes._1336_TOP_RIGHT_PART_JPG;

    private const string ASSET1336_BOTTOM_LEFT_PART_P_HASH = PHashes._1336_BOTTOM_LEFT_PART_JPG;
    private const string ASSET1336_BOTTOM_PART_P_HASH = PHashes._1336_BOTTOM_PART_JPG;
    private const string ASSET1336_BOTTOM_RIGHT_PART_P_HASH = PHashes._1336_BOTTOM_RIGHT_PART_JPG;
    private const string ASSET1336_LEFT_PART_P_HASH = PHashes._1336_LEFT_PART_JPG;
    private const string ASSET1336_ORIGINAL_P_HASH = PHashes._1336_ORIGINAL_JPG;
    private const string ASSET1336_RIGHT_PART_P_HASH = PHashes._1336_RIGHT_PART_JPG;
    private const string ASSET1336_TOP_LEFT_PART_P_HASH = PHashes._1336_TOP_LEFT_PART_JPG;
    private const string ASSET1336_TOP_PART_P_HASH = PHashes._1336_TOP_PART_JPG;
    private const string ASSET1336_TOP_RIGHT_PART_P_HASH = PHashes._1336_TOP_RIGHT_PART_JPG;

    private const string MISC_ASSET_HASH = Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_MD5_HASH = MD5Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_D_HASH = DHashes.IMAGE_1_JPG;
    private const string MISC_ASSET_P_HASH = PHashes.IMAGE_1_JPG;

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
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_BOTTOM_LEFT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_BOTTOM_LEFT_PART_JPG, Height = PixelHeightAsset._1336_BOTTOM_LEFT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_BOTTOM_LEFT_PART_JPG, Height = ThumbnailHeightAsset._1336_BOTTOM_LEFT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_BOTTOM_LEFT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_BOTTOM_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_BOTTOM_PART_JPG, Height = PixelHeightAsset._1336_BOTTOM_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_BOTTOM_PART_JPG, Height = ThumbnailHeightAsset._1336_BOTTOM_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_BOTTOM_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_BOTTOM_RIGHT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, Height = PixelHeightAsset._1336_BOTTOM_RIGHT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_BOTTOM_RIGHT_PART_JPG, Height = ThumbnailHeightAsset._1336_BOTTOM_RIGHT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_BOTTOM_RIGHT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_LEFT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_LEFT_PART_JPG, Height = PixelHeightAsset._1336_LEFT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_LEFT_PART_JPG, Height = ThumbnailHeightAsset._1336_LEFT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_LEFT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_ORIGINAL_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_ORIGINAL_JPG, Height = PixelHeightAsset._1336_ORIGINAL_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_ORIGINAL_JPG, Height = ThumbnailHeightAsset._1336_ORIGINAL_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_ORIGINAL_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset6 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_RIGHT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_RIGHT_PART_JPG, Height = PixelHeightAsset._1336_RIGHT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_RIGHT_PART_JPG, Height = ThumbnailHeightAsset._1336_RIGHT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_RIGHT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset7 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_TOP_LEFT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_TOP_LEFT_PART_JPG, Height = PixelHeightAsset._1336_TOP_LEFT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_TOP_LEFT_PART_JPG, Height = ThumbnailHeightAsset._1336_TOP_LEFT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_TOP_LEFT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset8 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_TOP_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_TOP_PART_JPG, Height = PixelHeightAsset._1336_TOP_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_TOP_PART_JPG, Height = ThumbnailHeightAsset._1336_TOP_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_TOP_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset9 = new()
        {
            FolderId = new Guid("5126101a-c970-4c87-92ee-ab415ff659f3"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_TOP_RIGHT_PART_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_TOP_RIGHT_PART_JPG, Height = PixelHeightAsset._1336_TOP_RIGHT_PART_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_TOP_RIGHT_PART_JPG, Height = ThumbnailHeightAsset._1336_TOP_RIGHT_PART_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_TOP_RIGHT_PART_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset10 = new() // Another asset completely different from the actual asset
        {
            FolderId = new Guid("5b506904-45af-4eaf-b402-53f6d0012075"),
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
            Hash = "", // Set in each test
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, int pHashThreshold, bool detectThumbnails)
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, pHashThreshold.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, detectThumbnails.ToString());

        _userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, _userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
    }

    // The hamming distance is about 113/124 between these hashes
    [Test]
    [Category("Part folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase(20, 0, new string[] { })]
    [TestCase(40, 0, new string[] { })]
    [TestCase(60, 0, new string[] { })]
    [TestCase(80, 0, new string[] { })]
    [TestCase(100, 0, new string[] { })]
    [TestCase(110, 0, new string[] { })]
    [TestCase(128, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartBasicHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);
            _assetRepository.AddAsset(_asset9, assetData);
            _assetRepository.AddAsset(_asset10, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 30 between these hashes
    [Test]
    [Category("Part folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase(5, 0, new string[] { })]
    [TestCase(10, 0, new string[] { })]
    [TestCase(15, 0, new string[] { })]
    [TestCase(20, 0, new string[] { })]
    [TestCase(25, 0, new string[] { })]
    [TestCase(32, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartMD5HashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_MD5_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_MD5_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_MD5_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_MD5_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_MD5_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_MD5_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_MD5_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_MD5_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_MD5_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);
            _assetRepository.AddAsset(_asset9, assetData);
            _assetRepository.AddAsset(_asset10, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [..assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 14 between these hashes (for some, 0)
    [Test]
    [Category("Part folder, DHash")] // The DHash is a 17-character number
    [TestCase(3, 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase(5, 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase(9, 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase(11, 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG })]
    [TestCase(14, 2, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG }, new[] { FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG }, new string[] { })]
    [TestCase(17, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartDHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            // TODO: Let see if this test is failing on GitHub CI (Win 10) while it is failing in local (Win 11)
            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_D_HASH); // 17
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_D_HASH); // 17
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_D_HASH); // 17
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_D_HASH); // 16
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_D_HASH); // 10
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_D_HASH); // 16
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_D_HASH); // 16
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_D_HASH); // 16
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_D_HASH); // 16
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good // 17

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);
            _assetRepository.AddAsset(_asset9, assetData);
            _assetRepository.AddAsset(_asset10, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList1 = [..assetsName1];
                Assert.That(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = [..assetsName2];
                Assert.That(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)), Is.True);
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = [..assetsName3];
                Assert.That(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 80/100 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Part folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase(10, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(20, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(30, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(40, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(50, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(60, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(80, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG }, new string[] { }, new string[] { })]
    [TestCase(90, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG }, new string[] { }, new string[] { })]
    [TestCase(100, 3, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG }, new[] { FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_RIGHT_PART_JPG }, new[] { FileNames._1336_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG })]
    [TestCase(120, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(140, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(160, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(180, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(210, 1, new[] { FileNames._1336_BOTTOM_LEFT_PART_JPG, FileNames._1336_BOTTOM_PART_JPG, FileNames._1336_BOTTOM_RIGHT_PART_JPG, FileNames._1336_LEFT_PART_JPG, FileNames._1336_ORIGINAL_JPG, FileNames._1336_RIGHT_PART_JPG, FileNames._1336_TOP_LEFT_PART_JPG, FileNames._1336_TOP_PART_JPG, FileNames._1336_TOP_RIGHT_PART_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_PartPHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.PART}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_LEFT_PART_P_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_PART_P_HASH);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1336_BOTTOM_RIGHT_PART_P_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1336_LEFT_PART_P_HASH);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1336_ORIGINAL_P_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1336_RIGHT_PART_P_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1336_TOP_LEFT_PART_P_HASH);
            _asset8 = _asset8!.WithFolder(folder1).WithHash(ASSET1336_TOP_PART_P_HASH);
            _asset9 = _asset9!.WithFolder(folder1).WithHash(ASSET1336_TOP_RIGHT_PART_P_HASH);
            _asset10 = _asset10!.WithFolder(folder2).WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);
            _assetRepository.AddAsset(_asset9, assetData);
            _assetRepository.AddAsset(_asset10, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList1 = [..assetsName1];
                Assert.That(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = [..assetsName2];
                Assert.That(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)), Is.True);
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = [..assetsName3];
                Assert.That(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
