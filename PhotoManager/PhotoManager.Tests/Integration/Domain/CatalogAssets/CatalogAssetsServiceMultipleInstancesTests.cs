namespace PhotoManager.Tests.Integration.Domain.CatalogAssets;

[TestFixture]
public class CatalogAssetsServiceMultipleInstancesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private string? _databaseBackupPath;
    private string? _defaultAssetsDirectory;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";
    private const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";

    private CatalogAssetsService? _catalogAssetsService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;
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
                Modification = _expectedFileModificationDateTime
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
                Modification = _expectedFileModificationDateTime
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

    private void ConfigureCatalogAssetService(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        _catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
    }

    // TODO: Do same tests as CatalogAssetsServiceTests but with multiple instances instead of one
    [Test]
    [Ignore("Tests about two instances will be written later")]
    public async Task CatalogAssetsAsync_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDeleteTwoInstances_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
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
            string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");
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

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

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
            List<Folder> foldersContainingAssetsFirstSync = [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
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

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssetsFirstSync, thumbnails, assetsImageByteSizeFirstSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssetsFirstSync,
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
                foldersContainingAssetsFirstSync,
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
            
            // Second Sync "after closing the app", so new instance

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

            thumbnails = _testableAssetRepository!.GetThumbnails();

            List<Folder> foldersContainingAssetsSecondSync = [imageDeletedFolder!, imageUpdatedFolder!, videoFirstFrameFolder!];

            // Because stored in the DB and null is converted into an empty string
            // _asset4!.Metadata.Corrupted.Message = string.Empty;
            // _asset4!.Metadata.Rotated.Message = string.Empty;
            // _asset3Temp!.Metadata.Corrupted.Message = string.Empty;
            // _asset3Temp!.Metadata.Rotated.Message = string.Empty;

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.That(assetsInDirectory, Is.Empty);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(2));

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

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

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssetsSecondSync, thumbnails, assetsImageByteSizeSecondSync);
            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalog(
                _blobStorage!,
                _database!,
                _userConfigurationService,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssetsSecondSync,
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
                foldersContainingAssetsSecondSync,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.That(catalogChanges, Has.Count.EqualTo(33));

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
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
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

    // TODO: Need to rework many things in the app to handle this case
    [Test]
    [Ignore("Issue because it kept the previous directory and assets still exist in there, need to find a solution to handle this case")]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndSyncTwoDifferentDirectories_SyncTheAssets(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

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

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

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

            // Second Sync "after closing the app" to change the directory, so new instance

            assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder1");

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

            imagePath1 = Path.Combine(assetsDirectory, "Image 1.jpg");

            assetPaths = [imagePath1];
            assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(1));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.That(thumbnails, Is.Empty);

            Assert.That(_testableAssetRepository.HasChanges(), Is.False);

            catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            expectedAssets = [_asset2Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(1));

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            folderToAssetsMapping = new() { { folder!, expectedAssets } };
            assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET1_IMAGE_BYTE_SIZE } };

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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
