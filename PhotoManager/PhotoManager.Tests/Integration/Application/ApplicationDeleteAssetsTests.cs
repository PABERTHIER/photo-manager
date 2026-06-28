using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationDeleteAssetsTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private MoveAssetsService? _moveAssetsService;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;

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
        _moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            _moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            _userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, _moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
    }

    [Test]
    public async Task DeleteAssets_AssetsAreValid_DeletesAssetsAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_assetsDirectory!, Directories.DESTINATION_TO_COPY);

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            const string asset1FileName = FileNames.IMAGE_6_JPG;
            const string asset2FileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath1 = Path.Combine(_assetsDirectory!, asset1FileName);
            string destinationFilePath1 = Path.Combine(destinationDirectory, asset1FileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath1, destinationFilePath1);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath1), Is.True);
            Assert.That(File.Exists(destinationFilePath1), Is.True);

            string sourceFilePath2 = Path.Combine(_assetsDirectory!, asset2FileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, asset2FileName);

            hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            await _application!.CatalogAssetsAsync(_ => { });

            Asset[] assetsInRepository = _testableAssetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2FileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset1FileName));

            Folder? sourceFolder = _testableAssetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            _application!.DeleteAssets([assetsInRepository[0], assetsInRepository[1]]);

            assetsInRepository = _testableAssetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Assert.That(File.Exists(destinationFilePath1), Is.False);
            Assert.That(File.Exists(destinationFilePath2), Is.False);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteAssets_AssetIsValid_DeletesAssetAndSavesCatalog()
    {
        string destinationDirectory = Path.Combine(_assetsDirectory!, Directories.DESTINATION_TO_COPY);

        ConfigureApplication(100, destinationDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(destinationDirectory);

            const string assetFileName = FileNames.IMAGE_1_JPG;

            string sourceFilePath2 = Path.Combine(_assetsDirectory!, assetFileName);
            string destinationFilePath2 = Path.Combine(destinationDirectory, assetFileName);

            bool hasBeenCopied = _moveAssetsService!.CopyAsset(sourceFilePath2, destinationFilePath2);

            Assert.That(hasBeenCopied, Is.True);
            Assert.That(File.Exists(sourceFilePath2), Is.True);
            Assert.That(File.Exists(destinationFilePath2), Is.True);

            await _application!.CatalogAssetsAsync(_ => { });

            Asset[] assetsInRepository = _testableAssetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(assetFileName));

            Folder? sourceFolder = _testableAssetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(sourceFolder, Is.Not.Null);

            _application!.DeleteAssets([assetsInRepository[0]]);

            assetsInRepository = _testableAssetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Assert.That(File.Exists(destinationFilePath2), Is.False);
        }
        finally
        {
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public void DeleteAssets_AssetsIsNull_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        Asset[]? assets = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets!));

        Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
        Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
    }

    [Test]
    public void DeleteAssets_AssetsIsEmpty_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        Asset[] assets = [];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

        Assert.That(exception?.Message, Is.EqualTo("assets cannot be null or empty. (Parameter 'assets')"));
        Assert.That(exception?.ParamName, Is.EqualTo(nameof(assets)));
    }

    [Test]
    public void DeleteAssets_OneAssetIsNull_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        Guid folderId1 = Guid.NewGuid();
        Guid folderId2 = Guid.NewGuid();

        Asset asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId1, Path = _assetsDirectory! })
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        Asset? asset2 = null;
        Asset asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId2, Path = _assetsDirectory! })
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        Asset[] assets = [asset1, asset2!, asset3];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

        Assert.That(exception?.Message, Is.EqualTo("asset cannot be null. (Parameter 'asset')"));
        Assert.That(exception?.ParamName, Is.EqualTo("asset"));
    }

    [Test]
    public void DeleteAssets_OneAssetFolderIsNull_ThrowsArgumentNullException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        Guid folderId1 = Guid.NewGuid();
        Guid folderId2 = Guid.NewGuid();
        Folder? folder = null;

        Asset asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId1, Path = _assetsDirectory! })
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        Asset asset2 = AssetBuilder.Create()
            .WithFolder(folder!)
            .WithFolderId(Guid.Empty)
            .WithFileName(FileNames.NON_EXISTENT_FILE_JPG)
            .WithPixels(PixelWidthAsset.NON_EXISTENT_FILE_JPG, PixelHeightAsset.NON_EXISTENT_FILE_JPG,
                ThumbnailWidthAsset.NON_EXISTENT_FILE_JPG, ThumbnailHeightAsset.NON_EXISTENT_FILE_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        Asset asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId2, Path = _assetsDirectory! })
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        Asset[] assets = [asset1, asset2, asset3];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _application!.DeleteAssets(assets));

        Assert.That(exception?.Message, Is.EqualTo("asset.Folder cannot be null. (Parameter 'Folder')"));
        Assert.That(exception?.ParamName, Is.EqualTo(nameof(Folder)));
    }

    [Test]
    public void DeleteAssets_AssetDoesNotExists_ThrowsFileNotFoundException()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        Folder folder = new() { Id = Guid.NewGuid(), Path = _assetsDirectory! };

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(FileNames.NON_EXISTENT_FILE_JPG)
            .WithPixels(PixelWidthAsset.NON_EXISTENT_FILE_JPG, PixelHeightAsset.NON_EXISTENT_FILE_JPG,
                ThumbnailWidthAsset.NON_EXISTENT_FILE_JPG, ThumbnailHeightAsset.NON_EXISTENT_FILE_JPG)
            .WithFileSize(0)
            .WithHash(string.Empty)
            .WithRotation(ImageRotation.Rotate0)
            .WithThumbnailCreationDateTime(default)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();

        Asset[] assets = [asset];

        FileNotFoundException? exception =
            Assert.Throws<FileNotFoundException>(() => _application!.DeleteAssets(assets));

        Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{asset.FullPath}'."));
    }
}
