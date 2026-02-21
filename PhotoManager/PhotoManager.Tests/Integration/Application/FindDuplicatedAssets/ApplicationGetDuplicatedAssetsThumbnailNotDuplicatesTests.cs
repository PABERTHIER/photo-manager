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
public class ApplicationGetDuplicatedAssetsThumbnailNotDuplicatesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;

    private const string ASSET1336_HASH = Hashes._1336_JPG;
    private const string ASSET1337_HASH = Hashes._1337_JPG;
    private const string ASSET1349_HASH = Hashes._1349_JPG;
    private const string ASSET1350_HASH = Hashes._1350_JPG;
    private const string ASSET1413_HASH = Hashes._1413_JPG;
    private const string ASSET1414_HASH = Hashes._1414_JPG;
    private const string ASSET1415_HASH = Hashes._1415_JPG;

    private const string ASSET1336_MD5_HASH = MD5Hashes._1336_JPG;
    private const string ASSET1337_MD5_HASH = MD5Hashes._1337_JPG;
    private const string ASSET1349_MD5_HASH = MD5Hashes._1349_JPG;
    private const string ASSET1350_MD5_HASH = MD5Hashes._1350_JPG;
    private const string ASSET1413_MD5_HASH = MD5Hashes._1413_JPG;
    private const string ASSET1414_MD5_HASH = MD5Hashes._1414_JPG;
    private const string ASSET1415_MD5_HASH = MD5Hashes._1415_JPG;

    private const string ASSET1336_D_HASH = DHashes._1336_JPG;
    private const string ASSET1337_D_HASH = DHashes._1337_JPG;
    private const string ASSET1349_D_HASH = DHashes._1349_JPG;
    private const string ASSET1350_D_HASH = DHashes._1350_JPG;
    private const string ASSET1413_D_HASH = DHashes._1413_JPG;
    private const string ASSET1414_D_HASH = DHashes._1414_JPG;
    private const string ASSET1415_D_HASH = DHashes._1415_JPG;

    private const string ASSET1336_P_HASH = PHashes._1336_JPG;
    private const string ASSET1337_P_HASH = PHashes._1337_JPG;
    private const string ASSET1349_P_HASH = PHashes._1349_JPG;
    private const string ASSET1350_P_HASH = PHashes._1350_JPG;
    private const string ASSET1413_P_HASH = PHashes._1413_JPG;
    private const string ASSET1414_P_HASH = PHashes._1414_JPG;
    private const string ASSET1415_P_HASH = PHashes._1415_JPG;

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
            FolderId = new Guid("db973547-ed18-4e7d-8dc8-ce441aabef80"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_JPG, Height = PixelHeightAsset._1336_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_JPG, Height = ThumbnailHeightAsset._1336_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_JPG,
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
            FolderId = new Guid("65a65540-586e-4b2e-a62a-ccdbd7d442f2"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1337_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1337_JPG, Height = PixelHeightAsset._1337_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1337_JPG, Height = ThumbnailHeightAsset._1337_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1337_JPG,
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
            FolderId = new Guid("65ba6b40-dc3c-46b6-8508-135cb4e1f961"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1349_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1349_JPG, Height = PixelHeightAsset._1349_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1349_JPG, Height = ThumbnailHeightAsset._1349_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1349_JPG,
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
            FolderId = new Guid("ee3dcc63-53d8-4937-a38a-45b3189118e6"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1350_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1350_JPG, Height = PixelHeightAsset._1350_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1350_JPG, Height = ThumbnailHeightAsset._1350_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1350_JPG,
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
            FolderId = new Guid("044e3039-0b4b-4065-b215-9cf185d1f90f"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1413_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1413_JPG, Height = PixelHeightAsset._1413_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1413_JPG, Height = ThumbnailHeightAsset._1413_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1413_JPG,
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
            FolderId = new Guid("80fa1201-1b9d-4d8e-8730-6efe8e46fcbd"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1414_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1414_JPG, Height = PixelHeightAsset._1414_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1414_JPG, Height = ThumbnailHeightAsset._1414_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1414_JPG,
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
            FolderId = new Guid("4c0b2c7c-3159-4855-850b-27c7e737a96e"),
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1415_JPG,
            ImageRotation = Rotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1415_JPG, Height = PixelHeightAsset._1415_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1415_JPG, Height = ThumbnailHeightAsset._1415_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1415_JPG,
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
        _asset8 = new() // Another asset completely different from the actual asset
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

        _userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(_database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, _userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService);
    }

    // The hamming distance is about 121 between these hashes
    [Test]
    [Category("NotDuplicate folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase(20, 0, new string[] { })]
    [TestCase(40, 0, new string[] { })]
    [TestCase(60, 0, new string[] { })]
    [TestCase(80, 0, new string[] { })]
    [TestCase(100, 0, new string[] { })]
    [TestCase(110, 0, new string[] { })]
    [TestCase(128, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateBasicHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}");
            string folderPath3 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}");
            string folderPath4 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_HASH);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_HASH);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_HASH);
            _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_HASH);
            _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_HASH);
            _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_HASH);
            _asset8 = _asset8!.WithFolder(folder4).WithHash(MISC_ASSET_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [.. assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 26/32 between these hashes
    [Test]
    [Category("NotDuplicate folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase(5, 0, new string[] { })]
    [TestCase(10, 0, new string[] { })]
    [TestCase(15, 0, new string[] { })]
    [TestCase(20, 0, new string[] { })]
    [TestCase(25, 0, new string[] { })]
    [TestCase(32, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateMD5HashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}");
            string folderPath3 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}");
            string folderPath4 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_MD5_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_MD5_HASH);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_MD5_HASH);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_MD5_HASH);
            _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_MD5_HASH);
            _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_MD5_HASH);
            _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_MD5_HASH);
            _asset8 = _asset8!.WithFolder(folder4).WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [.. assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase(3, 0, new string[] { })]
    [TestCase(5, 0, new string[] { })]
    [TestCase(9, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG })]
    [TestCase(11, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG })]
    [TestCase(14, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample1DHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}");
            string folderPath4 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_D_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_D_HASH);
            _asset8 = _asset8!.WithFolder(folder4).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [.. assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase(3, 0, new string[] { })]
    [TestCase(5, 0, new string[] { })]
    [TestCase(9, 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG })]
    [TestCase(11, 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG })]
    [TestCase(14, 1, new[] { FileNames._1349_JPG, FileNames._1350_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample2DHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset3 = _asset3!.WithFolder(folder1).WithHash(ASSET1349_D_HASH);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(ASSET1350_D_HASH);
            _asset8 = _asset8!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [.. assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [Category("NotDuplicate folder, DHash")] // The DHash is a 14-hex digits
    [TestCase(3, 0, new string[] { })]
    [TestCase(5, 0, new string[] { })]
    [TestCase(9, 0, new string[] { })]
    [TestCase(11, 0, new string[] { })]
    [TestCase(14, 1, new[] { FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicateSample3DHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

            _asset5 = _asset5!.WithFolder(folder1).WithHash(ASSET1413_D_HASH);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(ASSET1414_D_HASH);
            _asset7 = _asset7!.WithFolder(folder1).WithHash(ASSET1415_D_HASH);
            _asset8 = _asset8!.WithFolder(folder2).WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList = [.. assetsName];
                Assert.That(assetsNameList.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // The hamming distance is about 10/78 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("NotDuplicate folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase(10, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(20, 0, new string[] { }, new string[] { }, new string[] { })]
    [TestCase(30, 1, new[] { FileNames._1413_JPG, FileNames._1414_JPG }, new string[] { }, new string[] { })]
    [TestCase(40, 3, new[] { FileNames._1336_JPG, FileNames._1350_JPG }, new[] { FileNames._1337_JPG, FileNames._1350_JPG }, new[] { FileNames._1413_JPG, FileNames._1414_JPG })]
    [TestCase(50, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG }, new string[] { }, new string[] { })]
    [TestCase(60, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG }, new string[] { }, new string[] { })]
    [TestCase(80, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(90, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(100, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(120, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(140, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(160, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(180, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    [TestCase(210, 1, new[] { FileNames._1336_JPG, FileNames._1337_JPG, FileNames._1349_JPG, FileNames._1350_JPG, FileNames._1413_JPG, FileNames._1414_JPG, FileNames._1415_JPG, FileNames.IMAGE_1_JPG }, new string[] { }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_NotDuplicatePHashDifferentThresholdValues(int thresholdToMock, int expected, string[] assetsName1, string[] assetsName2, string[] assetsName3)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_1}");
            string folderPath2 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_2}");
            string folderPath3 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NOT_DUPLICATE}\\{Directories.SAMPLE_3}");
            string folderPath4 = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

            Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
            Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };
            Folder folder3 = new() { Id = Guid.NewGuid(), Path = folderPath3 };
            Folder folder4 = new() { Id = Guid.NewGuid(), Path = folderPath4 };

            _asset1 = _asset1!.WithFolder(folder1).WithHash(ASSET1336_P_HASH);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(ASSET1337_P_HASH);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(ASSET1349_P_HASH);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(ASSET1350_P_HASH);
            _asset5 = _asset5!.WithFolder(folder3).WithHash(ASSET1413_P_HASH);
            _asset6 = _asset6!.WithFolder(folder3).WithHash(ASSET1414_P_HASH);
            _asset7 = _asset7!.WithFolder(folder3).WithHash(ASSET1415_P_HASH);
            _asset8 = _asset8!.WithFolder(folder4).WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);
            _assetRepository.AddAsset(_asset6, assetData);
            _assetRepository.AddAsset(_asset7, assetData);
            _assetRepository.AddAsset(_asset8, assetData);

            List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

            if (expected > 0)
            {
                IList<string> assetsNameList1 = [.. assetsName1];
                Assert.That(assetsNameList1.SequenceEqual(duplicatedAssets[0].Select(y => y.FileName)), Is.True);
            }
            if (expected > 1)
            {
                IList<string> assetsNameList2 = [.. assetsName2];
                Assert.That(assetsNameList2.SequenceEqual(duplicatedAssets[1].Select(y => y.FileName)), Is.True);
            }
            if (expected > 2)
            {
                IList<string> assetsNameList3 = [.. assetsName3];
                Assert.That(assetsNameList3.SequenceEqual(duplicatedAssets[2].Select(y => y.FileName)), Is.True);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
