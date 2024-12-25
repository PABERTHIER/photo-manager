namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelCatalogAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private string? _databaseBackupPath;
    private string? _defaultAssetsDirectory;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";
    private const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    private Asset? _asset1Temp;
    private Asset? _asset2Temp;
    private Asset? _asset3Temp;
    private Asset? _asset4Temp;
    private Asset? _asset5Temp;

    private const int ASSET1_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET2_IMAGE_BYTE_SIZE = 11002;
    private const int ASSET3_IMAGE_BYTE_SIZE = 11002;
    private const int ASSET4_IMAGE_BYTE_SIZE = 5831;

    private const int ASSET1_TEMP_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET2_TEMP_IMAGE_BYTE_SIZE = 2097;
    private const int ASSET3_TEMP_IMAGE_BYTE_SIZE = 8594;
    private const int ASSET4_TEMP_IMAGE_BYTE_SIZE = 4779;
    private const int ASSET5_TEMP_IMAGE_BYTE_SIZE = 4779;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
        _databaseBackupPath = Path.Combine(_databaseDirectory, DATABASE_BACKUP_END_PATH);
        _defaultAssetsDirectory = Path.Combine(_dataDirectory, "Path");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _blobStorage = new();
        _database = new (new ObjectListStorage(), _blobStorage, new BackupStorage());
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
            FileName = "Image 1_duplicate_copied.jpg",
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
        _asset2Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
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
        _asset3Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Homer.gif",
            Pixel = new()
            {
                Asset = new() { Width = 320, Height = 320 },
                Thumbnail = new() { Width = 150, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 64123,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4Temp = new()
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
        _asset5Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "HomerDuplicated.jpg",
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

    private void ConfigureApplicationViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        PhotoManager.Application.Application application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    // ADD SECTION (Start) ------------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExists_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(9));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task CatalogAssets_AssetsImageAndVideosAndRootCatalogFolderExists_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");
            string videoPath2 = Path.Combine(_dataDirectory!, "Homer1s.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(assetsDirectory, "Homer1s.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath2, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(3));

            List<Folder> expectedFolders = [folder!, folder!, videoFirstFrameFolder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder!, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CatalogAssets_AssetsImageAndVideosAndAnalyseVideosIsFalseAndRootCatalogFolderExists_SyncTheAssetsButNotTheVideo()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");
            string videoPath2 = Path.Combine(_dataDirectory!, "Homer1s.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(assetsDirectory, "Homer1s.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath2, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            Assert.That(File.Exists(firstFramePath1), Is.False);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            List<Folder> expectedFolders = [folder!, folder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndCatalogBatchSizeIsSmaller_SyncTheFirstAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(2, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");

            List<string> assetPaths = [imagePath1, imagePath2];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneCorruptedImage_SyncTheAssetsButNotTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPaths = [imagePath2ToCopy];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE];

            // Corrupt image
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelper.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetNotCreated(catalogChanges, folderToAssetsMapping[folder!], imagePath1ToCopy, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
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

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndCallbackIsNull_SyncTheAssetsWithoutEvent(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogChangeCallback? catalogChangeCallback = null;

            await _applicationViewModel!.CatalogAssets(catalogChangeCallback!);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChangeCallback, Is.Null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(true, 0)]
    [TestCase(true, 2)]
    [TestCase(true, 100)]
    [TestCase(false, 0)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndIsCancellationRequestedOrCatalogBatchSizeIsEqualTo0_StopsTheSync(bool canceled, int catalogBatchSize)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(catalogBatchSize, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (canceled);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, folder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True); // SaveCatalog has not been done due to the Cancellation

            CatalogAssetsAsyncAsserts.CheckDefaultEmptyBackup(
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                false,
                false,
                folder!);

            Assert.That(catalogChanges, Has.Count.EqualTo(4));

            int increment = 0;

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
    // ADD SECTION (End) ------------------------------------------------------------------------------------------------

    // UPDATE SECTION (Start) -------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdated_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            _asset1Temp.FileProperties = _asset1Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(destinationFilePathToCopy, _asset1Temp.FileProperties.Modification);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    // TODO: It is not able to detect if a video has been updated
    [Test]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdated_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(3));

            List<Folder> expectedFolders = [folder!, folder!, videoFirstFrameFolder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder!, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.ThumbnailCreationDateTime);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(3));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdatedAndAnalyseVideosIsFalse_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            Assert.That(File.Exists(firstFramePath1), Is.False);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            List<Folder> expectedFolders = [folder!, folder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            _asset4Temp!.FileProperties = _asset4Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            Assert.That(File.Exists(firstFramePath1), Is.False);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(11));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndCatalogBatchSizeIsSmaller_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(6));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath1ToCopy, _asset2Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [_asset2Temp!, _asset3Temp!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new()
            {
                { folder!, [_asset2Temp!, _asset3Temp!]},
            };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(12));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder!],
                _asset3Temp,
                folder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneCorruptedImageIsUpdated_SyncTheAssetsAndRemovesTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPaths = [imagePath2ToCopy, imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            // Corrupt image
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelper.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            // Because recreated with CreateInvalidImage() + minus 10 min to simulate update
            _asset2Temp!.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddMinutes(-10) };
            File.SetLastWriteTime(imagePath1ToCopy, DateTime.Now);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(imagePath1ToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset2Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET2_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new() { { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(13));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(
                catalogChanges,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder!],
                _asset2Temp,
                folder!,
                true,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
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

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndBackupIsDeleted_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            CatalogAssetsAsyncAsserts.RemoveDatabaseBackup([folder!], blobsPath, tablesPath, backupFilePath);

            _asset1Temp.FileProperties = _asset1Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(destinationFilePathToCopy, _asset1Temp.FileProperties.Modification);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);
            expectedAssetsUpdated.Add(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }
    // UPDATE SECTION (End) -------------------------------------------------------------------------------------------

    // DELETE SECTION (Start) -----------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsDeleted_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.False);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            // If failing, just in case
            if (File.Exists(destinationFilePathToCopy))
            {
                File.Delete(destinationFilePathToCopy);
            }
        }
    }

    [Test]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeleted_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy, firstFramePath1];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET4_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(3));

            List<Folder> expectedFolders = [folder!, folder!, videoFirstFrameFolder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new()
            {
                { folder!, [_asset3Temp!, _asset2Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(3));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges,folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(videoPath1ToCopy))
            {
                File.Delete(videoPath1ToCopy);
            }
        }
    }

    [Test]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeletedAndAnalyseVideosIsFalse_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, videoPath1ToCopy];
            List<string> assetPathsAfterSync = [imagePath2ToCopy, imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET3_TEMP_IMAGE_BYTE_SIZE, ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(3));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            Assert.That(File.Exists(firstFramePath1), Is.False);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            List<Folder> expectedFolders = [folder!, folder!];
            List<string> expectedDirectories = [assetsDirectory, assetsDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!]} };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(7));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            Assert.That(File.Exists(firstFramePath1), Is.False);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(11));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(videoPath1ToCopy))
            {
                File.Delete(videoPath1ToCopy);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedThenAdded_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<string> assetPathsAfterFirstSync = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<string> assetPathsAfterSecondSync = [imagePath1, imagePath2, imagePath3, imagePath4, destinationFilePathToCopy];

            List<int> assetsImageByteSizeFirstSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeThirdSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssetsFirstSync = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<Asset> expectedAssetsSecondSync = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<Asset> expectedAssetsThirdSync = [_asset1!, _asset2!, _asset3!, _asset4!, _asset1Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new() { { folder!, expectedAssetsFirstSync } };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, [folder!], thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMappingFirstSync[folder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.False);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterFirstSync[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new() { { folder!, expectedAssetsSecondSync } };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, [folder!], thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMappingFirstSync[folder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsSecondSync, _asset1Temp, folder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Third sync

            File.Copy(imagePath1, destinationFilePathToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsThirdSync[i], assetPathsAfterSecondSync[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingThirdSync = new() { { folder!, expectedAssetsThirdSync } };
            Dictionary<string, int> assetNameToByteSizeMappingThirdSync = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingThirdSync, [folder!], thumbnails, assetsImageByteSizeThirdSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(22));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMappingFirstSync[folder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsSecondSync, _asset1Temp, folder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, expectedAssetsThirdSync, _asset1Temp, folder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            if (File.Exists(destinationFilePathToCopy))
            {
                File.Delete(destinationFilePathToCopy);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndCatalogBatchSizeIsSmaller_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(6));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            File.Delete(imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [_asset2Temp!, _asset3Temp!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new()
            {
                { folder!, [_asset2Temp!, _asset3Temp!] }
            };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE }
            };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(12));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder!],
                _asset3Temp,
                folder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath1ToCopy))
            {
                File.Delete(imagePath1ToCopy);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndBackupIsDeleted_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            CatalogAssetsAsyncAsserts.RemoveDatabaseBackup([folder!], blobsPath, tablesPath, backupFilePath);

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.False);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            // If failing, just in case
            if (File.Exists(destinationFilePathToCopy))
            {
                File.Delete(destinationFilePathToCopy);
            }
        }
    }

    // TODO: Rework the CancellationRequested (when done, add same test for update as well)
    [Test]
    [Ignore("Need to rework the CancellationRequested")]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneImageDeletedAndIsCancellationRequested_SyncTheAssetsAndRemovesDeletedOne(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(10));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Remove(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.False);

            CancellationToken cancellationToken = new (true);
            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            // If failing, just in case
            if (File.Exists(destinationFilePathToCopy))
            {
                File.Delete(destinationFilePathToCopy);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeleted_SyncTheAssetsAndRemovesDeletedOnes(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string tempDirectory = Path.Combine(assetsDirectory, "TempFolder");
        string destinationFilePathToCopy = Path.Combine(tempDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(tempDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4, destinationFilePathToCopy];

            List<int> assetsImageByteSizeFirstSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1, Has.Length.EqualTo(4));

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.That(assetsInDirectory2, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Null);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder1!);
            _asset2 = _asset2!.WithFolder(folder1!);
            _asset3 = _asset3!.WithFolder(folder1!);
            _asset4 = _asset4!.WithFolder(folder1!);
            _asset1Temp = _asset1Temp!.WithFolder(folder2!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset1Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1, Has.Count.EqualTo(4));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            List<string> expectedDirectories = [assetsDirectory, assetsDirectory, assetsDirectory, assetsDirectory, tempDirectory];
            List<Folder> expectedFolders = [folder1!, folder1!, folder1!, folder1!, folder2!];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [folder1!, folder2!];
            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder1!, [_asset1, _asset2, _asset3, _asset4] }, { folder2!, [_asset1Temp] } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, folders, thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                folders,
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(12));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder1!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder1!][..(i + 1)],
                    folderToAssetsMapping[folder1!][i],
                    folder1!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder2!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    tempDirectory,
                    folderToAssetsMapping[folder2!][..(i + 1)],
                    folderToAssetsMapping[folder2!][i],
                    folder2!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            Directory.Delete(tempDirectory, true);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1, Has.Length.EqualTo(4));

            Assert.That(File.Exists(destinationFilePathToCopy), Is.False);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            Folder? deletedFolder = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(deletedFolder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1, Has.Count.EqualTo(4));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder1!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder1!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder1!], thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder1!],
                [folder1!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder1!],
                [folder1!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(20));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder1!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder1!][..(i + 1)],
                    folderToAssetsMapping[folder1!][i],
                    folder1!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, 1, [folder2!], tempDirectory, ref increment); // Not foldersInRepository because this folder has been deleted
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, [folder2!], tempDirectory, ref increment); // Not foldersInRepository because this folder has been deleted

            for (int i = 0; i < folderToAssetsMapping[folder2!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    tempDirectory,
                    folderToAssetsMapping[folder2!][..(i + 1)],
                    folderToAssetsMapping[folder2!][i],
                    folder2!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, assetsFromRepositoryByPath2, _asset1Temp, folder2!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            // If failing, just in case
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeletedAndAndCatalogBatchSizeIsSmaller_SyncTheAssetsAndDoesNotRemoveAllDeletedOnes(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string tempDirectory = Path.Combine(assetsDirectory, "FolderToDelete");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(tempDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string destinationFilePathToCopy1 = Path.Combine(tempDirectory, _asset2Temp!.FileName);
            string destinationFilePathToCopy2 = Path.Combine(tempDirectory, _asset2!.FileName);
            File.Copy(imagePath1, destinationFilePathToCopy1);
            File.Copy(imagePath2, destinationFilePathToCopy2);

            List<string> assetPaths = [destinationFilePathToCopy1, destinationFilePathToCopy2];

            List<int> assetsImageByteSizeFirstSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeThirdSync = [ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1.Length, Is.EqualTo(0));

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.That(assetsInDirectory2, Has.Length.EqualTo(2));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Null);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(folder2!);

            List<Folder> folders = [folder1!, folder2!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new() { { folder2!, [_asset2Temp] } };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1.Count, Is.EqualTo(0));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[0], _asset2Temp, destinationFilePathToCopy1, tempDirectory, folder2!);
            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, [folder2!], thumbnails, assetsImageByteSizeFirstSync); // Only folder2 contains assets
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(8));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Not.Null);

            _asset2 = _asset2!.WithFolder(folder2!);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new() { { folder2!, [_asset2Temp, _asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE }
            };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1.Count, Is.EqualTo(0));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], folderToAssetsMappingSecondSync[folder2!][i], assetPaths[i], tempDirectory, folder2!);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, [folder2!], thumbnails, assetsImageByteSizeSecondSync); // Only folder2 contains assets
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(16));

            increment = 0;

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Third sync

            Directory.Delete(tempDirectory, true);

            Assert.That(File.Exists(destinationFilePathToCopy1), Is.False);
            Assert.That(File.Exists(destinationFilePathToCopy2), Is.False);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2, Is.Not.Null);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingThirdSync = new() { { folder2!, [_asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingThirdSync = new() { { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE } };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1.Count, Is.EqualTo(0));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[0], _asset2, destinationFilePathToCopy2, tempDirectory, folder2!);
            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingThirdSync, [folder2!], thumbnails, assetsImageByteSizeThirdSync); // Only folder2 contains assets
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                [folder2!],
                assetsFromRepository,
                folderToAssetsMappingThirdSync,
                assetNameToByteSizeMappingThirdSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(23));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 2, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, [_asset2], _asset2Temp, folder2!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Fourth sync

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1.Length, Is.EqualTo(0));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            Folder? folder2Updated = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2Updated, Is.Null);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingFourthSync = [];
            Dictionary<string, int> assetNameToByteSizeMappingFourthSync = [];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1.Count, Is.EqualTo(0));

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2.Count, Is.EqualTo(0));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository.Count, Is.EqualTo(0));

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, [], [], thumbnails, []); // No Folders and assets anymore
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder1!],
                [],
                assetsFromRepository,
                folderToAssetsMappingFourthSync,
                assetNameToByteSizeMappingFourthSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder1!],
                [],
                assetsFromRepository,
                folderToAssetsMappingFourthSync,
                assetNameToByteSizeMappingFourthSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(31));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, assetsDirectory, ref increment);  // Not foldersInRepository because this folder has been deleted
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingFirstSync[folder2!], _asset2Temp, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, folders, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, tempDirectory, folderToAssetsMappingSecondSync[folder2!], _asset2, folder2!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, [_asset2], _asset2Temp, folder2!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, tempDirectory, [], _asset2, folder2!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, tempDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
    // DELETE SECTION (End) -----------------------------------------------------------------------------------------

    // FULL SCENARIO SECTION (Start) --------------------------------------------------------------------------------
    [Test]
    public async Task CatalogAssets_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageUpdatedDirectory = Path.Combine(assetsDirectory, "FolderImageUpdated");
            string subDirDirectory = Path.Combine(assetsDirectory, "FolderSubDir");
            string subSubDirDirectory = Path.Combine(subDirDirectory, "FolderSubSubDir");

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image_11.heic");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string imagePath3 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath4 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image_11.heic");
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, "Image 1.jpg");
            string imagePath4ToCopy = Path.Combine(subDirDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, "HomerDuplicated.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");
            string firstFramePath2 = Path.Combine(firstFrameVideosDirectory, "HomerDuplicated.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2];

            List<int> assetsImageByteSizeFirstSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);
            Assert.That(File.Exists(firstFramePath2), Is.False);

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Null);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Null);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Null);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Null);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            Assert.That(File.Exists(firstFramePath1), Is.True);
            Assert.That(File.Exists(firstFramePath2), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);
            _asset5Temp = _asset5Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!, _asset5Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(6));

            List<Folder> expectedFolders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!, videoFirstFrameFolder!];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, subSubDirFolder!, videoFirstFrameFolder!];
            List<Folder> foldersContainingAssets = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder!, [_asset4!]},
                { imageDeletedFolder!, [_asset2!]},
                { imageUpdatedFolder!, [_asset2Temp!]},
                { subDirFolder!, [_asset3Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!, _asset5Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(21));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[videoFirstFrameFolder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][i],
                    videoFirstFrameFolder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder!, [_asset4!]},
                { subDirFolder!, [_asset3Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!, _asset5Temp!]},
                { imageUpdatedFolder!, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory.Length, Is.EqualTo(0));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            Assert.That(File.Exists(firstFramePath1), Is.True);
            Assert.That(File.Exists(firstFramePath2), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);
            _asset5Temp = _asset5Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset5Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2, imagePath3ToCopy];
            expectedFolders = [rootFolder!, subDirFolder!, videoFirstFrameFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(38));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[videoFirstFrameFolder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][i],
                    videoFirstFrameFolder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath2ToCopy))
            {
                File.Delete(imagePath2ToCopy);
            }
        }
    }

    // TODO: Actually, video with same name are considered the same, need to evolve this
    [Test]
    public async Task CatalogAssets_AssetsImageAndSameVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_SyncTheAssetsButNotTheVideoInSubdirectory()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageUpdatedDirectory = Path.Combine(assetsDirectory, "FolderImageUpdated");
            string subDirDirectory = Path.Combine(assetsDirectory, "FolderSubDir");
            string subSubDirDirectory = Path.Combine(subDirDirectory, "FolderSubSubDir");

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image_11.heic");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string imagePath3 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath4 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image_11.heic");
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, "Image 1.jpg");
            string imagePath4ToCopy = Path.Combine(subDirDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, "Homer.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, firstFramePath1];

            List<int> assetsImageByteSizeFirstSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Null);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Null);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Null);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Null);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            List<Folder> expectedFolders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, subSubDirFolder!, videoFirstFrameFolder!];
            List<Folder> foldersContainingAssets = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder!, [_asset4!]},
                { imageDeletedFolder!, [_asset2!]},
                { imageUpdatedFolder!, [_asset2Temp!]},
                { subDirFolder!, [_asset3Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(20));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder!, [_asset4!]},
                { subDirFolder!, [_asset3Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]},
                { imageUpdatedFolder!, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory.Length, Is.EqualTo(0));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(firstFramePath1), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, imagePath3ToCopy];
            expectedFolders = [rootFolder!, subDirFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(37));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath2ToCopy))
            {
                File.Delete(imagePath2ToCopy);
            }
        }
    }

    // TODO: Videos need to be in the same folder or at least in folder before the OutputFirstFrame (alphabetical order)
    [Test]
    public async Task CatalogAssets_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAfterOutputVideoAndUpdateAndDelete_SyncTheAssetsButTheVideoInSubdirectoryInTheSecondSync()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageUpdatedDirectory = Path.Combine(assetsDirectory, "FolderImageUpdated");
            string subDirDirectory = Path.Combine(assetsDirectory, "ZFolderSubDir");
            string subSubDirDirectory = Path.Combine(subDirDirectory, "FolderSubSubDir");

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image_11.heic");
            string imagePath2 = Path.Combine(_dataDirectory!, "Image 9.png");
            string imagePath3 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath4 = Path.Combine(_dataDirectory!, "Homer.gif");
            string videoPath1 = Path.Combine(_dataDirectory!, "Homer.mp4");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image_11.heic");
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, "Image 1.jpg");
            string imagePath4ToCopy = Path.Combine(subDirDirectory, "Homer.gif");
            string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, "HomerDuplicated.mp4");

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, "Homer.jpg");
            string firstFramePath2 = Path.Combine(firstFrameVideosDirectory, "HomerDuplicated.jpg");

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy, videoPath2ToCopy];
            List<string> assetPathsAfterSync = [imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, firstFramePath1, imagePath4ToCopy];

            List<int> assetsImageByteSizeFirstSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Assert.That(File.Exists(firstFramePath1), Is.False);

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Null);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Null);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Null);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Null);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Null);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            Assert.That(File.Exists(firstFramePath1), Is.True);
            Assert.That(File.Exists(firstFramePath2), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset4Temp!, _asset3Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            List<Folder> expectedFolders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, videoFirstFrameFolder!, subDirFolder!];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, firstFrameVideosDirectory, subDirDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, subSubDirFolder!, videoFirstFrameFolder!];
            List<Folder> foldersContainingAssets = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder!, [_asset4!]},
                { imageDeletedFolder!, [_asset2!]},
                { imageUpdatedFolder!, [_asset2Temp!]},
                { subDirFolder!, [_asset3Temp!]},
                { videoFirstFrameFolder!, [_asset4Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(20));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory.Length, Is.EqualTo(0));
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.That(imageDeletedFolder, Is.Not.Null);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.That(imageUpdatedFolder, Is.Not.Null);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.That(subDirFolder, Is.Not.Null);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.That(subSubDirFolder, Is.Not.Null);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFrameFolder, Is.Not.Null);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            Assert.That(File.Exists(firstFramePath1), Is.True);
            Assert.That(File.Exists(firstFramePath2), Is.True);

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);
            _asset5Temp = _asset5Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset4Temp!, _asset3Temp!, _asset2Temp!, _asset5Temp!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder!, [_asset4!]},
                { videoFirstFrameFolder!, [_asset4Temp!, _asset5Temp!]},
                { subDirFolder!, [_asset3Temp!]},
                { imageUpdatedFolder!, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE }
            };

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath.Count, Is.EqualTo(0));

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            assetPathsAfterSync = [imagePath1ToCopy, firstFramePath1, imagePath4ToCopy, imagePath3ToCopy, firstFramePath2];
            expectedFolders = [rootFolder!, videoFirstFrameFolder!, subDirFolder!, imageUpdatedFolder!, videoFirstFrameFolder!];
            expectedDirectories = [assetsDirectory, firstFrameVideosDirectory, subDirDirectory, imageUpdatedDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(38));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                catalogChanges,
                firstFrameVideosDirectory,
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                _asset5Temp!,
                videoFirstFrameFolder!,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            // If failing, just in case
            if (File.Exists(imagePath2ToCopy))
            {
                File.Delete(imagePath2ToCopy);
            }
        }
    }
    // FULL SCENARIO SECTION (End) --------------------------------------------------------------------------------

    // NO ASSET SECTION (Start) -----------------------------------------------------------------------------------
    [Test]
    [TestCase(2)]
    [TestCase(100)]
    public async Task CatalogAssets_NoAssetsAndRootCatalogFolderExists_DoesNothing(int catalogBatchSize)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            Directory.CreateDirectory(_defaultAssetsDirectory!);

            string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
            Assert.That(assetsInDirectory, Is.Empty);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, true, folder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckDefaultEmptyBackup(
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                true,
                true,
                folder!);

            Assert.That(catalogChanges, Has.Count.EqualTo(5));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, _defaultAssetsDirectory!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, _defaultAssetsDirectory!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(_defaultAssetsDirectory!, true);
        }
    }

    [Test]
    [TestCase(2)]
    [TestCase(100)]
    public async Task CatalogAssets_NoAssetsAndRootCatalogFolderDoesNotExist_DoesNothing(int catalogBatchSize)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, false, folder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckDefaultEmptyBackup(
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                true,
                false,
                folder!);

            Assert.That(catalogChanges, Has.Count.EqualTo(5));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderDeleted(catalogChanges, 0, foldersInRepository.Length, _defaultAssetsDirectory!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, _defaultAssetsDirectory!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_RootCatalogFolderPointsToAFile_DoesNotSyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Image 1.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, false, folder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckDefaultEmptyBackup(
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                true,
                false,
                folder!);

            Assert.That(catalogChanges, Has.Count.EqualTo(5));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderDeleted(catalogChanges, 0, foldersInRepository.Length, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, true)]
    [TestCase(0, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    [TestCase(100, true)]
    [TestCase(100, false)]
    public async Task CatalogAssets_NoAssetsAndRootCatalogExistAndFolderAndIsCancellationRequested_StopsTheSync(int catalogBatchSize, bool folderExists)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        try
        {
            if (folderExists)
            {
                Directory.CreateDirectory(_defaultAssetsDirectory!);

                string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
                Assert.That(assetsInDirectory, Is.Empty);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (true);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, folder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True); // SaveCatalog has not been done due to the Cancellation

            CatalogAssetsAsyncAsserts.CheckDefaultEmptyBackup(
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                false,
                false,
                folder!);

            Assert.That(catalogChanges, Has.Count.EqualTo(4));

            int increment = 0;

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, _defaultAssetsDirectory!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (folderExists)
            {
                Directory.Delete(_defaultAssetsDirectory!, true);
            }
        }
    }
    // NO ASSET SECTION (End) -----------------------------------------------------------------------------------

    // BACKUP SECTION (Start) -----------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndBackupExistsAndSameContent_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(9));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(13));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndBackupExistsAndOneNewAsset_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(9));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));
            Assert.That(File.Exists(destinationFilePathToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(15));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsAndRootCatalogFolderExistsAndBackupExistsOnDifferentDateAndOneNewAsset_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);
        string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
        string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(4));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!], thumbnails, assetsImageByteSize);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.That(catalogChanges, Has.Count.EqualTo(9));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second sync

            string oldBackupFileName = DateTime.Now.Date.ToString("20240110") + ".zip";
            string oldBackupFilePath = Path.Combine(_databaseBackupPath!, oldBackupFileName);
            File.Copy(backupFilePath, oldBackupFilePath);
            File.Delete(backupFilePath);

            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(5));
            Assert.That(File.Exists(destinationFilePathToCopy), Is.True);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(5));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMappingUpdated = new() { { folder!, expectedAssetsUpdated } };
            Dictionary<string, int> assetNameToByteSizeMappingUpdated = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset1Temp!.FileName, ASSET1_TEMP_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingUpdated, [folder!], thumbnails, assetsImageByteSizeUpdated);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            CatalogAssetsAsyncAsserts.CheckBackupAfter(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                _databasePath!,
                _databaseBackupPath!,
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder!],
                [folder!],
                assetsFromRepository,
                folderToAssetsMappingUpdated,
                assetNameToByteSizeMappingUpdated);

            Assert.That(catalogChanges, Has.Count.EqualTo(15));

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            Assert.That(File.Exists(oldBackupFilePath), Is.True);
            Assert.That(File.Exists(backupFilePath), Is.True);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
            // If failing, just in case
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
        }
    }
    // BACKUP SECTION (End) -----------------------------------------------------------------------------------

    // ERROR SECTION (Start) -----------------------------------------------------------------------------------
    // TODO: Test case where IsCancellationRequested for no assets (tests Already above for adding, updating and deleting)
    // (don't forget to add TestCase for AnalyseVideo)
    // TODO: Test if _currentFolderPath is good & SaveCatalog performed correctly
    // TODO: Test to Cancel the token for each method (testcase)
    [Test]
    [Ignore("Needs the rework of CancellationToken")]
    public void CatalogAssets_NoAssetsAndTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // ConfigureApplicationViewModel(defaultAssetsDirectory!);
        //
        // try
        // {
        //     CancellationTokenSource cancellationTokenSource = new();
        //
        //     // Start the task but don't wait for it
        //     Task task = _applicationViewModel!.CatalogAssets(null!, cancellationTokenSource.Token);
        //
        //     // Simulate cancellation after a short delay
        //     cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        //
        //     Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        // }
        // finally
        // {
        //     Directory.Delete(databaseDirectory!, true);
        // }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssets_AssetsImageAndRootCatalogFolderExistsAndAccessToFolderIsDenied_LogsError(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);
        LoggingAssertsService loggingAssertsService = new();

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imagePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePathToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");

            File.Copy(imagePath, imagePathToCopy);

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            Assert.That(File.Exists(imagePathToCopy), Is.True);

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            DirectoryHelper.DenyAccess(assetsDirectory);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(rootFolder, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(rootFolder!);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.False);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

            List<Folder> folders = [rootFolder!];

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, rootFolder!);

            Assert.That(_testableAssetRepository.HasChanges(), Is.True); // SaveCatalog has not been done due to the exception

            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            Assert.That(catalogChanges, Has.Count.EqualTo(3));

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            UnauthorizedAccessException unauthorizedAccessException = new ($"Access to the path '{assetsDirectory}' is denied.");
            Exception[] expectedExceptions = [unauthorizedAccessException];
            Type typeOfService = typeof(CatalogAssetsService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesException(catalogChanges, unauthorizedAccessException, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            DirectoryHelper.AllowAccess(assetsDirectory);
            Directory.Delete(assetsDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }
    // ERROR SECTION (End) -------------------------------------------------------------------------------------
}
