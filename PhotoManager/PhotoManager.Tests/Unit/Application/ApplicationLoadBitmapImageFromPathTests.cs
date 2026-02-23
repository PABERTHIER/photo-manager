using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationLoadBitmapImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
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

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        AssetRepository assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(assetRepository, fileOperationsService, userConfigurationService);
        _application = new(assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation, int expectedWith,
        int expectedHeight)
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

            BitmapImage image = _application!.LoadBitmapImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWith));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWith));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.NON_EXISTENT_IMAGE_JPG);
            const Rotation rotation = Rotation.Rotate90;

            BitmapImage image = _application!.LoadBitmapImageFromPath(filePath, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapImageFromPath_FilePathIsNull_ReturnsDefaultBitmapImage()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string? filePath = null;
            const Rotation rotation = Rotation.Rotate90;

            BitmapImage image = _application!.LoadBitmapImageFromPath(filePath!, rotation);

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            const Rotation rotation = (Rotation)999;

            ArgumentException? exception =
                Assert.Throws<ArgumentException>(() => _application!.LoadBitmapImageFromPath(filePath, rotation));

            Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsBitmapImage()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            string filePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
            const Rotation rotation = Rotation.Rotate0;

            BitmapImage image = _application!.LoadBitmapImageFromPath(filePath, rotation);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(image, Is.Not.Null);
                Assert.That(image.StreamSource, Is.Null);
                Assert.That(image.Rotation, Is.EqualTo(rotation));
                Assert.That(image.Width,
                    Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
                Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
                Assert.That(image.PixelWidth, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
                Assert.That(image.PixelHeight, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
                Assert.That(image.DecodePixelWidth, Is.Zero);
                Assert.That(image.DecodePixelHeight, Is.Zero);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
