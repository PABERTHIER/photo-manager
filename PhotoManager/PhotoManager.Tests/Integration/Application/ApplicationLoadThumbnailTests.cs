using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using Reactive = System.Reactive;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationLoadThumbnailTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;

    private IPathProviderService? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset1Temp;

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
            .WithFileName(FileNames.IMAGE_1_DUPLICATE_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, PixelHeightAsset.IMAGE_1_DUPLICATE_JPG,
                ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG)
            .WithFileProperties(FileSize.IMAGE_1_DUPLICATE_JPG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_1_DUPLICATE_JPG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFileName(FileNames.IMAGE_9_PNG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.IMAGE_9_PNG, PixelHeightAsset.IMAGE_9_PNG,
                ThumbnailWidthAsset.IMAGE_9_PNG, ThumbnailHeightAsset.IMAGE_9_PNG)
            .WithFileProperties(FileSize.IMAGE_9_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFileName(FileNames.IMAGE_9_DUPLICATE_PNG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, PixelHeightAsset.IMAGE_9_DUPLICATE_PNG,
                ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG)
            .WithFileProperties(FileSize.IMAGE_9_DUPLICATE_PNG, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_9_DUPLICATE_PNG)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFileName(FileNames.IMAGE_11_HEIC)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC,
                ThumbnailWidthAsset.IMAGE_11_HEIC, ThumbnailHeightAsset.IMAGE_11_HEIC)
            .WithFileProperties(FileSize.IMAGE_11_HEIC, DateTime.Now, ModificationDate.Default)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.IMAGE_11_HEIC)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset1Temp = AssetBuilder.Create()
            .WithFolder(new() { Id = Guid.Empty, Path = "" }) // Initialised later
            .WithFileName(FileNames.HOMER_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(
                PixelWidthAsset.HOMER_JPG, PixelHeightAsset.HOMER_JPG,
                ThumbnailWidthAsset.HOMER_JPG, ThumbnailHeightAsset.HOMER_JPG)
            .WithFileProperties(FileSize.HOMER_JPG_CURRENT_OS, DateTime.Now, DateTime.Now)
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithHash(Hashes.HOMER_JPG_CURRENT_OS)
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

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(_pathProviderServiceMock.ResolveDatabaseDirectory());
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
        MoveAssetsService moveAssetsService =
            new(_testableAssetRepository, fileOperationsService, assetCreationService,
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

    [Test]
    public async Task LoadThumbnail_CataloguedAssets_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            await _application!.CatalogAssetsAsync(_ => { });

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Folder folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory)!;

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            Assert.That(File.Exists(_asset1!.FullPath), Is.True);
            Assert.That(File.Exists(_asset2!.FullPath), Is.True);
            Assert.That(File.Exists(_asset3!.FullPath), Is.True);
            Assert.That(File.Exists(_asset4!.FullPath), Is.True);

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);
            _application!.LoadThumbnail(_asset2!);
            _application!.LoadThumbnail(_asset3!);
            _application!.LoadThumbnail(_asset4!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public async Task LoadThumbnail_CataloguedAssetFromVideo_SetsBitmapImageToTheAsset()
    {
        string tempDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_FOLDER);

        ConfigureApplication(100, tempDirectory, 200, 150, false, false, false, true);

        string outputFirstFrameDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Directory.CreateDirectory(tempDirectory);

            string videoSourcePath = Path.Combine(_assetsDirectory!, FileNames.HOMER_MP4);
            string videoDestinationPath = Path.Combine(tempDirectory, FileNames.HOMER_MP4);
            File.Copy(videoSourcePath, videoDestinationPath);

            await _application!.CatalogAssetsAsync(_ => { });

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Folder folder = _testableAssetRepository!.GetFolderByPath(outputFirstFrameDirectory)!;

            _asset1Temp = _asset1Temp!.WithFolder(folder);

            Assert.That(File.Exists(_asset1Temp!.FullPath), Is.True);

            Assert.That(_asset1Temp.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1Temp);

            Assert.That(_asset1Temp.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1Temp.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath1 = Path.Combine(assetsDirectory, _asset1!.FileName);
            string filePath2 = Path.Combine(assetsDirectory, _asset2!.FileName);
            string filePath3 = Path.Combine(assetsDirectory, _asset3!.FileName);
            string filePath4 = Path.Combine(assetsDirectory, _asset4!.FileName);

            byte[] assetData1 = File.ReadAllBytes(filePath1);
            byte[] assetData2 = File.ReadAllBytes(filePath2);
            byte[] assetData3 = File.ReadAllBytes(filePath3);
            byte[] assetData4 = File.ReadAllBytes(filePath4);

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData1);
            _testableAssetRepository!.AddAsset(_asset2!, assetData2);
            _testableAssetRepository!.AddAsset(_asset3!, assetData3);
            _testableAssetRepository!.AddAsset(_asset4!, assetData4);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);
            _application!.LoadThumbnail(_asset2!);
            _application!.LoadThumbnail(_asset3!);
            _application!.LoadThumbnail(_asset4!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_DoesNotSetBitmapImageToTheAsset()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath = Path.Combine(assetsDirectory, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            _testableAssetRepository!.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetAndFolderDoNotExist_DoesNotSetBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderPathIsNull_ThrowsArgumentNullException()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = null! });

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
            {
                _application!.LoadThumbnail(_asset1!);
            });

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FileNameIsNull_ThrowsArgumentNullException()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            Asset asset = AssetBuilder.Create()
                .WithFolder(addedFolder)
                .WithFileName(null!)
                .WithRotation(ImageRotation.Rotate0)
                .WithPixels(
                    PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                    ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
                .WithFileProperties(FileSize.IMAGE_1_JPG, DateTime.Now, ModificationDate.Default)
                .WithThumbnailCreationDateTime(DateTime.Now)
                .WithHash(Hashes.IMAGE_1_JPG)
                .WithCorrupted(false, null)
                .WithRotated(false, null)
                .Build();

            Assert.That(assetsUpdatedEvents, Is.Empty);

            ArgumentNullException? exception =
                Assert.Throws<ArgumentNullException>(() => _application!.LoadThumbnail(asset));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_BitmapImageAreSetToEachAssetSafely()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string filePath1 = Path.Combine(assetsDirectory, _asset1!.FileName);
            string filePath2 = Path.Combine(assetsDirectory, _asset2!.FileName);
            string filePath3 = Path.Combine(assetsDirectory, _asset3!.FileName);
            string filePath4 = Path.Combine(assetsDirectory, _asset4!.FileName);

            byte[] assetData1 = File.ReadAllBytes(filePath1);
            byte[] assetData2 = File.ReadAllBytes(filePath2);
            byte[] assetData3 = File.ReadAllBytes(filePath3);
            byte[] assetData4 = File.ReadAllBytes(filePath4);

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            _testableAssetRepository!.AddAsset(_asset1!, assetData1);
            _testableAssetRepository!.AddAsset(_asset2!, assetData2);
            _testableAssetRepository!.AddAsset(_asset3!, assetData3);
            _testableAssetRepository!.AddAsset(_asset4!, assetData4);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);
            Assert.That(_asset2!.ImageData, Is.Null);
            Assert.That(_asset3!.ImageData, Is.Null);
            Assert.That(_asset4!.ImageData, Is.Null);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _application!.LoadThumbnail(_asset4!),
                () => _application!.LoadThumbnail(_asset2!),
                () => _application!.LoadThumbnail(_asset1!),
                () => _application!.LoadThumbnail(_asset3!)
            );

            Assert.That(_asset1!.ImageData, Is.Not.Null);
            Assert.That(_asset2!.ImageData, Is.Not.Null);
            Assert.That(_asset3!.ImageData, Is.Not.Null);
            Assert.That(_asset4!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(4));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
