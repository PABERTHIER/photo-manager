using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;

namespace PhotoManager.Tests.Unit.Application;

[TestFixture]
public class ApplicationLoadBitmapHeicImageFromPathTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
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

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        _testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext,
            new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService,
            new ImageMagickThumbnailGenerator(imageProcessingService),
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator,
            new CatalogFolderPipeline(fileOperationsService, assetCreationService,
                _testableAssetRepository),
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
    [TestCase(ImageRotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate90, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(ImageRotation.Rotate270, PixelHeightAsset.IMAGE_11_HEIC, PixelWidthAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapImageFromPath_HeicValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);

        using IImageData image = _application!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
    }

    [Test]
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, ImageRotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, ImageRotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, ImageRotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC,
        PixelHeightAsset.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapImageFromPath_HeicValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(string fileName,
        ImageRotation rotation, int expectedWidth, int expectedHeight)
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        string filePath = Path.Combine(_assetsDirectory!, fileName);

        using IImageData image = _application!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(expectedWidth));
        Assert.That(image.Height, Is.EqualTo(expectedHeight));
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicFilePathIsNull_ReturnsBitmapImage()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        string? filePath = null;
        const ImageRotation rotation = ImageRotation.Rotate90;

        using IImageData image = _application!.LoadBitmapImageFromPath(filePath!, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicImageDoesNotExist_ReturnsDefaultBitmapImage()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        string filePath = Path.Combine(_assetsDirectory!, FileNames.NON_EXISTENT_IMAGE_HEIC);
        const ImageRotation rotation = ImageRotation.Rotate90;

        using IImageData image = _application!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(1));
        Assert.That(image.Height, Is.EqualTo(1));
    }

    [Test]
    public void LoadBitmapImageFromPath_HeicInvalidRotation_ReturnsBitmapImageWithRotate0()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        string filePath = Path.Combine(_assetsDirectory!, FileNames.IMAGE_11_HEIC);
        const ImageRotation rotation = (ImageRotation)999;

        using IImageData image = _application!.LoadBitmapImageFromPath(filePath, rotation);

        Assert.That(image, Is.Not.Null);
        Assert.That(image.Rotation, Is.EqualTo(ImageRotation.Rotate0));
        Assert.That(image.Width, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
        Assert.That(image.Height, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
    }
}
