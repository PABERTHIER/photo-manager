using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetAssetsByPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image 9_duplicate.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
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

        _userConfigurationService = new (configurationRootMock.Object);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        _storageServiceMock!.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _testableAssetRepository = new (_database, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        _application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
    }

    [Test]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExists_ReturnsAssetsArray()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            await _application!.CatalogAssetsAsync(_ => {});

            bool folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
            Assert.That(folderExists, Is.True);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(4));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(cataloguedAssets[0].Hash, Is.EqualTo(_asset1!.Hash));
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[1].Hash, Is.EqualTo(_asset2!.Hash));
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(cataloguedAssets[2].Hash, Is.EqualTo(_asset3!.Hash));
            Assert.That(cataloguedAssets[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(cataloguedAssets[3].Hash, Is.EqualTo(_asset4!.Hash));

            Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(4));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(4));

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

            Assert.That(assets[1].FolderId, Is.EqualTo(_asset2!.FolderId));
            Assert.That(assets[1].Folder.Path, Is.EqualTo(_asset2!.Folder.Path));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets[1].Hash, Is.EqualTo(_asset2!.Hash));

            Assert.That(assets[2].FolderId, Is.EqualTo(_asset3!.FolderId));
            Assert.That(assets[2].Folder.Path, Is.EqualTo(_asset3!.Folder.Path));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets[2].Hash, Is.EqualTo(_asset3!.Hash));

            Assert.That(assets[3].FolderId, Is.EqualTo(_asset4!.FolderId));
            Assert.That(assets[3].Folder.Path, Is.EqualTo(_asset4!.Folder.Path));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(assets[3].Hash, Is.EqualTo(_asset4!.Hash));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(8));

            CheckThumbnail(assetsDirectory, _asset1!.FileName, _asset1!.Pixel.Thumbnail.Width, _asset1!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset2!.FileName, _asset2!.Pixel.Thumbnail.Width, _asset2!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset3!.FileName, _asset3!.Pixel.Thumbnail.Width, _asset3!.Pixel.Thumbnail.Height);
            CheckThumbnail(assetsDirectory, _asset4!.FileName, _asset4!.Pixel.Thumbnail.Width, _asset4!.Pixel.Thumbnail.Height);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task GetAssetsByPath_ValidDirectoryAndFolderExistsAndNavigateAmongDifferentDirectories_ReturnsAssetsArrays(bool analyseVideos)
    {
        string rootDirectory = _dataDirectory!;
        string duplicatesDirectory = Path.Combine(rootDirectory, "Duplicates");
        string duplicatesNewFolder1Directory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");
        string duplicatesNewFolder2Directory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");
        string testFolderDirectory = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureApplication(100, rootDirectory, 200, 150, false, false, false, analyseVideos);

        string outputVideoFirstFrameDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        try
        {
            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            await _application!.CatalogAssetsAsync(_ => {});

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, analyseVideos ? Has.Count.EqualTo(52) : Has.Count.EqualTo(51));

            // rootDirectory
            bool folderExists = _testableAssetRepository!.FolderExists(rootDirectory);

            Assert.That(folderExists, Is.True);

            Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(rootDirectory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(20));
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(20));

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo("Homer.gif"));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(assetsInRepository[2].FileName, Is.EqualTo("Image 10 portrait.png"));
            Assert.That(assetsInRepository[3].FileName, Is.EqualTo("Image 1_180_deg.jpg"));
            Assert.That(assetsInRepository[4].FileName, Is.EqualTo("Image 1_270_deg.jpg"));
            Assert.That(assetsInRepository[5].FileName, Is.EqualTo("Image 1_90_deg.jpg"));
            Assert.That(assetsInRepository[6].FileName, Is.EqualTo("Image 2 duplicated.jpg"));
            Assert.That(assetsInRepository[7].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(assetsInRepository[8].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(assetsInRepository[9].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(assetsInRepository[10].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(assetsInRepository[11].FileName, Is.EqualTo("Image 6.jpg"));
            Assert.That(assetsInRepository[12].FileName, Is.EqualTo("Image 7.jpg"));
            Assert.That(assetsInRepository[13].FileName, Is.EqualTo("Image 8.jpeg"));
            Assert.That(assetsInRepository[14].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(assetsInRepository[15].FileName, Is.EqualTo("Image_11.heic"));
            Assert.That(assetsInRepository[16].FileName, Is.EqualTo("Image_11_180.heic"));
            Assert.That(assetsInRepository[17].FileName, Is.EqualTo("Image_11_270.heic"));
            Assert.That(assetsInRepository[18].FileName, Is.EqualTo("Image_11_90.heic"));
            Assert.That(assetsInRepository[19].FileName, Is.EqualTo("IMAGE_WITH_UPPERCASE_NAME.JPG"));

            Asset[] assets = _application!.GetAssetsByPath(rootDirectory);

            Assert.That(assets, Has.Length.EqualTo(20));

            Assert.That(assets[0].FileName, Is.EqualTo("Homer.gif"));
            Assert.That(assets[1].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(assets[2].FileName, Is.EqualTo("Image 10 portrait.png"));
            Assert.That(assets[3].FileName, Is.EqualTo("Image 1_180_deg.jpg"));
            Assert.That(assets[4].FileName, Is.EqualTo("Image 1_270_deg.jpg"));
            Assert.That(assets[5].FileName, Is.EqualTo("Image 1_90_deg.jpg"));
            Assert.That(assets[6].FileName, Is.EqualTo("Image 2 duplicated.jpg"));
            Assert.That(assets[7].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(assets[8].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(assets[9].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(assets[10].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(assets[11].FileName, Is.EqualTo("Image 6.jpg"));
            Assert.That(assets[12].FileName, Is.EqualTo("Image 7.jpg"));
            Assert.That(assets[13].FileName, Is.EqualTo("Image 8.jpeg"));
            Assert.That(assets[14].FileName, Is.EqualTo("Image 9.png"));
            Assert.That(assets[15].FileName, Is.EqualTo("Image_11.heic"));
            Assert.That(assets[16].FileName, Is.EqualTo("Image_11_180.heic"));
            Assert.That(assets[17].FileName, Is.EqualTo("Image_11_270.heic"));
            Assert.That(assets[18].FileName, Is.EqualTo("Image_11_90.heic"));
            Assert.That(assets[19].FileName, Is.EqualTo("IMAGE_WITH_UPPERCASE_NAME.JPG"));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(40));

            // duplicatesDirectory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesDirectory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesDirectory);

            Assert.That(assetsInRepository, Is.Empty);
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(40));

            assets = _application!.GetAssetsByPath(duplicatesDirectory);

            Assert.That(assets, Is.Empty);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(40));

            // duplicatesNewFolder1Directory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesNewFolder1Directory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesNewFolder1Directory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(41));

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo("Image 1.jpg"));

            assets = _application!.GetAssetsByPath(duplicatesNewFolder1Directory);

            Assert.That(assets, Has.Length.EqualTo(1));

            Assert.That(assets[0].FileName, Is.EqualTo("Image 1.jpg"));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(42));

            // duplicatesNewFolder2Directory
            folderExists = _testableAssetRepository!.FolderExists(duplicatesNewFolder2Directory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(duplicatesNewFolder2Directory);

            Assert.That(assetsInRepository, Has.Length.EqualTo(4));
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(46));

            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assetsInRepository[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assetsInRepository[3].FileName, Is.EqualTo(_asset4!.FileName));

            assets = _application!.GetAssetsByPath(duplicatesNewFolder2Directory);

            Assert.That(assets, Has.Length.EqualTo(4));

            Assert.That(assets[0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(assets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets[2].FileName, Is.EqualTo(_asset3!.FileName));
            Assert.That(assets[3].FileName, Is.EqualTo(_asset4!.FileName));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(50));

            // testFolderDirectory
            folderExists = _testableAssetRepository!.FolderExists(testFolderDirectory);

            Assert.That(folderExists, Is.True);

            assetsInRepository = _testableAssetRepository.GetAssetsByPath(testFolderDirectory);

            Assert.That(assetsInRepository, Is.Empty);
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(50));

            assets = _application!.GetAssetsByPath(testFolderDirectory);

            Assert.That(assets, Is.Empty);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(50));

            // outputVideoFirstFrameDirectory

            folderExists = _testableAssetRepository!.FolderExists(outputVideoFirstFrameDirectory);

            if (analyseVideos)
            {
                Assert.That(folderExists, Is.True);

                assetsInRepository = _testableAssetRepository.GetAssetsByPath(outputVideoFirstFrameDirectory);

                Assert.That(assetsInRepository, Has.Length.EqualTo(1));
                _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(51));

                Assert.That(assetsInRepository[0].FileName, Is.EqualTo("Homer.jpg"));

                assets = _application!.GetAssetsByPath(outputVideoFirstFrameDirectory);

                Assert.That(assets, Has.Length.EqualTo(1));

                Assert.That(assets[0].FileName, Is.EqualTo("Homer.jpg"));

                _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(52));
            }
            else
            {
                Assert.That(folderExists, Is.False);

                _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(50));
            }

            CheckThumbnail(rootDirectory, "Homer.gif", 200, 112);
            CheckThumbnail(rootDirectory, "Image 1.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 10 portrait.png", 200, 112);
            CheckThumbnail(rootDirectory, "Image 1_180_deg.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 1_270_deg.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 1_90_deg.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 2 duplicated.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 2.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 3.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 4.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 5.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 6.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 7.jpg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 8.jpeg", 200, 112);
            CheckThumbnail(rootDirectory, "Image 9.png", 200, 112);
            CheckThumbnail(rootDirectory, "Image_11.heic", 200, 112);
            CheckThumbnail(rootDirectory, "Image_11_180.heic", 200, 112);
            CheckThumbnail(rootDirectory, "Image_11_270.heic", 200, 112);
            CheckThumbnail(rootDirectory, "Image_11_90.heic", 200, 112);
            CheckThumbnail(rootDirectory, "IMAGE_WITH_UPPERCASE_NAME.JPG", 200, 112);

            CheckThumbnail(duplicatesNewFolder1Directory, "Image 1.jpg", 200, 112);

            CheckThumbnail(duplicatesNewFolder2Directory, _asset1!.FileName, _asset1!.Pixel.Thumbnail.Width, _asset1!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset2!.FileName, _asset2!.Pixel.Thumbnail.Width, _asset2!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset3!.FileName, _asset3!.Pixel.Thumbnail.Width, _asset3!.Pixel.Thumbnail.Height);
            CheckThumbnail(duplicatesNewFolder2Directory, _asset4!.FileName, _asset4!.Pixel.Thumbnail.Width, _asset4!.Pixel.Thumbnail.Height);

            if (analyseVideos)
            {
                CheckThumbnail(outputVideoFirstFrameDirectory, "Homer.jpg", 200, 112);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder1");

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
            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            Assert.That(assets, Is.Empty); // Because folder added but assets not catalogued yet

            Folder folder = _testableAssetRepository.GetFolderByPath(assetsDirectory)!;

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = "Image 1.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = new (2024, 06, 07, 08, 54, 37)
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = Rotation.Rotate0,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            await _application!.CatalogAssetsAsync(_ => {});

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(asset.FileName));
            Assert.That(cataloguedAssets[0].Hash, Is.EqualTo(asset.Hash));

            Assert.That(File.Exists(asset.FullPath), Is.True);

            assets = _application!.GetAssetsByPath(assetsDirectory);

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FolderId, Is.EqualTo(asset.FolderId));
            Assert.That(assets[0].Folder.Path, Is.EqualTo(asset.Folder.Path));
            Assert.That(assets[0].FileName, Is.EqualTo(asset.FileName));
            Assert.That(assets[0].Hash, Is.EqualTo(asset.Hash));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

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
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            await _application!.CatalogAssetsAsync(_ => {});

            bool folderExists = _testableAssetRepository!.FolderExists(assetsDirectory);
            Assert.That(folderExists, Is.True);

            cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Asset[] assetsInRepository = _testableAssetRepository.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);
            Assert.That(assets, Is.Empty);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
    public void GetAssetsByPath_DirectoryIsNullOrEmptyAndFolderExistsOrNot_ThrowsArgumentException(string? directory, bool folderExists)
    {
        ConfigureApplication(100, string.Empty, 200, 150, false, false, false, false);

        try
        {
            if (folderExists)
            {
                _testableAssetRepository!.AddFolder(directory!);
            }

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _application!.GetAssetsByPath(directory!));

            Assert.That(exception?.Message, Is.EqualTo("Directory cannot be null or empty."));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            _asset3 = _asset3!.WithFolder(folder2);
            byte[] assetData3 = [4, 5, 6];

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

            Asset[] assets1 = _application!.GetAssetsByPath(folderPath1);
            Asset[] assets2 = _application!.GetAssetsByPath(folderPath2);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath1][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData3));

            Assert.That(assets1, Has.Length.EqualTo(2));
            Assert.That(assets2, Has.Length.EqualTo(1));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets1[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset3!.FileName));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));

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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        BitmapImage? bitmapImage = null;
        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(bitmapImage!);

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        TestableAssetRepository testableAssetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (testableAssetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (testableAssetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (testableAssetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new (testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder1");
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

            Asset[] assets = application.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

            storageServiceMock.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] assetData1 = [1, 2, 3];
            byte[] assetData2 = [4, 5, 6];

            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, assetData1 },
                { _asset2!.FileName, assetData2 }
            };

            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });

            _database!.WriteBlob(blobToWrite, _asset1!.Folder.ThumbnailsFilename);

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData1);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset[] assets = _application!.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));

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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder = _testableAssetRepository!.AddFolder(folderPath1);

            _asset1 = _asset1!.WithFolder(new() { Id = folder.Id, Path = folderPath2 });
            byte[] assetData = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset[] assets = _application!.GetAssetsByPath(folderPath1);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);
            Assert.That(thumbnails[folderPath1], Is.Empty);
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath2][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Has.Length.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1.FileName));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            byte[] assetData = [1, 2, 3];

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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
            Guid folderId = Guid.NewGuid();
            Folder folder = new() { Id = folderId, Path = folderPath };

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData = [1, 2, 3];

            List<Asset> cataloguedAssets = _testableAssetRepository!.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            _testableAssetRepository.AddAsset(_asset1!, assetData);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Asset[] assets = _application!.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(1));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(folderPath), Is.True);
            Assert.That(thumbnails[folderPath], Has.Count.EqualTo(1));
            Assert.That(thumbnails[folderPath].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath][_asset1.FileName], Is.EqualTo(assetData));

            Assert.That(assets, Is.Empty);

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");
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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder");

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

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);

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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new Exception());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        TestableAssetRepository testableAssetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (testableAssetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (testableAssetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (testableAssetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new (testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = testableAssetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath = Path.Combine(_dataDirectory!, "NewFolder1");
            Folder folder = testableAssetRepository.AddFolder(folderPath);

            _asset1 = _asset1!.WithFolder(folder);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder);
            byte[] assetData2 = [];

            List<Asset> cataloguedAssets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(cataloguedAssets, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            testableAssetRepository.AddAsset(_asset1, assetData1);
            testableAssetRepository.AddAsset(_asset2, assetData2);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(2));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));

            Asset[] assets = application.GetAssetsByPath(folderPath);

            Assert.That(cataloguedAssets, Has.Count.EqualTo(2));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));

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

            storageServiceMock.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);

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
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "NewFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "NewFolder2");
            Folder folder1 = _testableAssetRepository!.AddFolder(folderPath1);
            Folder folder2 = _testableAssetRepository!.AddFolder(folderPath2);

            _asset1 = _asset1!.WithFolder(folder1);
            byte[] assetData1 = [1, 2, 3];

            _asset2 = _asset2!.WithFolder(folder1);
            byte[] assetData2 = [];

            _asset3 = _asset3!.WithFolder(folder2);
            byte[] assetData3 = [4, 5, 6];

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

            Asset[] assets1 = [];
            Asset[] assets2 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => assets1 = _application!.GetAssetsByPath(folderPath1),
                () => assets2 = _application!.GetAssetsByPath(folderPath2)
            );

            Assert.That(cataloguedAssets, Has.Count.EqualTo(3));
            Assert.That(cataloguedAssets[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(cataloguedAssets[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(cataloguedAssets[2].FileName, Is.EqualTo(_asset3!.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(2));
            Assert.That(thumbnails.ContainsKey(folderPath1), Is.True);
            Assert.That(thumbnails.ContainsKey(folderPath2), Is.True);

            Assert.That(thumbnails[folderPath1], Has.Count.EqualTo(2));
            Assert.That(thumbnails[folderPath2], Has.Count.EqualTo(1));

            Assert.That(thumbnails[folderPath1].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[folderPath1].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[folderPath2].ContainsKey(_asset3.FileName), Is.True);

            Assert.That(thumbnails[folderPath1][_asset1.FileName], Is.EqualTo(assetData1));
            Assert.That(thumbnails[folderPath1][_asset2.FileName], Is.EqualTo(assetData2));
            Assert.That(thumbnails[folderPath2][_asset3.FileName], Is.EqualTo(assetData3));

            Assert.That(assets1, Has.Length.EqualTo(2));
            Assert.That(assets2, Has.Length.EqualTo(1));

            Assert.That(assets1[0].FileName, Is.EqualTo(_asset1.FileName));
            Assert.That(assets1[1].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(assets2[0].FileName, Is.EqualTo(_asset3!.FileName));

            _storageServiceMock!.Verify(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));

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

        BitmapImage? assetBitmapImage = _testableAssetRepository.LoadThumbnail(directory, fileName, thumbnailWidth, thumbnailHeight);
        Assert.That(assetBitmapImage, Is.Not.Null);
    }
}
