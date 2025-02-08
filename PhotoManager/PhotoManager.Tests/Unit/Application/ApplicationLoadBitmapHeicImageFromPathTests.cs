namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationLoadBitmapHeicImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    private void ConfigureApplication(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        AssetRepository assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (assetRepository, storageService, userConfigurationService);
        _application = new (assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
    }

    [Test]
    [TestCase(Rotation.Rotate0, 3024, 4032)]
    [TestCase(Rotation.Rotate90, 3024, 4032)]
    [TestCase(Rotation.Rotate180, 3024, 4032)]
    [TestCase(Rotation.Rotate270, 3024, 4032)]
    // [TestCase(null, 3024, 4032)]
    public void LoadBitmapHeicImageFromPath_ValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int expectedWidth, int expectedHeight)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");

            BitmapImage image = _application!.LoadBitmapHeicImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("Image_11_90.heic", Rotation.Rotate90, 4032, 3024)]
    [TestCase("Image_11_180.heic", Rotation.Rotate180, 3024, 4032)]
    [TestCase("Image_11_270.heic", Rotation.Rotate270, 4032, 3024)]
    public void LoadBitmapHeicImageFromPath_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName, Rotation rotation, int expectedWidth, int expectedHeight)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, fileName);

            BitmapImage image = _application!.LoadBitmapHeicImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapHeicImageFromPath_FilePathIsNull_ReturnsBitmapImage()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string? filePath = null;
            const Rotation rotation = Rotation.Rotate90;

            BitmapImage image = _application!.LoadBitmapHeicImageFromPath(filePath!, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapHeicImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, "ImageDoesNotExist.heic");
            const Rotation rotation = Rotation.Rotate90;

            BitmapImage image = _application!.LoadBitmapHeicImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapHeicImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, "Image_11.heic");
            const Rotation rotation = (Rotation)999;

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _application!.LoadBitmapHeicImageFromPath(filePath, rotation));

            Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
