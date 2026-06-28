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
public class ApplicationGetDuplicatedAssetsThumbnailThumbnailTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    private const string ORIGINAL_ASSET_HASH = Hashes.IMAGE_1336_ORIGINAL_JPG;
    private const string MINI_ASSET_HASH = Hashes.IMAGE_1336_MINI_JPG;
    private const string SHIT_QUALITY_ASSET_HASH = Hashes.IMAGE_1336_SHIT_QUALITY_JPG;
    private const string SMALL_ASSET_HASH = Hashes.IMAGE_1336_SMALL_JPG;

    private const string ORIGINAL_ASSET_MD5_HASH = MD5Hashes.IMAGE_1336_ORIGINAL_JPG;
    private const string MINI_ASSET_MD5_HASH = MD5Hashes.IMAGE_1336_MINI_JPG;
    private const string SHIT_QUALITY_ASSET_MD5_HASH = MD5Hashes.IMAGE_1336_SHIT_QUALITY_JPG;
    private const string SMALL_ASSET_MD5_HASH = MD5Hashes.IMAGE_1336_SMALL_JPG;

    private const string ORIGINAL_ASSET_D_HASH = DHashes.IMAGE_1336_ORIGINAL_JPG;
    private const string MINI_ASSET_D_HASH = DHashes.IMAGE_1336_MINI_JPG;
    private const string SHIT_QUALITY_ASSET_D_HASH = DHashes.IMAGE_1336_SHIT_QUALITY_JPG;
    private const string SMALL_ASSET_D_HASH = DHashes.IMAGE_1336_SMALL_JPG;

    private const string ORIGINAL_ASSET_P_HASH = PHashes.IMAGE_1336_ORIGINAL_JPG;
    private const string MINI_ASSET_P_HASH = PHashes.IMAGE_1336_MINI_JPG;
    private const string SHIT_QUALITY_ASSET_P_HASH = PHashes.IMAGE_1336_SHIT_QUALITY_JPG;
    private const string SMALL_ASSET_P_HASH = PHashes.IMAGE_1336_SMALL_JPG;

    private const string MISC_ASSET_HASH = Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_MD5_HASH = MD5Hashes.IMAGE_1_JPG;
    private const string MISC_ASSET_D_HASH = DHashes.IMAGE_1_JPG;
    private const string MISC_ASSET_P_HASH = PHashes.IMAGE_1_JPG;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("ff140210-e4db-4c2c-96c1-85faed197aa7"))
            .WithFileName(FileNames.IMAGE_1336_MINI_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1336_MINI_JPG, PixelHeightAsset.IMAGE_1336_MINI_JPG,
                ThumbnailWidthAsset.IMAGE_1336_MINI_JPG, ThumbnailHeightAsset.IMAGE_1336_MINI_JPG)
            .WithFileProperties(FileSize.IMAGE_1336_MINI_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("ff140210-e4db-4c2c-96c1-85faed197aa7"))
            .WithFileName(FileNames.IMAGE_1336_ORIGINAL_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG, PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG,
                ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG)
            .WithFileProperties(FileSize.IMAGE_1336_ORIGINAL_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("ff140210-e4db-4c2c-96c1-85faed197aa7"))
            .WithFileName(FileNames.IMAGE_1336_SHIT_QUALITY_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, PixelHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG,
                ThumbnailWidthAsset.IMAGE_1336_SHIT_QUALITY_JPG, ThumbnailHeightAsset.IMAGE_1336_SHIT_QUALITY_JPG)
            .WithFileProperties(FileSize.IMAGE_1336_SHIT_QUALITY_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("ff140210-e4db-4c2c-96c1-85faed197aa7"))
            .WithFileName(FileNames.IMAGE_1336_SMALL_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1336_SMALL_JPG, PixelHeightAsset.IMAGE_1336_SMALL_JPG,
                ThumbnailWidthAsset.IMAGE_1336_SMALL_JPG, ThumbnailHeightAsset.IMAGE_1336_SMALL_JPG)
            .WithFileProperties(FileSize.IMAGE_1336_SMALL_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create() // Another asset completely different from the actual asset
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFolderId(new("5b506904-45af-4eaf-b402-53f6d0012075"))
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash("") // Set in each test
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, int pHashThreshold,
        bool detectThumbnails)
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.PHASH_THRESHOLD, pHashThreshold.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.DETECT_THUMBNAILS, detectThumbnails.ToString());

        _userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, _userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            _userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            _userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
    }

    // The hamming distance is about 120 between these hashes
    [Test]
    [Category("Thumbnail folder, basic hashing method")] // SHA-512 generates a 128-character long hash in hexadecimal representation
    [TestCase(20, 0, new string[] { })]
    [TestCase(40, 0, new string[] { })]
    [TestCase(60, 0, new string[] { })]
    [TestCase(80, 0, new string[] { })]
    [TestCase(100, 0, new string[] { })]
    [TestCase(110, 0, new string[] { })]
    [TestCase(128, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailBasicHashDifferentThresholdValues(int thresholdToMock,
        int expected, string[] assetsName)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.THUMBNAIL);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_HASH);
        _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_HASH);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_HASH);
        // If this asset is in the set, then the threshold is not good
        _asset5 = _asset5!.WithFolder(folder2).WithHash(MISC_ASSET_HASH);

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsName));
        }
    }

    // The hamming distance is about 30 between these hashes
    [Test]
    [Category("Thumbnail folder, MD5Hash")] // The MD5Hash is a 32-character hexadecimal string
    [TestCase(5, 0, new string[] { })]
    [TestCase(10, 0, new string[] { })]
    [TestCase(15, 0, new string[] { })]
    [TestCase(20, 0, new string[] { })]
    [TestCase(25, 0, new string[] { })]
    [TestCase(32, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailMD5HashDifferentThresholdValues(int thresholdToMock,
        int expected, string[] assetsName)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.THUMBNAIL);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_MD5_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_MD5_HASH);
        _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_MD5_HASH);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_MD5_HASH);
        _asset5 = _asset5!.WithFolder(folder2)
            .WithHash(MISC_ASSET_MD5_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsName));
        }
    }

    [Test]
    [Category("Thumbnail folder, DHash")] // The DHash is a 14-hex digits
    [TestCase(3, 1, new[] { FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileNames.IMAGE_1336_SMALL_JPG })]
    [TestCase(5, 1, new[] { FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileNames.IMAGE_1336_SMALL_JPG })]
    [TestCase(9, 1,
        new[]
        {
            FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileNames.IMAGE_1336_SMALL_JPG
        })]
    [TestCase(11, 1,
        new[]
        {
            FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileNames.IMAGE_1336_SMALL_JPG
        })]
    [TestCase(14, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailDHashDifferentThresholdValues(int thresholdToMock,
        int expected, string[] assetsName)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.THUMBNAIL);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_D_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_D_HASH);
        _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_D_HASH);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_D_HASH);
        _asset5 = _asset5!.WithFolder(folder2)
            .WithHash(MISC_ASSET_D_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsName));
        }
    }

    // The hamming distance is about 10/74 between these hashes, except for the last picture which is a completely different one
    [Test]
    [Category("Thumbnail folder, PHash")] // The PHash is a 210-character hexadecimal string
    [TestCase(10, 0, new string[] { }, new string[] { })]
    [TestCase(20, 0, new string[] { }, new string[] { })]
    [TestCase(30, 1,
        new[] { FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG, FileNames.IMAGE_1336_SMALL_JPG },
        new string[] { })]
    [TestCase(40, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG
        }, new string[] { })]
    [TestCase(50, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG
        }, new string[] { })]
    [TestCase(60, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG
        }, new string[] { })]
    [TestCase(80, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(90, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(100, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(120, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(140, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(160, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(180, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    [TestCase(210, 1,
        new[]
        {
            FileNames.IMAGE_1336_MINI_JPG, FileNames.IMAGE_1336_ORIGINAL_JPG, FileNames.IMAGE_1336_SHIT_QUALITY_JPG,
            FileNames.IMAGE_1336_SMALL_JPG, FileNames.IMAGE_1_JPG
        }, new string[] { })]
    public void GetDuplicatesBetweenOriginalAndThumbnail_ThumbnailPHashDifferentThresholdValues(int thresholdToMock,
        int expected, string[] assetsName1, string[] assetsName2)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, true, thresholdToMock, true);

        string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.THUMBNAIL);
        string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        Folder folder1 = new() { Id = Guid.NewGuid(), Path = folderPath1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = folderPath2 };

        _asset1 = _asset1!.WithFolder(folder1).WithHash(MINI_ASSET_P_HASH);
        _asset2 = _asset2!.WithFolder(folder1).WithHash(ORIGINAL_ASSET_P_HASH);
        _asset3 = _asset3!.WithFolder(folder1).WithHash(SHIT_QUALITY_ASSET_P_HASH);
        _asset4 = _asset4!.WithFolder(folder1).WithHash(SMALL_ASSET_P_HASH);
        _asset5 = _asset5!.WithFolder(folder2)
            .WithHash(MISC_ASSET_P_HASH); // If this asset is in the set, then the threshold is not good

        byte[] assetData = [1, 2, 3];

        _testableAssetRepository!.AddAsset(_asset1, assetData);
        _testableAssetRepository.AddAsset(_asset2, assetData);
        _testableAssetRepository.AddAsset(_asset3, assetData);
        _testableAssetRepository.AddAsset(_asset4, assetData);
        _testableAssetRepository.AddAsset(_asset5, assetData);

        List<List<Asset>> duplicatedAssets = _application!.GetDuplicatedAssets();

        Assert.That(duplicatedAssets, Has.Count.EqualTo(expected));

        if (expected > 0)
        {
            Assert.That(duplicatedAssets[0].Select(y => y.FileName), Is.EquivalentTo(assetsName1));
        }
        if (expected > 1)
        {
            Assert.That(duplicatedAssets[1].Select(y => y.FileName), Is.EquivalentTo(assetsName2));
        }
    }
}
