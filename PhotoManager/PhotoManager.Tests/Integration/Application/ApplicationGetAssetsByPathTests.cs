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
public class ApplicationGetAssetsByPathTests
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
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExists_ReturnsAssetsArray()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
        Assert.That(cataloguedAssets, Is.Empty);

        await _application!.CatalogAssetsAsync(_ => { });

        bool folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
        Assert.That(folderExists, Is.True);

        cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
        Assert.That(cataloguedAssets, Has.Count.EqualTo(4));
        Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1!.FileName));
        Assert.That(cataloguedAssets[0].Hash, Is.EqualTo(_asset1!.Hash));
        Assert.That(cataloguedAssets[0].ImageData, Is.Null);
        Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
        Assert.That(cataloguedAssets[1].Hash, Is.EqualTo(_asset2!.Hash));
        Assert.That(cataloguedAssets[1].ImageData, Is.Null);
        Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
        Assert.That(cataloguedAssets[2].Hash, Is.EqualTo(_asset3!.Hash));
        Assert.That(cataloguedAssets[2].ImageData, Is.Null);
        Assert.That(cataloguedAssets[3].FileName, Is.EqualTo(_asset4!.FileName));
        Assert.That(cataloguedAssets[3].Hash, Is.EqualTo(_asset4!.Hash));
        Assert.That(cataloguedAssets[3].ImageData, Is.Null);

        Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(assetsDirectory);
        Assert.That(assetsInRepository, Has.Length.EqualTo(4));

        Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
        Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
        Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);
        Assert.That(cataloguedAssets[3].ImageData, Is.Not.Null);

        Asset assetsInRepository1 = assetsInRepository.First(x => x.FileName == _asset1!.FileName);
        Assert.That(assetsInRepository1.Hash, Is.EqualTo(_asset1!.Hash));

        Asset assetsInRepository2 = assetsInRepository.First(x => x.FileName == _asset2!.FileName);
        Assert.That(assetsInRepository2.Hash, Is.EqualTo(_asset2!.Hash));

        Asset assetsInRepository3 = assetsInRepository.First(x => x.FileName == _asset3!.FileName);
        Assert.That(assetsInRepository3.Hash, Is.EqualTo(_asset3!.Hash));

        Asset assetsInRepository4 = assetsInRepository.First(x => x.FileName == _asset4!.FileName);
        Assert.That(assetsInRepository4.Hash, Is.EqualTo(_asset4!.Hash));

        Folder folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory)!;

        _asset1 = _asset1!.WithFolder(folder);
        _asset2 = _asset2!.WithFolder(folder);
        _asset3 = _asset3!.WithFolder(folder);
        _asset4 = _asset4!.WithFolder(folder);

        Assert.That(File.Exists(_asset1!.FullPath), Is.True);
        Assert.That(File.Exists(_asset2!.FullPath), Is.True);
        Assert.That(File.Exists(_asset3!.FullPath), Is.True);
        Assert.That(File.Exists(_asset4!.FullPath), Is.True);

        Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

        Assert.That(assets, Has.Length.EqualTo(4));

        Asset asset1 = assets.First(x => x.FileName == _asset1!.FileName);
        Assert.That(asset1.FolderId, Is.EqualTo(_asset1!.FolderId));
        Assert.That(asset1.Folder.Path, Is.EqualTo(_asset1!.Folder.Path));
        Assert.That(asset1.Hash, Is.EqualTo(_asset1!.Hash));
        Assert.That(asset1.ImageData, Is.Not.Null);

        Asset asset2 = assets.First(x => x.FileName == _asset2!.FileName);
        Assert.That(asset2.FolderId, Is.EqualTo(_asset2!.FolderId));
        Assert.That(asset2.Folder.Path, Is.EqualTo(_asset2!.Folder.Path));
        Assert.That(asset2.Hash, Is.EqualTo(_asset2!.Hash));
        Assert.That(asset2.ImageData, Is.Not.Null);

        Asset asset3 = assets.First(x => x.FileName == _asset3!.FileName);
        Assert.That(asset3.FolderId, Is.EqualTo(_asset3!.FolderId));
        Assert.That(asset3.Folder.Path, Is.EqualTo(_asset3!.Folder.Path));
        Assert.That(asset3.Hash, Is.EqualTo(_asset3!.Hash));
        Assert.That(asset3.ImageData, Is.Not.Null);

        Asset asset4 = assets.First(x => x.FileName == _asset4!.FileName);
        Assert.That(asset4.FolderId, Is.EqualTo(_asset4!.FolderId));
        Assert.That(asset4.Folder.Path, Is.EqualTo(_asset4!.Folder.Path));
        Assert.That(asset4.Hash, Is.EqualTo(_asset4!.Hash));
        Assert.That(asset4.ImageData, Is.Not.Null);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task
        GetAssetsByPath_ValidDirectoryAndFolderExistsAndNavigateAmongDifferentDirectories_ReturnsAssetsArrays(
            bool analyseVideos)
    {
        string rootDirectory = _assetsDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
        string duplicatesNewFolder2Directory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string testFolderDirectory = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER);

        ConfigureApplication(100, rootDirectory, 200, 150, false, false, false, analyseVideos);

        string outputVideoFirstFrameDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            await _application!.CatalogAssetsAsync(_ => { });

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();

            Assert.That(cataloguedAssets, analyseVideos ? Has.Count.EqualTo(52) : Has.Count.EqualTo(51));

            for (int i = 0; i < cataloguedAssets.Count; i++)
            {
                Assert.That(cataloguedAssets[i].ImageData, Is.Null);
            }

            // rootDirectory
            bool folderExists = _testableAssetRepository!.FolderExists(rootDirectory);

            Assert.That(folderExists, Is.True);

            Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(rootDirectory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(20));

            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.HOMER_GIF));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_1_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_10_PORTRAIT_PNG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_1_180_DEG_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_1_270_DEG_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_1_90_DEG_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_2_DUPLICATED_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_2_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_3_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_4_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_5_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_6_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_7_JPG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_8_JPEG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_9_PNG));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_11_HEIC));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_11_180_DEG_HEIC));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_11_270_DEG_HEIC));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_11_90_DEG_HEIC));
            Assert.That(assetsInRepository.Any(x => x.FileName == FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG));

            for (int i = 0; i < assetsInRepository.Length; i++)
            {
                Assert.That(assetsInRepository[i].ImageData, Is.Not.Null);
            }

            Asset[] assets = _application!.GetAssetsByPath(rootDirectory);

            Assert.That(assets, Has.Length.EqualTo(20));

            Assert.That(assets.Any(x => x.FileName == FileNames.HOMER_GIF));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_1_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_10_PORTRAIT_PNG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_1_180_DEG_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_1_270_DEG_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_1_90_DEG_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_2_DUPLICATED_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_2_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_3_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_4_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_5_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_6_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_7_JPG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_8_JPEG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_9_PNG));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_11_HEIC));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_11_180_DEG_HEIC));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_11_270_DEG_HEIC));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_11_90_DEG_HEIC));
            Assert.That(assets.Any(x => x.FileName == FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG));

            // duplicatesDirectory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesDirectory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesDirectory);

            Assert.That(assetsInRepository, Is.Empty);

            assets = _application!.GetAssetsByPath(duplicatesDirectory);

            Assert.That(assets, Is.Empty);

            // duplicatesNewFolder1Directory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesNewFolder1Directory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesNewFolder1Directory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(1));

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(assetsInRepository[0].ImageData, Is.Not.Null);

            assets = _application!.GetAssetsByPath(duplicatesNewFolder1Directory);

            Assert.That(assets, Has.Length.EqualTo(1));

            Assert.That(assets[0].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(assets[0].ImageData, Is.Not.Null);

            // duplicatesNewFolder2Directory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesNewFolder2Directory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesNewFolder2Directory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(4));

            Asset assetsInRepository1 = assetsInRepository.First(x => x.FileName == _asset1!.FileName);
            Assert.That(assetsInRepository1.ImageData, Is.Not.Null);

            Asset assetsInRepository2 = assetsInRepository.First(x => x.FileName == _asset2!.FileName);
            Assert.That(assetsInRepository2.ImageData, Is.Not.Null);

            Asset assetsInRepository3 = assetsInRepository.First(x => x.FileName == _asset3!.FileName);
            Assert.That(assetsInRepository3.ImageData, Is.Not.Null);

            Asset assetsInRepository4 = assetsInRepository.First(x => x.FileName == _asset4!.FileName);
            Assert.That(assetsInRepository4.ImageData, Is.Not.Null);

            assets = _application!.GetAssetsByPath(duplicatesNewFolder2Directory);

            Assert.That(assets, Has.Length.EqualTo(4));

            Asset asset1 = assets.First(x => x.FileName == _asset1!.FileName);
            Assert.That(asset1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(asset1.ImageData, Is.Not.Null);

            Asset asset2 = assets.First(x => x.FileName == _asset2!.FileName);
            Assert.That(asset2.Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(asset2.ImageData, Is.Not.Null);

            Asset asset3 = assets.First(x => x.FileName == _asset3!.FileName);
            Assert.That(asset3.Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(asset3.ImageData, Is.Not.Null);

            Asset asset4 = assets.First(x => x.FileName == _asset4!.FileName);
            Assert.That(asset4.Hash, Is.EqualTo(_asset4!.Hash));
            Assert.That(asset4.ImageData, Is.Not.Null);

            // testFolderDirectory
            folderExists = _testableAssetRepository!.FolderExists(testFolderDirectory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(testFolderDirectory);

            Assert.That(assetsInRepository, Is.Empty);

            assets = _application!.GetAssetsByPath(testFolderDirectory);

            Assert.That(assets, Is.Empty);

            // outputVideoFirstFrameDirectory
            folderExists = _testableAssetRepository!.FolderExists(outputVideoFirstFrameDirectory);

            if (analyseVideos)
            {
                Assert.That(folderExists, Is.True);

                assetsInRepository = _testableAssetRepository.GetAssetsByPath(outputVideoFirstFrameDirectory);

                Assert.That(assetsInRepository, Has.Length.EqualTo(1));

                Assert.That(assetsInRepository[0].FileName, Is.EqualTo(FileNames.HOMER_JPG));
                Assert.That(assetsInRepository[0].ImageData, Is.Not.Null);

                assets = _application!.GetAssetsByPath(outputVideoFirstFrameDirectory);

                Assert.That(assets, Has.Length.EqualTo(1));

                Assert.That(assets[0].FileName, Is.EqualTo(FileNames.HOMER_JPG));
                Assert.That(assets[0].ImageData, Is.Not.Null);
            }
            else
            {
                Assert.That(folderExists, Is.False);
            }
        }
        finally
        {
            if (analyseVideos)
            {
                Directory.Delete(outputVideoFirstFrameDirectory, true);
            }
        }
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderDoesNotExist_AddsFolderAndReturnsAssetsArray()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
        Assert.That(cataloguedAssets, Is.Empty);

        bool folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
        Assert.That(folderExists, Is.False);

        Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

        folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
        Assert.That(folderExists, Is.True);

        Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(assetsDirectory);

        Assert.That(assetsInRepository, Is.Empty);

        Assert.That(assets, Is.Empty); // Because folder added but assets not catalogued yet

        Folder folder = _testableAssetRepository.GetFolderByPath(assetsDirectory)!;

        Asset asset = AssetBuilder.Create()
            .WithFolder(folder)
            .WithFileName(FileNames.IMAGE_1_JPG)
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

        await _application!.CatalogAssetsAsync(_ => { });

        cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
        Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
        Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(asset.FileName));
        Assert.That(cataloguedAssets[0].Hash, Is.EqualTo(asset.Hash));
        Assert.That(cataloguedAssets[0].ImageData, Is.Null);

        Assert.That(File.Exists(asset.FullPath), Is.True);

        assets = _application!.GetAssetsByPath(assetsDirectory);

        Assert.That(assets, Has.Length.EqualTo(1));
        Assert.That(assets[0].FolderId, Is.EqualTo(asset.FolderId));
        Assert.That(assets[0].Folder.Path, Is.EqualTo(asset.Folder.Path));
        Assert.That(assets[0].FileName, Is.EqualTo(asset.FileName));
        Assert.That(assets[0].Hash, Is.EqualTo(asset.Hash));
        Assert.That(assets[0].ImageData, Is.Not.Null);
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExistsButNoAsset_ReturnsEmptyArray()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            await _application!.CatalogAssetsAsync(_ => { });

            bool folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
            Assert.That(folderExists, Is.True);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);
            Assert.That(assets, Is.Empty);
        }
        finally
        {
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase("", true)]
    [TestCase("", false)]
    [TestCase(" ", true)]
    [TestCase(" ", false)]
    [TestCase(null, true)]
    [TestCase(null, false)]
    public void GetAssetsByPath_DirectoryIsNullOrEmptyAndFolderExistsOrNot_ThrowsArgumentException(string? directory,
        bool folderExists)
    {
        ConfigureApplication(100, string.Empty, 200, 150, false, false, false, false);

        if (folderExists && directory != null)
        {
            _testableAssetRepository!.AddFolder(directory);
        }

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _application!.GetAssetsByPath(directory!));

        Assert.That(exception?.Message, Is.EqualTo("Directory cannot be null or empty."));
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssetsArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder);
            string filePath3 = Path.Combine(folderPath, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(assets, Has.Length.EqualTo(3));

            Asset asset1 = assets.First(x => x.FileName == _asset1!.FileName);
            Assert.That(asset1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(asset1.ImageData, Is.Not.Null);

            Asset asset2 = assets.First(x => x.FileName == _asset2!.FileName);
            Assert.That(asset2.Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(asset2.ImageData, Is.Not.Null);

            Asset asset3 = assets.First(x => x.FileName == _asset3!.FileName);
            Assert.That(asset3.Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(asset3.ImageData, Is.Not.Null);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IImageData? bitmapImage = null;
        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        imageProcessingServiceMock.LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(bitmapImage!);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        TestableAssetRepository testableAssetRepository = new(imageProcessingServiceMock, imageMetadataService,
            userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = application.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(assets, Is.Empty);

            imageProcessingServiceMock.Received(1).LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            testableAssetRepository.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssetsArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });
            string filePath = Path.Combine(folderPath2, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_AssetFolderIsDefault_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderDoesNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);
            Guid folderId = Guid.NewGuid();
            Folder folder = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2,
                _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            _testableAssetRepository!.AddFolder(folderPath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.NEW_FOLDER);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ExceptionIsThrown_ReturnsEmptyArray()
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory!);

        IImageProcessingService imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        imageProcessingServiceMock.LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>())
            .Throws(new Exception());

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        TestableAssetRepository testableAssetRepository = new(imageProcessingServiceMock, imageMetadataService,
            userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData1);
            testableAssetRepository.AddAsset(_asset2, assetData2);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(2));

            Asset cataloguedAsset1 = cataloguedAssets.First(x => x.FileName == _asset1!.FileName);
            Assert.That(cataloguedAsset1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(cataloguedAsset1.ImageData, Is.Null);

            Asset cataloguedAsset2 = cataloguedAssets.First(x => x.FileName == _asset2!.FileName);
            Assert.That(cataloguedAsset2.Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(cataloguedAsset2.ImageData, Is.Null);

            Asset[] assets = application.GetAssetsByPath(folderPath);
            Assert.That(assets, Is.Empty);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);

            imageProcessingServiceMock.Received(1).LoadBitmapThumbnailImage(
                Arg.Any<byte[]>(), Arg.Any<ImageRotation>(), Arg.Any<int>(), Arg.Any<int>());

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            testableAssetRepository.Dispose();
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        ConfigureApplication(100, _assetsDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset3 = _asset3!.WithFolder(folder);
            string filePath3 = Path.Combine(folderPath, _asset3.FileName);
            byte[] assetData3 = File.ReadAllBytes(filePath3);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));

            Asset cataloguedAsset1 = cataloguedAssets.First(x => x.FileName == _asset1!.FileName);
            Assert.That(cataloguedAsset1.Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(cataloguedAsset1.ImageData, Is.Null);

            Asset cataloguedAsset2 = cataloguedAssets.First(x => x.FileName == _asset2!.FileName);
            Assert.That(cataloguedAsset2.Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(cataloguedAsset2.ImageData, Is.Null);

            Asset cataloguedAsset3 = cataloguedAssets.First(x => x.FileName == _asset3!.FileName);
            Assert.That(cataloguedAsset3.Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(cataloguedAsset3.ImageData, Is.Null);

            Asset[] assets1 = [];
            Asset[] assets2 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _application!.GetAssetsByPath(folderPath),
                () => assets2 = _application!.GetAssetsByPath(folderPath)
            );

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Not.Null);
            Assert.That(cataloguedAssets[2].ImageData, Is.Not.Null);

            Assert.That(assets1, Has.Length.EqualTo(3));
            Assert.That(assets2, Has.Length.EqualTo(3));

            Asset asset1First = assets1.First(x => x.FileName == _asset1!.FileName);
            Assert.That(asset1First.Hash, Is.EqualTo(_asset1!.Hash));

            Asset asset2First = assets1.First(x => x.FileName == _asset2!.FileName);
            Assert.That(asset2First.Hash, Is.EqualTo(_asset2!.Hash));

            Asset asset3First = assets1.First(x => x.FileName == _asset3!.FileName);
            Assert.That(asset3First.Hash, Is.EqualTo(_asset3!.Hash));

            Asset asset1Second = assets1.First(x => x.FileName == _asset1!.FileName);
            Assert.That(asset1Second.Hash, Is.EqualTo(_asset1!.Hash));

            Asset asset2Second = assets1.First(x => x.FileName == _asset2!.FileName);
            Assert.That(asset2Second.Hash, Is.EqualTo(_asset2!.Hash));

            Asset asset3Second = assets1.First(x => x.FileName == _asset3!.FileName);
            Assert.That(asset3Second.Hash, Is.EqualTo(_asset3!.Hash));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }
}
