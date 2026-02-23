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
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

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
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
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

        _userConfigurationService = new(configurationRootMock.Object);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService =
            new(_testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_testableAssetRepository, fileOperationsService, _userConfigurationService);
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExists_ReturnsAssetsArray()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
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

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetsInRepository[0].Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assetsInRepository[1].Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(assetsInRepository[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assetsInRepository[2].Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(assetsInRepository[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(assetsInRepository[3].Hash, Is.EqualTo(_asset4!.Hash));

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

            Assert.That(assets[0].FolderId, Is.EqualTo(_asset1!.FolderId));
            Assert.That(assets[0].Folder.Path, Is.EqualTo(_asset1!.Folder.Path));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assets[0].Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(assets[0].ImageData, Is.Not.Null);

            Assert.That(assets[1].FolderId, Is.EqualTo(_asset2!.FolderId));
            Assert.That(assets[1].Folder.Path, Is.EqualTo(_asset2!.Folder.Path));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets[1].Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(assets[1].ImageData, Is.Not.Null);

            Assert.That(assets[2].FolderId, Is.EqualTo(_asset3!.FolderId));
            Assert.That(assets[2].Folder.Path, Is.EqualTo(_asset3!.Folder.Path));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets[2].Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(assets[2].ImageData, Is.Not.Null);

            Assert.That(assets[3].FolderId, Is.EqualTo(_asset4!.FolderId));
            Assert.That(assets[3].Folder.Path, Is.EqualTo(_asset4!.Folder.Path));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(assets[3].Hash, Is.EqualTo(_asset4!.Hash));
            Assert.That(assets[3].ImageData, Is.Not.Null);

            CheckThumbnail(assetsDirectory, _asset1!.FileName, _asset1!.Pixel.Thumbnail.Width,
                _asset1!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset2!.FileName, _asset2!.Pixel.Thumbnail.Width,
                _asset2!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset3!.FileName, _asset3!.Pixel.Thumbnail.Width,
                _asset3!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset4!.FileName, _asset4!.Pixel.Thumbnail.Width,
                _asset4!.Pixel.Thumbnail.Height);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task
        GetAssetsByPath_ValidDirectoryAndFolderExistsAndNavigateAmongDifferentDirectories_ReturnsAssetsArrays(
            bool analyseVideos)
    {
        string rootDirectory = _dataDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string duplicatesNewFolder1Directory =
            Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");
        string duplicatesNewFolder2Directory =
            Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_2}");
        string testFolderDirectory = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

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

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(FileNames.HOMER_GIF));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(assetsInRepository[2].FileName, Is.EqualTo(FileNames.IMAGE_10_PORTRAIT_PNG));
            Assert.That(assetsInRepository[3].FileName, Is.EqualTo(FileNames.IMAGE_1_180_DEG_JPG));
            Assert.That(assetsInRepository[4].FileName, Is.EqualTo(FileNames.IMAGE_1_270_DEG_JPG));
            Assert.That(assetsInRepository[5].FileName, Is.EqualTo(FileNames.IMAGE_1_90_DEG_JPG));
            Assert.That(assetsInRepository[6].FileName, Is.EqualTo(FileNames.IMAGE_2_DUPLICATED_JPG));
            Assert.That(assetsInRepository[7].FileName, Is.EqualTo(FileNames.IMAGE_2_JPG));
            Assert.That(assetsInRepository[8].FileName, Is.EqualTo(FileNames.IMAGE_3_JPG));
            Assert.That(assetsInRepository[9].FileName, Is.EqualTo(FileNames.IMAGE_4_JPG));
            Assert.That(assetsInRepository[10].FileName, Is.EqualTo(FileNames.IMAGE_5_JPG));
            Assert.That(assetsInRepository[11].FileName, Is.EqualTo(FileNames.IMAGE_6_JPG));
            Assert.That(assetsInRepository[12].FileName, Is.EqualTo(FileNames.IMAGE_7_JPG));
            Assert.That(assetsInRepository[13].FileName, Is.EqualTo(FileNames.IMAGE_8_JPEG));
            Assert.That(assetsInRepository[14].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(assetsInRepository[15].FileName, Is.EqualTo(FileNames.IMAGE_11_HEIC));
            Assert.That(assetsInRepository[16].FileName, Is.EqualTo(FileNames.IMAGE_11_180_DEG_HEIC));
            Assert.That(assetsInRepository[17].FileName, Is.EqualTo(FileNames.IMAGE_11_270_DEG_HEIC));
            Assert.That(assetsInRepository[18].FileName, Is.EqualTo(FileNames.IMAGE_11_90_DEG_HEIC));
            Assert.That(assetsInRepository[19].FileName, Is.EqualTo(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG));

            for (int i = 0; i < assetsInRepository.Length; i++)
            {
                Assert.That(assetsInRepository[i].ImageData, Is.Not.Null);
            }

            Asset[] assets = _application!.GetAssetsByPath(rootDirectory);

            Assert.That(assets, Has.Length.EqualTo(20));

            Assert.That(assets[0].FileName, Is.EqualTo(FileNames.HOMER_GIF));
            Assert.That(assets[1].FileName, Is.EqualTo(FileNames.IMAGE_1_JPG));
            Assert.That(assets[2].FileName, Is.EqualTo(FileNames.IMAGE_10_PORTRAIT_PNG));
            Assert.That(assets[3].FileName, Is.EqualTo(FileNames.IMAGE_1_180_DEG_JPG));
            Assert.That(assets[4].FileName, Is.EqualTo(FileNames.IMAGE_1_270_DEG_JPG));
            Assert.That(assets[5].FileName, Is.EqualTo(FileNames.IMAGE_1_90_DEG_JPG));
            Assert.That(assets[6].FileName, Is.EqualTo(FileNames.IMAGE_2_DUPLICATED_JPG));
            Assert.That(assets[7].FileName, Is.EqualTo(FileNames.IMAGE_2_JPG));
            Assert.That(assets[8].FileName, Is.EqualTo(FileNames.IMAGE_3_JPG));
            Assert.That(assets[9].FileName, Is.EqualTo(FileNames.IMAGE_4_JPG));
            Assert.That(assets[10].FileName, Is.EqualTo(FileNames.IMAGE_5_JPG));
            Assert.That(assets[11].FileName, Is.EqualTo(FileNames.IMAGE_6_JPG));
            Assert.That(assets[12].FileName, Is.EqualTo(FileNames.IMAGE_7_JPG));
            Assert.That(assets[13].FileName, Is.EqualTo(FileNames.IMAGE_8_JPEG));
            Assert.That(assets[14].FileName, Is.EqualTo(FileNames.IMAGE_9_PNG));
            Assert.That(assets[15].FileName, Is.EqualTo(FileNames.IMAGE_11_HEIC));
            Assert.That(assets[16].FileName, Is.EqualTo(FileNames.IMAGE_11_180_DEG_HEIC));
            Assert.That(assets[17].FileName, Is.EqualTo(FileNames.IMAGE_11_270_DEG_HEIC));
            Assert.That(assets[18].FileName, Is.EqualTo(FileNames.IMAGE_11_90_DEG_HEIC));
            Assert.That(assets[19].FileName, Is.EqualTo(FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG));

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

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetsInRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assetsInRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsInRepository[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assetsInRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsInRepository[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(assetsInRepository[3].ImageData, Is.Not.Null);

            assets = _application!.GetAssetsByPath(duplicatesNewFolder2Directory);

            Assert.That(assets, Has.Length.EqualTo(4));

            Assert.That(assets[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assets[0].ImageData, Is.Not.Null);
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets[1].ImageData, Is.Not.Null);
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets[2].ImageData, Is.Not.Null);
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(assets[3].ImageData, Is.Not.Null);

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

            CheckThumbnail(rootDirectory, FileNames.HOMER_GIF, ThumbnailWidthAsset.HOMER_GIF,
                ThumbnailHeightAsset.HOMER_GIF);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG,
                ThumbnailHeightAsset.IMAGE_1_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_10_PORTRAIT_PNG, ThumbnailWidthAsset.IMAGE_10_PORTRAIT_PNG,
                ThumbnailHeightAsset.IMAGE_10_PORTRAIT_PNG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_1_180_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_180_DEG_JPG,
                ThumbnailHeightAsset.IMAGE_1_180_DEG_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_1_270_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_270_DEG_JPG,
                ThumbnailHeightAsset.IMAGE_1_270_DEG_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_1_90_DEG_JPG, ThumbnailWidthAsset.IMAGE_1_90_DEG_JPG,
                ThumbnailHeightAsset.IMAGE_1_90_DEG_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_2_DUPLICATED_JPG, ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG,
                ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_2_JPG, ThumbnailWidthAsset.IMAGE_2_JPG,
                ThumbnailHeightAsset.IMAGE_2_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_3_JPG, ThumbnailWidthAsset.IMAGE_3_JPG,
                ThumbnailHeightAsset.IMAGE_3_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_4_JPG, ThumbnailWidthAsset.IMAGE_4_JPG,
                ThumbnailHeightAsset.IMAGE_4_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_5_JPG, ThumbnailWidthAsset.IMAGE_5_JPG,
                ThumbnailHeightAsset.IMAGE_5_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_6_JPG, ThumbnailWidthAsset.IMAGE_6_JPG,
                ThumbnailHeightAsset.IMAGE_6_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_7_JPG, ThumbnailWidthAsset.IMAGE_7_JPG,
                ThumbnailHeightAsset.IMAGE_7_JPG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_8_JPEG, ThumbnailWidthAsset.IMAGE_8_JPEG,
                ThumbnailHeightAsset.IMAGE_8_JPEG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_9_PNG, ThumbnailWidthAsset.IMAGE_9_PNG,
                ThumbnailHeightAsset.IMAGE_9_PNG);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_11_HEIC, ThumbnailWidthAsset.IMAGE_11_HEIC,
                ThumbnailHeightAsset.IMAGE_11_HEIC);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC,
                ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC,
                ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC,
                ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC);
            CheckThumbnail(rootDirectory, FileNames.IMAGE_WITH_UPPERCASE_NAME_JPG,
                ThumbnailWidthAsset.IMAGE_WITH_UPPERCASE_NAME_JPG, ThumbnailHeightAsset.IMAGE_WITH_UPPERCASE_NAME_JPG);

            CheckThumbnail(duplicatesNewFolder1Directory, FileNames.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG,
                ThumbnailHeightAsset.IMAGE_1_JPG);

            CheckThumbnail(duplicatesNewFolder2Directory, _asset1!.FileName, _asset1!.Pixel.Thumbnail.Width,
                _asset1!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset2!.FileName, _asset2!.Pixel.Thumbnail.Width,
                _asset2!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset3!.FileName, _asset3!.Pixel.Thumbnail.Width,
                _asset3!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset4!.FileName, _asset4!.Pixel.Thumbnail.Width,
                _asset4!.Pixel.Thumbnail.Height);

            if (analyseVideos)
            {
                CheckThumbnail(outputVideoFirstFrameDirectory, FileNames.HOMER_JPG, ThumbnailWidthAsset.HOMER_JPG,
                    ThumbnailHeightAsset.HOMER_JPG);
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (analyseVideos)
            {
                Directory.Delete(outputVideoFirstFrameDirectory, true);
            }
        }
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderDoesNotExist_AddsFolderAndReturnsAssetsArray()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, $"{Directories.DUPLICATES}\\{Directories.NEW_FOLDER_1}");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
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

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.IMAGE_1_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.IMAGE_1_JPG,
                        Height = ThumbnailHeightAsset.IMAGE_1_JPG
                    }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_1_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = Hashes.IMAGE_1_JPG,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

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

            CheckThumbnail(assetsDirectory, asset.FileName, asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExistsButNoAsset_ReturnsEmptyArray()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

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
            Directory.Delete(_databaseDirectory!, true);
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

        try
        {
            if (folderExists)
            {
                _testableAssetRepository!.AddFolder(directory!);
            }

            ArgumentException? exception =
                Assert.Throws<ArgumentException>(() => _application!.GetAssetsByPath(directory!));

            Assert.That(exception?.Message, Is.EqualTo("Directory cannot be null or empty."));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExist_ReturnsAssetsArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

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

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);

            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(3));

            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath][_asset3.FileName], Is.EqualTo(assetData3));

            Assert.That(assets, Has.Length.EqualTo(3));

            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3!.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButLoadBitmapThumbnailImageReturnsNull_ReturnsEmptyArray()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        BitmapImage? bitmapImage = null;
        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Mock<IImageProcessingService> imageProcessingServiceMock = new();
        imageProcessingServiceMock.Setup(x =>
            x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(bitmapImage!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(database, pathProviderServiceMock.Object,
            imageProcessingServiceMock.Object, imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService,
            imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService,
            imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(testableAssetRepository, fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = application.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

            imageProcessingServiceMock.Verify(x => x.LoadBitmapThumbnailImage(
                It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistButBinExists_ReturnsAssetsArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            string filePath1 = Path.Combine(folderPath2, _asset1!.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);
            string filePath2 = Path.Combine(folderPath2, _asset2!.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            _asset1 = _asset1!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Not.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(2));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset2.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath1][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath2][_asset2.FileName], Is.EqualTo(assetData2));

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailsAndFolderExistDifferentDirectory_ReturnsAssetsArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);
            string folderPath2 = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });
            string filePath = Path.Combine(folderPath2, _asset1.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);
            Assert.That(thumbnails[folderPath1], Is.Empty);
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath2][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_AssetFolderIsDefault_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            string filePath = Path.Combine(folderPath, _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderDoesNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);
            Guid folderId = Guid.NewGuid();
            Folder folder = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            string filePath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2,
                _asset1!.FileName);
            byte[] assetData = File.ReadAllBytes(filePath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            _testableAssetRepository!.AddFolder(folderPath);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ThumbnailDoesNotExistButBinExists_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER);
            Folder folder = _testableAssetRepository!.AddFolder(folderPath);

            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            _database!.WriteBlob(blobToWrite, folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_FolderAndThumbnailsDoNotExist_ReturnsEmptyArray()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.NEW_FOLDER);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Is.Empty);

            Assert.That(assets, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ExceptionThrown_ReturnsAssetsWithPartialData()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Mock<IImageProcessingService> imageProcessingServiceMock = new();
        imageProcessingServiceMock.Setup(x =>
            x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        TestableAssetRepository testableAssetRepository = new(database, pathProviderServiceMock.Object,
            imageProcessingServiceMock.Object, imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService,
            imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService,
            imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(testableAssetRepository, fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            byte[] assetData1 = File.ReadAllBytes(filePath1);

            _asset2 = _asset2!.WithFolder(folder);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            byte[] assetData2 = File.ReadAllBytes(filePath2);

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData1);
            testableAssetRepository.AddAsset(_asset2, assetData2);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);

            Asset[] assets = application.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);

            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(2));

            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);

            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));

            Assert.That(assets, Has.Length.EqualTo(2));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2.FileName));

            imageProcessingServiceMock.Verify(x => x.LoadBitmapThumbnailImage(
                It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetAssetsByPath_ConcurrentAccess_AssetsAreHandledSafely()
    {
        ConfigureApplication(100, _dataDirectory!, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription =
            _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);
            _testableAssetRepository.AddAsset(_asset2, assetData2);
            _testableAssetRepository.AddAsset(_asset3, assetData3);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[0].ImageData, Is.Null);
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].ImageData, Is.Null);
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].ImageData, Is.Null);

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

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);

            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(3));

            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath][_asset3.FileName], Is.EqualTo(assetData3));

            Assert.That(assets1, Has.Length.EqualTo(3));
            Assert.That(assets2, Has.Length.EqualTo(3));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets1[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets1[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets2[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[2].FileName, Is.EqualTo(_asset3!.FileName));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(3));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    private void CheckThumbnail(string directory, string fileName, int thumbnailWidth, int thumbnailHeight)
    {
        bool assetContainsThumbnail = _testableAssetRepository!.ContainsThumbnail(directory, fileName);
        Assert.That(assetContainsThumbnail, Is.True);

        BitmapImage? assetBitmapImage =
            _testableAssetRepository.LoadThumbnail(directory, fileName, thumbnailWidth, thumbnailHeight);
        Assert.That(assetBitmapImage, Is.Not.Null);
    }
}
