using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationLoadThumbnailTests
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
    private Asset? _asset1Temp;

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
        _asset1Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Homer.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 320, Height = 180 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 6599,
                Creation = DateTime.Now,
                Modification = DateTime.Now
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66",
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
    public async Task LoadThumbnail_CataloguedAssets_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            await _application!.CatalogAssetsAsync(_ => {});

            Folder folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory)!;

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            Assert.That(File.Exists(_asset1!.FullPath), Is.True);
            Assert.That(File.Exists(_asset2!.FullPath), Is.True);
            Assert.That(File.Exists(_asset3!.FullPath), Is.True);
            Assert.That(File.Exists(_asset4!.FullPath), Is.True);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.Not.Empty);

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

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.Not.Empty);
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.Not.Empty);

            // True because the catalog has been saved during the CatalogAssetsAsync
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task LoadThumbnail_CataloguedAssetFromVideo_SetsBitmapImageToTheAsset()
    {
        string tempDirectory = Path.Combine(_dataDirectory!, "TempFolder");
        string outputFirstFrameDirectory = Path.Combine(tempDirectory, "OutputVideoFirstFrame");

        ConfigureApplication(100, tempDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(tempDirectory);

            string videoSourcePath = Path.Combine(_dataDirectory!, "Homer.mp4");
            string videoDestinationPath = Path.Combine(tempDirectory, "Homer.mp4");
            File.Copy(videoSourcePath, videoDestinationPath);

            await _application!.CatalogAssetsAsync(_ => {});

            Folder folder = _testableAssetRepository!.GetFolderByPath(outputFirstFrameDirectory)!;

            _asset1Temp = _asset1Temp!.WithFolder(folder);

            Assert.That(File.Exists(_asset1Temp!.FullPath), Is.True);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(outputFirstFrameDirectory), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[outputFirstFrameDirectory].ContainsKey(_asset1Temp.FileName), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory][_asset1Temp.FileName], Is.Not.Empty);

            Assert.That(_asset1Temp.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1Temp);

            Assert.That(_asset1Temp.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));
            Assert.That(assets[0].FileName, Is.EqualTo(_asset1Temp.FileName));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(outputFirstFrameDirectory), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[outputFirstFrameDirectory].ContainsKey(_asset1Temp.FileName), Is.True);
            Assert.That(thumbnails[outputFirstFrameDirectory][_asset1Temp.FileName], Is.Not.Empty);

            // True because the catalog has been saved during the CatalogAssetsAsync
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1Temp.Folder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void LoadThumbnail_ThumbnailExists_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            byte[] asset1Data = [1, 2, 3];
            byte[] asset2Data = [2, 3, 4];
            byte[] asset3Data = [3, 4, 5];
            byte[] asset4Data = [4, 5, 6];

            _testableAssetRepository!.AddAsset(_asset1!, asset1Data);
            _testableAssetRepository!.AddAsset(_asset2!, asset2Data);
            _testableAssetRepository!.AddAsset(_asset3!, asset3Data);
            _testableAssetRepository!.AddAsset(_asset4!, asset4Data);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(asset1Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(asset2Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(asset3Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(asset4Data));

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

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(asset1Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(asset2Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(asset3Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(asset4Data));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExists_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            byte[] asset1Data = [1, 2, 3];
            byte[] asset2Data = [4, 5, 6];

            const string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, asset1Data },
                { fileName, asset2Data }
            };

            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _database!.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(2));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(asset1Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(fileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][fileName], Is.EqualTo(asset2Data));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinExistsAndRemoveOldThumbnailsDictionaryEntriesIs0_DoesNotSetBitmapImageToTheAsset()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, _dataDirectory!);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP, "0");

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

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
            byte[] asset1Data = [1, 2, 3];
            byte[] asset2Data = [4, 5, 6];

            const string fileName = "Image2.png";
            Dictionary<string, byte[]> blobToWrite = new()
            {
                { _asset1!.FileName, asset1Data },
                { fileName, asset2Data }
            };

            Folder addedFolder = testableAssetRepository.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(addedFolder);

            database.WriteBlob(blobToWrite, addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = testableAssetRepository.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            application.LoadThumbnail(_asset1);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, userConfigurationService.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_AssetDoesNotExistButBinNotContainingTheAssetExists_DoesNotSetBitmapImageToTheAssetAndWritesBlobAndDbFiles()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);

            _database!.WriteBlob([], addedFolder.ThumbnailsFilename);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, addedFolder.ThumbnailsFilename)), Is.True);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.True);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderDoesNotExist_SetsBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            _testableAssetRepository!.AddAsset(_asset1!, []);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(Array.Empty<byte>()));

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(1));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Not.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Has.Count.EqualTo(1));

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(1));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(Array.Empty<byte>()));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

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
    public void LoadThumbnail_AssetAndFolderDoNotExist_DoesNotSetBitmapImageToTheAsset()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            Guid folderId = Guid.NewGuid();

            _asset1 = _asset1!.WithFolder(new() { Id = folderId, Path = assetsDirectory });

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_asset1!.ImageData, Is.Null);

            _application!.LoadThumbnail(_asset1!);

            Assert.That(_asset1!.ImageData, Is.Null);

            List<Asset> assets = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assets, Is.Empty);

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Is.Empty);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FolderPathIsNull_ThrowsArgumentNullException()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_FileNameIsNull_ThrowsArgumentNullException()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            Asset asset = new()
            {
                FolderId = addedFolder.Id,
                Folder = addedFolder,
                FileName = null!,
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

            Assert.That(assetsUpdatedEvents, Is.Empty);

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _application!.LoadThumbnail(asset));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'key')"));

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void LoadThumbnail_ConcurrentAccess_BitmapImageAreSetToEachAssetSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _testableAssetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Folder addedFolder = _testableAssetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(addedFolder);
            _asset2 = _asset2!.WithFolder(addedFolder);
            _asset3 = _asset3!.WithFolder(addedFolder);
            _asset4 = _asset4!.WithFolder(addedFolder);

            byte[] asset1Data = [1, 2, 3];
            byte[] asset2Data = [2, 3, 4];
            byte[] asset3Data = [3, 4, 5];
            byte[] asset4Data = [4, 5, 6];

            _testableAssetRepository!.AddAsset(_asset1!, asset1Data);
            _testableAssetRepository!.AddAsset(_asset2!, asset2Data);
            _testableAssetRepository!.AddAsset(_asset3!, asset3Data);
            _testableAssetRepository!.AddAsset(_asset4!, asset4Data);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(asset1Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(asset2Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(asset3Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(asset4Data));

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

            Assert.That(thumbnails, Has.Count.EqualTo(1));
            Assert.That(thumbnails.ContainsKey(assetsDirectory), Is.True);
            Assert.That(thumbnails[assetsDirectory], Has.Count.EqualTo(4));

            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset1.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset1.FileName], Is.EqualTo(asset1Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset2.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset2.FileName], Is.EqualTo(asset2Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset3.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset3.FileName], Is.EqualTo(asset3Data));
            Assert.That(thumbnails[assetsDirectory].ContainsKey(_asset4.FileName), Is.True);
            Assert.That(thumbnails[assetsDirectory][_asset4.FileName], Is.EqualTo(asset4Data));

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, _asset1.Folder.ThumbnailsFilename)), Is.False);

            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "assets.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "folders.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "syncassetsdirectoriesdefinitions.db")), Is.False);
            Assert.That(File.Exists(Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables, "recenttargetpaths.db")), Is.False);

            Assert.That(assetsUpdatedEvents, Has.Count.EqualTo(4));
            Assert.That(assetsUpdatedEvents[0], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[1], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[2], Is.EqualTo(Reactive.Unit.Default));
            Assert.That(assetsUpdatedEvents[3], Is.EqualTo(Reactive.Unit.Default));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
