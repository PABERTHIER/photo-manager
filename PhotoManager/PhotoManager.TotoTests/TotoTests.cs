namespace PhotoManager.TotoTests;

[TestFixture]
public class TotoTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private CatalogAssetsService? _catalogAssetsService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset2Temp;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _blobStorage = new();
        _database = new (new ObjectListStorage(), _blobStorage, new BackupStorage());
    }

    [SetUp]
    public void Setup()
    {
        _asset2Temp = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2024, 06, 07, 08, 54, 37),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
    }

    private void ConfigureCatalogAssetService(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        _catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
    }

    // UPDATE SECTION (Start) -------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task TotoTests_CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndOneCorruptedImageIsUpdated_SyncTheAssetsAndRemovesTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"TempAssetsDirectory_{analyseVideos}"); // Issues in the CI to corrupt image if same directory for each case
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, $"Image 1_Temp_{analyseVideos}.jpg");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository!.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath1.Count);

            List<Asset> assetsFromRepository1 = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository1.Count);

            // Second sync

            _asset2Temp!.ThumbnailCreationDateTime = DateTime.Now; // Because recreated with CreateInvalidImage()
            File.SetLastWriteTime(imagePath1ToCopy, DateTime.Now.AddDays(10));

            // Corrupt image
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelper.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository!.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            List<Asset> assetsFromRepository2 = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository2.Count);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath1ToCopyTemp))
            {
                File.Delete(imagePath1ToCopyTemp);
            }
        }
    }
    // UPDATE SECTION (End) -------------------------------------------------------------------------------------------
}
