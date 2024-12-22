using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelNotifyCatalogChangeTests
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
    private PhotoManager.Application.Application? _application;
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
        _application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (_application);
    }

    // ADD SECTION (Start) ------------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExists_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(17, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task NotifyCatalogChange_AssetsImageAndVideosAndRootCatalogFolderExists_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                2,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(14, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);
            
            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                1,
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(22, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[21]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task NotifyCatalogChange_AssetsImageAndVideosAndAnalyseVideosIsFalseAndRootCatalogFolderExists_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(0, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                0,
                [],
                null!,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(19, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[18]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndCatalogBatchSizeIsSmaller_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(2, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");

            List<string> assetPaths = [imagePath1, imagePath2];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneCorruptedImage_NotifiesChangesButNotTheCorruptedImage(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeAssetNotCreated(catalogChanges, assetsDirectory, 1, folderToAssetsMapping[folder!], imagePath1ToCopy, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.AreEqual(9, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    [TestCase(true, 0)]
    [TestCase(true, 2)]
    [TestCase(true, 100)]
    [TestCase(false, 0)]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndIsCancellationRequestedOrCatalogBatchSizeIsEqualTo0_NotifiesNoAssetChanges(bool canceled, int catalogBatchSize)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(catalogBatchSize, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (canceled);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, folder!);

            Assert.IsTrue(_testableAssetRepository.HasChanges()); // SaveCatalog has not been done due to the Cancellation

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

            Assert.AreEqual(4, catalogChanges.Count);

            int increment = 0;

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(_applicationViewModel!, assetsDirectory, 0, [], null!, folder!, false);

            Assert.AreEqual(4, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);

            CheckInstance(applicationViewModelInstances, assetsDirectory, 0, [], null!, folder!, false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdated_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(5, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNotNull(assetsFromRepository[2].ImageData);
            Assert.IsNotNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetUpdated(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(36, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[35]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            File.Delete(destinationFilePathToCopy);
        }
    }

    // TODO: It is not able to detect if a video has been updated
    [Test]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdated_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                2,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(14, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.ThumbnailCreationDateTime);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNull(assetsFromRepository[2].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                1,
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(28, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[27]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneVideoIsUpdatedAndAnalyseVideosIsFalse_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            _asset4Temp!.FileProperties = _asset4Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(videoPath1ToCopy, _asset4Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(3, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            Assert.IsFalse(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(11, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(15, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(0, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                0,
                [],
                null!,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndCatalogBatchSizeIsSmaller_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");

            string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(6, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.AreEqual(8, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.Copy(imagePath2, imagePath2ToCopy);

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath1ToCopy, _asset2Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

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

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNull(assetsFromRepository[1].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(12, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                _asset3Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.AreEqual(16, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneCorruptedImageIsUpdated_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, "Image 1_Temp.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(2, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset3Temp = _asset3Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            // Corrupt image
            File.Copy(imagePath1ToCopy, imagePath1ToCopyTemp);
            ImageHelper.CreateInvalidImage(imagePath1ToCopyTemp, imagePath1ToCopy);
            File.Delete(imagePath1ToCopyTemp);
            Assert.IsTrue(File.Exists(imagePath1ToCopy));

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
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(13, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                folderToAssetsMappingUpdated[folder!].Count,
                folderToAssetsMappingUpdated[folder!],
                _asset2Temp,
                folder!,
                true,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsUpdatedAndBackupIsDeleted_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(5, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNotNull(assetsFromRepository[2].ImageData);
            Assert.IsNotNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetUpdated(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(36, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[35]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsDeleted_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated.Count, expectedAssetsUpdated, _asset1Temp, folder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.AreEqual(32, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeleted_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!, _asset4Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                2,
                folderToAssetsMapping[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(14, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(3, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNull(assetsFromRepository[2].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                1,
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(28, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[27]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneVideoIsDeletedAndAnalyseVideosIsFalse_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string videoPath1ToCopy = Path.Combine(assetsDirectory, "Homer.mp4");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(3, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            Assert.IsFalse(File.Exists(firstFramePath1));

            _asset3Temp = _asset3Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset3Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(7, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(11, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.Delete(videoPath1ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            Assert.IsFalse(File.Exists(firstFramePath1));

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(11, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(15, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Changing folder
            Assert.AreEqual(2, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(0, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                0,
                [],
                null!,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedThenAdded_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssetsFirstSync = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];
            List<Asset> expectedAssetsSecondSync = [_asset1!, _asset2!, _asset3!, _asset4!];
            List<Asset> expectedAssetsThirdSync = [_asset1!, _asset2!, _asset3!, _asset4!, _asset1Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMappingFirstSync[folder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMappingFirstSync[folder!],
                folderToAssetsMappingFirstSync[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMappingFirstSync[folder!],
                folderToAssetsMappingFirstSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.Delete(destinationFilePathToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterFirstSync[i], assetsDirectory, folder!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsSecondSync.Count, expectedAssetsSecondSync, _asset1Temp, folder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMappingSecondSync[folder!],
                folderToAssetsMappingSecondSync[folder!][0],
                folder!,
                true);

            Assert.AreEqual(32, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMappingSecondSync[folder!],
                folderToAssetsMappingSecondSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Third sync

            File.Copy(imagePath1, destinationFilePathToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            Assert.IsTrue(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsThirdSync[i], assetPathsAfterSecondSync[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNotNull(assetsFromRepository[2].ImageData);
            Assert.IsNotNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(22, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsThirdSync.Count, expectedAssetsThirdSync, _asset1Temp, folder!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMappingThirdSync[folder!],
                folderToAssetsMappingThirdSync[folder!][0],
                folder!,
                true);

            Assert.AreEqual(40, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[35]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[36]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[37]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[38]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[39]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMappingThirdSync[folder!],
                folderToAssetsMappingThirdSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndCatalogBatchSizeIsSmaller_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imagePath1ToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePath2 = Path.Combine(_dataDirectory!, "Homer.gif");
            string imagePath2ToCopy = Path.Combine(assetsDirectory, "Homer.gif");

            File.Copy(imagePath1, imagePath1ToCopy);

            List<string> assetPaths = [imagePath1ToCopy];
            List<int> assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(6, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.AreEqual(8, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            List<string> assetPathsUpdated = [imagePath1ToCopy, imagePath2ToCopy];
            List<int> assetsImageByteSizeUpdated = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET3_TEMP_IMAGE_BYTE_SIZE];

            File.Delete(imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

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

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNull(assetsFromRepository[1].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(12, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                _asset3Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.AreEqual(16, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                2,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageIsDeletedAndBackupIsDeleted_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated.Count, expectedAssetsUpdated, _asset1Temp, folder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                folder!,
                true);

            Assert.AreEqual(32, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("ViewerPosition", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("CanGoToPreviousAsset", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("CanGoToNextAsset", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("CurrentAsset", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneImageDeletedAndIsCancellationRequested_NotifiesNoAssetChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");
            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPaths = [imagePath1, destinationFilePathToCopy, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET1_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset1Temp!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(10, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }


            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(4, assetsInDirectory.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            CancellationToken cancellationToken = new (true);
            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(24, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeleted_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string tempDirectory = Path.Combine(assetsDirectory, "TempFolder");
        string destinationFilePathToCopy = Path.Combine(tempDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(4, assetsInDirectory1.Length);

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.AreEqual(1, assetsInDirectory2.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder1);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath1);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath2);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset1 = _asset1!.WithFolder(folder1!);
            _asset2 = _asset2!.WithFolder(folder1!);
            _asset3 = _asset3!.WithFolder(folder1!);
            _asset4 = _asset4!.WithFolder(folder1!);
            _asset1Temp = _asset1Temp!.WithFolder(folder2!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset1Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(12, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder1!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder1!][..(i + 1)],
                    folderToAssetsMapping[folder1!][i],
                    folder1!,
                    ref increment);
            }

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder2!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    tempDirectory,
                    assetsDirectory,
                    folderToAssetsMapping[folder1!].Count,
                    folderToAssetsMapping[folder2!][..(i + 1)],
                    folderToAssetsMapping[folder2!][i],
                    folder2!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder1!],
                folderToAssetsMapping[folder1!][0],
                folder1!,
                true);

            Assert.AreEqual(20, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder1!],
                folderToAssetsMapping[folder1!][0],
                folder1!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            Directory.Delete(tempDirectory, true);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Remove(destinationFilePathToCopy);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Remove(_asset1Temp);

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory1.Length);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            Folder? deletedFolder = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(deletedFolder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath2);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder1!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(20, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, tempDirectory, assetsDirectory, 4, assetsFromRepositoryByPath2, _asset1Temp, folder2!, false, ref increment);
            NotifyCatalogChangeFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMappingUpdated[folder1!],
                folderToAssetsMappingUpdated[folder1!][0],
                folder1!,
                true);

            Assert.AreEqual(28, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[27]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMappingUpdated[folder1!],
                folderToAssetsMappingUpdated[folder1!][0],
                folder1!,
                true);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.AreEqual(1, folderRemovedEvents.Count);
            Assert.AreEqual(folder2, folderRemovedEvents[0]);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndOneFolderIsDeletedAndAndCatalogBatchSizeIsSmaller_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string tempDirectory = Path.Combine(assetsDirectory, "FolderToDelete");

        ConfigureApplicationViewModel(1, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(0, assetsInDirectory1.Length);

            string[] assetsInDirectory2 = Directory.GetFiles(tempDirectory);
            Assert.AreEqual(2, assetsInDirectory2.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder1);

            Folder? folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath1);

            List<Asset> assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath2);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset2Temp = _asset2Temp!.WithFolder(folder2!);

            List<Folder> folders = [folder1!, folder2!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new() { { folder2!, [_asset2Temp] } };
            Dictionary<string, int> assetNameToByteSizeMappingFirstSync = new() { { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE } };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(8, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                0,
                folderToAssetsMappingFirstSync[folder2!],
                _asset2Temp,
                folder2!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            Assert.AreEqual(8, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            _asset2 = _asset2!.WithFolder(folder2!);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new() { { folder2!, [_asset2Temp, _asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE }
            };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(2, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(2, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                // Assets are not in the current folder, so ImageData stays null
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(16, catalogChanges.Count);

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                0,
                folderToAssetsMappingSecondSync[folder2!],
                _asset2,
                folder2!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            Assert.AreEqual(16, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Third sync

            Directory.Delete(tempDirectory, true);

            Assert.IsFalse(File.Exists(destinationFilePathToCopy1));
            Assert.IsFalse(File.Exists(destinationFilePathToCopy2));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            folder2 = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNotNull(folder2);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingThirdSync = new() { { folder2!, [_asset2] } };
            Dictionary<string, int> assetNameToByteSizeMappingThirdSync = new() { { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE } };

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(23, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 2, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, tempDirectory, assetsDirectory, 0, [_asset2], _asset2Temp, folder2!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.IsEmpty(folderRemovedEvents);

            // Fourth sync

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(0, assetsInDirectory1.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder1);

            Folder? folder2Updated = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.IsNull(folder2Updated);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingFourthSync = [];
            Dictionary<string, int> assetNameToByteSizeMappingFourthSync = [];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath1.Count);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.AreEqual(0, assetsFromRepositoryByPath2.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(0, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(31, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, tempDirectory, assetsDirectory, 0, [], _asset2, folder2!, false, ref increment);
            NotifyCatalogChangeFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            Assert.AreEqual(31, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(1, folderAddedEvents.Count);
            Assert.AreEqual(folder2, folderAddedEvents[0]);

            Assert.AreEqual(1, folderRemovedEvents.Count);
            Assert.AreEqual(folder2, folderRemovedEvents[0]);
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
    public async Task NotifyCatalogChange_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));
            Assert.IsFalse(File.Exists(firstFramePath2));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNull(imageDeletedFolder);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNull(imageUpdatedFolder);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNull(subDirFolder);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNull(subSubDirFolder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.IsEmpty(assetsInImageUpdatedFolderFromRepositoryByPath);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.IsEmpty(assetsInSubDirFolderFromRepositoryByPath);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.IsEmpty(assetsInSubSubDirFolderFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);
            _asset5Temp = _asset5Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!, _asset5Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInImageDeletedFolderFromRepositoryByPath.Count);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(2, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(6, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(21, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                _asset4,
                rootFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageDeletedFolder!],
                _asset2,
                imageDeletedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                subDirDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[subDirFolder!],
                _asset3Temp,
                subDirFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[videoFirstFrameFolder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    assetsDirectory,
                    1,
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][..(i + 1)],
                    folderToAssetsMappingFirstSync[videoFirstFrameFolder!][i],
                    videoFirstFrameFolder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(23, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[2]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);
            
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
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(0, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);
            _asset5Temp = _asset5Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset5Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(2, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2, imagePath3ToCopy];
            expectedFolders = [rootFolder!, subDirFolder!, videoFirstFrameFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNull(assetsFromRepository[1].ImageData);
            Assert.IsNull(assetsFromRepository[2].ImageData);
            Assert.IsNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(38, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, imageDeletedDirectory, assetsDirectory, 1, [], _asset2!, imageDeletedFolder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetUpdated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(40, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[35]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[36]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[37]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[38]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[39]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[2]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.AreEqual(1, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                0,
                [],
                null!,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                1,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                1,
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                0,
                [],
                null!,
                subSubDirFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(2, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                2,
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                true);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(64, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[40]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[41]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[42]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[43]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[44]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[45]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[46]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[47]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[48]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[49]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[50]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[51]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[52]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[53]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[54]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[55]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[56]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[57]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[58]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[59]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[60]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[61]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[62]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[63]);
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
    public async Task NotifyCatalogChange_AssetsImageAndSameVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDelete_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNull(imageDeletedFolder);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNull(imageUpdatedFolder);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNull(subDirFolder);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNull(subSubDirFolder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.IsEmpty(assetsInImageUpdatedFolderFromRepositoryByPath);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.IsEmpty(assetsInSubDirFolderFromRepositoryByPath);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.IsEmpty(assetsInSubSubDirFolderFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInImageDeletedFolderFromRepositoryByPath.Count);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(20, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                _asset4,
                rootFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageDeletedFolder!],
                _asset2,
                imageDeletedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                subDirDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[subDirFolder!],
                _asset3Temp,
                subDirFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(22, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[2]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);
            
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
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(0, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset2Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, imagePath3ToCopy];
            expectedFolders = [rootFolder!, subDirFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNull(assetsFromRepository[1].ImageData);
            Assert.IsNull(assetsFromRepository[2].ImageData);
            Assert.IsNull(assetsFromRepository[3].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(37, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, imageDeletedDirectory, assetsDirectory, 1, [], _asset2!, imageDeletedFolder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetUpdated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(39, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[35]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[36]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[37]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[38]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[2]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.AreEqual(1, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                0,
                [],
                null!,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                1,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                1,
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                0,
                [],
                null!,
                subSubDirFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                1,
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(63, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[39]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[40]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[41]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[42]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[43]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[44]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[45]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[46]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[47]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[48]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[49]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[50]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[51]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[52]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[53]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[54]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[55]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[56]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[57]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[58]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[59]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[60]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[61]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[62]);
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
    public async Task NotifyCatalogChange_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAfterOutputVideoAndUpdateAndDelete_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");
        string imageDeletedDirectory = Path.Combine(assetsDirectory, "FolderImageDeleted");
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, "Image 9.png");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

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
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Assert.IsFalse(File.Exists(firstFramePath1));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            Folder? imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNull(imageDeletedFolder);

            Folder? imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNull(imageUpdatedFolder);

            Folder? subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNull(subDirFolder);

            Folder? subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNull(subSubDirFolder);

            Folder? videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFrameFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.IsEmpty(assetsInImageUpdatedFolderFromRepositoryByPath);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.IsEmpty(assetsInSubDirFolderFromRepositoryByPath);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.IsEmpty(assetsInSubSubDirFolderFromRepositoryByPath);

            List<Asset> videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.IsEmpty(videoFirstFramesFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4 = _asset4!.WithFolder(rootFolder!);
            _asset2 = _asset2!.WithFolder(imageDeletedFolder!);
            _asset2Temp = _asset2Temp!.WithFolder(imageUpdatedFolder!);
            _asset3Temp = _asset3Temp!.WithFolder(subDirFolder!);
            _asset4Temp = _asset4Temp!.WithFolder(videoFirstFrameFolder!);

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset4Temp!, _asset3Temp!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.AreEqual(1, assetsInImageDeletedFolderFromRepositoryByPath.Count);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(1, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(20, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                _asset4,
                rootFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageDeletedFolder!],
                _asset2,
                imageDeletedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[videoFirstFrameFolder!],
                _asset4Temp!,
                videoFirstFrameFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                subDirDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[subDirFolder!],
                _asset3Temp,
                subDirFolder!,
                ref increment);

            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(22, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[2]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);
            
            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageDeletedDirectory);
            Assert.AreEqual(0, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(subSubDirDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            imageDeletedFolder = _testableAssetRepository!.GetFolderByPath(imageDeletedDirectory);
            Assert.IsNotNull(imageDeletedFolder);

            imageUpdatedFolder = _testableAssetRepository!.GetFolderByPath(imageUpdatedDirectory);
            Assert.IsNotNull(imageUpdatedFolder);

            subDirFolder = _testableAssetRepository!.GetFolderByPath(subDirDirectory);
            Assert.IsNotNull(subDirFolder);

            subSubDirFolder = _testableAssetRepository!.GetFolderByPath(subSubDirDirectory);
            Assert.IsNotNull(subSubDirFolder);

            videoFirstFrameFolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFrameFolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

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

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsInRootFromRepositoryByPath.Count);

            assetsInImageDeletedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.IsEmpty(assetsInImageDeletedFolderFromRepositoryByPath);

            assetsInImageUpdatedFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.AreEqual(1, assetsInImageUpdatedFolderFromRepositoryByPath.Count);

            assetsInSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.AreEqual(1, assetsInSubDirFolderFromRepositoryByPath.Count);

            assetsInSubSubDirFolderFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.AreEqual(0, assetsInSubSubDirFolderFromRepositoryByPath.Count);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.AreEqual(2, videoFirstFramesFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            assetPathsAfterSync = [imagePath1ToCopy, firstFramePath1, imagePath4ToCopy, imagePath3ToCopy, firstFramePath2];
            expectedFolders = [rootFolder!, videoFirstFrameFolder!, subDirFolder!, imageUpdatedFolder!, videoFirstFrameFolder!];
            expectedDirectories = [assetsDirectory, firstFrameVideosDirectory, subDirDirectory, imageUpdatedDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNull(assetsFromRepository[1].ImageData);
            Assert.IsNull(assetsFromRepository[2].ImageData);
            Assert.IsNull(assetsFromRepository[3].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(38, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(catalogChanges, imageDeletedDirectory, assetsDirectory, 1, [], _asset2!, imageDeletedFolder!, false, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeAssetUpdated(
                catalogChanges,
                imageUpdatedDirectory,
                assetsDirectory,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                _asset2Temp,
                imageUpdatedFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, imageUpdatedDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                firstFrameVideosDirectory,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                _asset5Temp!,
                videoFirstFrameFolder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, firstFrameVideosDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, subSubDirDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(40, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[25]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[26]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[27]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[28]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[29]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[30]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[31]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[32]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[33]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[34]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[35]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[36]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[37]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[38]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[39]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.AreEqual(5, folderAddedEvents.Count);
            Assert.AreEqual(imageDeletedFolder, folderAddedEvents[0]);
            Assert.AreEqual(imageUpdatedFolder, folderAddedEvents[1]);
            Assert.AreEqual(videoFirstFrameFolder, folderAddedEvents[2]);
            Assert.AreEqual(subDirFolder, folderAddedEvents[3]);
            Assert.AreEqual(subSubDirFolder, folderAddedEvents[4]);

            Assert.IsEmpty(folderRemovedEvents);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.AreEqual(1, _applicationViewModel!.ObservableAssets.Count);

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                0,
                [],
                null!,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                1,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.AreEqual(2, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                2,
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                true);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                1,
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.IsEmpty(_applicationViewModel.ObservableAssets);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                0,
                [],
                null!,
                subSubDirFolder!,
                false);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                1,
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.AreEqual(64, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[40]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[41]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[42]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[43]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[44]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[45]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[46]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[47]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[48]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[49]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[50]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[51]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[52]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[53]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[54]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[55]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[56]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[57]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[58]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[59]);
            Assert.AreEqual("CurrentFolder", notifyPropertyChangedEvents[60]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[61]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[62]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[63]);
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
    public async Task NotifyCatalogChange_NoAssetsAndRootCatalogFolderExists_NotifiesNoAssetChanges(int catalogBatchSize)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(_defaultAssetsDirectory!);

            Directory.CreateDirectory(_defaultAssetsDirectory!);

            string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsInDirectory);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, true, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            Assert.AreEqual(5, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_NoAssetsAndRootCatalogFolderDoesNotExist_NotifiesNoAssetChanges(int catalogBatchSize)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(_defaultAssetsDirectory!);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, false, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderDeleted(catalogChanges, 0, foldersInRepository.Length, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            Assert.AreEqual(5, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);

            Assert.AreEqual(1, folderRemovedEvents.Count);
            Assert.AreEqual(_defaultAssetsDirectory, folderRemovedEvents[0].Path);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task NotifyCatalogChange_RootCatalogFolderPointsToAFile_NotifiesNoAssetChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Image 1.jpg");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository!.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, true, false, folder!);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(5, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderDeleted(catalogChanges, 0, foldersInRepository.Length, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                folder!,
                false);

            Assert.AreEqual(5, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);

            Assert.AreEqual(1, folderRemovedEvents.Count);
            Assert.AreEqual(assetsDirectory, folderRemovedEvents[0].Path);
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
    public async Task NotifyCatalogChange_NoAssetsAndRootCatalogExistAndFolderAndIsCancellationRequested_NotifiesNoAssetChanges(int catalogBatchSize, bool folderExists)
    {
        ConfigureApplicationViewModel(catalogBatchSize, _defaultAssetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(_defaultAssetsDirectory!);

            if (folderExists)
            {
                Directory.CreateDirectory(_defaultAssetsDirectory!);

                string[] assetsInDirectory = Directory.GetFiles(_defaultAssetsDirectory!);
                Assert.IsEmpty(assetsInDirectory);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];
            CancellationToken cancellationToken = new (true);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add, cancellationToken);

            folder = _testableAssetRepository!.GetFolderByPath(_defaultAssetsDirectory!);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(_defaultAssetsDirectory!);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, folder!);

            Assert.IsTrue(_testableAssetRepository.HasChanges()); // SaveCatalog has not been done due to the Cancellation

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

            Assert.AreEqual(4, catalogChanges.Count);

            int increment = 0;

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            Assert.AreEqual(4, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[3]);

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                0,
                [],
                null!,
                folder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndBackupExistsAndSameContent_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(17, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertCurrentAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(13, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(21, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[20]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndBackupExistsAndOneNewAsset_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(17, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

            // Second sync

            File.Copy(imagePath1, destinationFilePathToCopy);

            List<string> assetPathsUpdated = [];
            assetPaths.ForEach(assetPathsUpdated.Add);
            assetPathsUpdated.Add(destinationFilePathToCopy);

            List<int> assetsImageByteSizeUpdated = [];
            assetsImageByteSize.ForEach(assetsImageByteSizeUpdated.Add);
            assetsImageByteSizeUpdated.Add(ASSET1_TEMP_IMAGE_BYTE_SIZE);

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(5, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNotNull(assetsFromRepository[2].ImageData);
            Assert.IsNotNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(15, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                5,
                expectedAssetsUpdated,
                _asset1Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.AreEqual(25, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExistsAndBackupExistsOnDifferentDateAndOneNewAsset_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string destinationFilePathToCopy = Path.Combine(assetsDirectory, _asset1Temp!.FileName);
        string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
        string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, "Image 1_duplicate.jpg");
            string imagePath2 = Path.Combine(assetsDirectory, "Image 9.png");
            string imagePath3 = Path.Combine(assetsDirectory, "Image 9_duplicate.png");
            string imagePath4 = Path.Combine(assetsDirectory, "Image_11.heic");

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize = [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(4, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    i + 1,
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.AreEqual(17, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                4,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);

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
            Assert.AreEqual(5, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(destinationFilePathToCopy));

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);

            List<Asset> expectedAssetsUpdated = [];
            expectedAssets.ForEach(expectedAssetsUpdated.Add);
            expectedAssetsUpdated.Add(_asset1Temp);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(5, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(5, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.IsNotNull(assetsFromRepository[0].ImageData);
            Assert.IsNotNull(assetsFromRepository[1].ImageData);
            Assert.IsNotNull(assetsFromRepository[2].ImageData);
            Assert.IsNotNull(assetsFromRepository[3].ImageData);
            Assert.IsNull(assetsFromRepository[4].ImageData);

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

            Assert.IsFalse(_testableAssetRepository.HasChanges());

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

            Assert.AreEqual(15, catalogChanges.Count);

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                5,
                expectedAssetsUpdated,
                _asset1Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            Assert.IsTrue(File.Exists(oldBackupFilePath));
            Assert.IsTrue(File.Exists(backupFilePath));

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                5,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.AreEqual(25, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[2]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[3]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[4]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[5]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[6]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[7]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[8]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[9]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[10]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[11]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[12]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[13]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[14]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[15]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[16]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[17]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[18]);
            Assert.AreEqual("ObservableAssets", notifyPropertyChangedEvents[19]);
            Assert.AreEqual("AppTitle", notifyPropertyChangedEvents[20]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[21]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[22]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[23]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[24]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                5,
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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
    public void NotifyCatalogChange_NoAssetsAndTokenIsCancelled_NotifiesNoAssetChanges()
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
    public async Task NotifyCatalogChange_AssetsImageAndRootCatalogFolderExistsAndAccessToFolderIsDenied_LogsErrorAndNotifiesNoAssetChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempAssetsDirectory");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);
        LoggingAssertsService loggingAssertsService = new();

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            string imagePath = Path.Combine(_dataDirectory!, "Image 1.jpg");
            string imagePathToCopy = Path.Combine(assetsDirectory, "Image 1.jpg");

            File.Copy(imagePath, imagePathToCopy);

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(imagePathToCopy));

            Folder? rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(rootFolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            DirectoryHelper.DenyAccess(assetsDirectory);

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            rootFolder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(rootFolder);

            _asset2Temp = _asset2Temp!.WithFolder(rootFolder!);

            Assert.IsFalse(_testableAssetRepository!.BackupExists());

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsInRootFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            List<Folder> folders = [rootFolder!];

            CatalogAssetsAsyncAsserts.CheckBlobsAndTablesAfterSaveCatalogEmpty(_database!, _userConfigurationService, blobsPath, tablesPath, false, false, rootFolder!);

            Assert.IsTrue(_testableAssetRepository.HasChanges()); // SaveCatalog has not been done due to the exception

            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            Assert.AreEqual(3, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            UnauthorizedAccessException unauthorizedAccessException = new ($"Access to the path '{assetsDirectory}' is denied.");
            Exception[] expectedExceptions = [unauthorizedAccessException];
            Type typeOfService = typeof(CatalogAssetsService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeException(catalogChanges, unauthorizedAccessException, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                0,
                [],
                null!,
                rootFolder!,
                false);

            Assert.AreEqual(3, notifyPropertyChangedEvents.Count);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[0]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[1]);
            Assert.AreEqual("StatusMessage", notifyPropertyChangedEvents[2]);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                0,
                [],
                null!,
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.IsEmpty(folderAddedEvents);
            Assert.IsEmpty(folderRemovedEvents);
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

    private
        (List<string> notifyPropertyChangedEvents,
        List<ApplicationViewModel> applicationViewModelInstances,
        List<Folder> folderAddedEvents,
        List<Folder> folderRemovedEvents)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<ApplicationViewModel> applicationViewModelInstances = [];

        _applicationViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            applicationViewModelInstances.Add((ApplicationViewModel)sender!);
        };

        List<Folder> folderAddedEvents = [];

        _applicationViewModel.FolderAdded += delegate(object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = [];

        _applicationViewModel.FolderRemoved += delegate(object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private void CheckBeforeNotifyCatalogChanges(string expectedRootDirectory)
    {
        Assert.IsTrue(_applicationViewModel!.SortAscending);
        Assert.IsNull(_applicationViewModel!.Product);
        Assert.IsNull(_applicationViewModel!.Version);
        Assert.IsFalse(_applicationViewModel!.IsRefreshingFolders);
        Assert.AreEqual(AppMode.Thumbnails, _applicationViewModel!.AppMode);
        Assert.AreEqual(SortCriteria.FileName, _applicationViewModel!.SortCriteria);
        Assert.AreEqual(Visibility.Visible, _applicationViewModel!.ThumbnailsVisible);
        Assert.AreEqual(Visibility.Hidden, _applicationViewModel!.ViewerVisible);
        Assert.AreEqual(0, _applicationViewModel!.ViewerPosition);
        Assert.IsEmpty(_applicationViewModel!.SelectedAssets);
        Assert.AreEqual(expectedRootDirectory, _applicationViewModel!.CurrentFolder);
        Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        Assert.IsNull(_applicationViewModel!.GlobaleAssetsCounter);
        Assert.IsNull(_applicationViewModel!.ExecutionTime);
        Assert.IsNull(_applicationViewModel!.TotalFilesNumber);
        Assert.AreEqual($"  - {expectedRootDirectory} - image 1 of 0 - sorted by file name ascending", _applicationViewModel!.AppTitle);
        Assert.IsNull(_applicationViewModel!.StatusMessage);
        Assert.IsNull(_applicationViewModel!.CurrentAsset);
        Assert.IsNull(_applicationViewModel!.LastSelectedFolder); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.IsFalse(_applicationViewModel!.CanGoToPreviousAsset);
        Assert.IsFalse(_applicationViewModel!.CanGoToNextAsset);
    }

    private static void CheckAfterNotifyCatalogChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        IReadOnlyCollection<Asset> expectedObservableAssets,
        Asset expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.IsTrue(applicationViewModelInstance.SortAscending);
        Assert.IsNull(applicationViewModelInstance.Product);
        Assert.IsNull(applicationViewModelInstance.Version);
        Assert.IsFalse(applicationViewModelInstance.IsRefreshingFolders);
        Assert.AreEqual(AppMode.Thumbnails, applicationViewModelInstance.AppMode);
        Assert.AreEqual(SortCriteria.FileName, applicationViewModelInstance.SortCriteria);
        Assert.AreEqual(Visibility.Visible, applicationViewModelInstance.ThumbnailsVisible);
        Assert.AreEqual(Visibility.Hidden, applicationViewModelInstance.ViewerVisible);
        Assert.AreEqual(0, applicationViewModelInstance.ViewerPosition);
        Assert.IsEmpty(applicationViewModelInstance.SelectedAssets);
        Assert.AreEqual(expectedLastDirectoryInspected, applicationViewModelInstance.CurrentFolder);
        Assert.AreEqual(expectedObservableAssets.Count, applicationViewModelInstance.ObservableAssets.Count);
        Assert.IsNull(applicationViewModelInstance.GlobaleAssetsCounter);
        Assert.IsNull(applicationViewModelInstance.ExecutionTime);
        Assert.IsNull(applicationViewModelInstance.TotalFilesNumber);
        Assert.AreEqual($"  - {expectedLastDirectoryInspected} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", applicationViewModelInstance.AppTitle);
        Assert.AreEqual("The catalog process has ended.", applicationViewModelInstance.StatusMessage);

        if (applicationViewModelInstance.CurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }

        Assert.IsNull(applicationViewModelInstance.LastSelectedFolder); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.IsFalse(applicationViewModelInstance.CanGoToPreviousAsset);
        Assert.AreEqual(expectedCanGoToNextAsset, applicationViewModelInstance.CanGoToNextAsset);
    }

    private void NotifyCatalogChangeFolderInspectionInProgress(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.Id == catalogChange.Folder!.Id), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderInspectionInProgress, catalogChange.Reason);
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeFolderInspectionCompleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderInspectionCompleted, catalogChange.Reason);
        Assert.AreEqual($"Folder inspection for {assetsDirectory}, subfolders included, has been completed.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Folder inspection for {assetsDirectory}, subfolders included, has been completed.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeFolderCreated(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.Id == catalogChange.Folder!.Id), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderCreated, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Folder {assetsDirectory} added to catalog.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, foldersCount);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.FolderDeleted, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual($"Folder {assetsDirectory} deleted from catalog.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeAssetCreated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        int expectedAppTitleAssetsCount,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNotNull(catalogChange.Asset);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageData(expectedAsset, currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.AreEqual(CatalogChangeReason.AssetCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
        if (string.Equals(expectedAsset.FullPath, catalogChange.Asset!.FullPath))
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
        }
        else
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
        }

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset.Folder.Path, currentDirectory))
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList());
        }

        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeAssetNotCreated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string currentDirectory,
        int expectedAppTitleAssetsCount,
        IReadOnlyList<Asset> expectedAssets,
        string expectedAssetPath,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        Assert.IsTrue(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData != null));
        Assert.AreEqual(CatalogChangeReason.AssetNotCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAssetPath} not added to catalog (corrupted).", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);

        Assert.AreEqual($"Image {expectedAssetPath} not added to catalog (corrupted).", _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeAssetUpdated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNotNull(catalogChange.Asset);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageData(expectedAsset, currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.AreEqual(CatalogChangeReason.AssetUpdated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} updated in catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
        if (string.Equals(expectedAsset.FullPath, catalogChange.Asset!.FullPath))
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
        }
        else
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
        }

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset.Folder.Path, currentDirectory))
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList());
        }

        Assert.AreEqual($"Image {expectedAsset.FullPath} updated in catalog.", _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAssets.Count} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeAssetDeleted(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        int expectedAppTitleAssetsCount,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        bool isCorrupted,
        ref int increment)
    {
        string expectedStatusMessage = isCorrupted ? $"Image {expectedAsset.FullPath} deleted from catalog (corrupted)." : $"Image {expectedAsset.FullPath} deleted from catalog.";

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.IsNotNull(catalogChange.Asset);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageDataAssetDeleted(currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.AreEqual(CatalogChangeReason.AssetDeleted, catalogChange.Reason);
        Assert.AreEqual(expectedStatusMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset!.Folder.Path, currentDirectory))
        {
            Assert.IsNotNull(catalogChange.Asset!.ImageData);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.IsNull(catalogChange.Asset!.ImageData);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList());
        }

        Assert.AreEqual(expectedStatusMessage, _applicationViewModel!.StatusMessage);
        Assert.AreEqual($"  - {currentDirectory} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending", _applicationViewModel!.AppTitle);
        increment++;
    }

    private void NotifyCatalogChangeBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeReason catalogChangeReason = string.Equals(expectedMessage, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE) ? CatalogChangeReason.BackupCreationStarted : CatalogChangeReason.BackupUpdateStarted;

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(catalogChangeReason, catalogChange.Reason);
        Assert.AreEqual(expectedMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual(expectedMessage, _applicationViewModel!.StatusMessage);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.BackupCompleted, catalogChange.Reason);
        Assert.AreEqual("Backup completed successfully.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("Backup completed successfully.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangesNoBackupChanges(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.NoBackupChangesDetected, catalogChange.Reason);
        Assert.AreEqual("No changes made to the backup.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("No changes made to the backup.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.CatalogProcessEnded, catalogChange.Reason);
        Assert.AreEqual("The catalog process has ended.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("The catalog process has ended.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private void NotifyCatalogChangeException(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, Exception exceptionExpected, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(CatalogChangeReason.CatalogProcessFailed, catalogChange.Reason);
        Assert.AreEqual("The catalog process has failed.", catalogChange.Message);
        Assert.IsNotNull(catalogChange.Exception);
        Assert.AreEqual(exceptionExpected.Message, catalogChange.Exception!.Message);
        Assert.AreEqual(exceptionExpected.GetType(), catalogChange.Exception.GetType());

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.AreEqual("The catalog process has failed.", _applicationViewModel!.StatusMessage);
        increment++;
    }

    private static void AssertCataloguedAssetsByPathPropertyValidity(IReadOnlyList<Asset> expectedAssets, CatalogChangeCallbackEventArgs catalogChange, int cataloguedAssetsByPathCount)
    {
        for (int i = 0; i < cataloguedAssetsByPathCount; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
                catalogChange.CataloguedAssetsByPath[i],
                currentExpectedAsset,
                currentExpectedAsset.FullPath,
                currentExpectedAsset.Folder.Path,
                currentExpectedAsset.Folder);
        }
    }

    private static void AssertCataloguedAssetsByPathImageData(
        Asset expectedNewAsset,
        string currentDirectory,
        CatalogChangeCallbackEventArgs catalogChange,
        int cataloguedAssetsByPathCount)
    {
        if (cataloguedAssetsByPathCount > 0)
        {
            // The ImageData of the last current has not been loaded yet
            for (int i = 0; i < cataloguedAssetsByPathCount - 1; i++)
            {
                Asset currentCataloguedAssetsByPath = catalogChange.CataloguedAssetsByPath[i];

                // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
                if (string.Equals(currentCataloguedAssetsByPath.FullPath, expectedNewAsset.FullPath))
                {
                    Assert.IsNull(currentCataloguedAssetsByPath.ImageData);
                }
                else if (!string.Equals(currentDirectory, currentCataloguedAssetsByPath.Folder.Path)) // All assets in other directories have ImageData null
                {
                    Assert.IsNull(currentCataloguedAssetsByPath.ImageData);
                }
                else
                {
                    Assert.IsNotNull(currentCataloguedAssetsByPath.ImageData);
                }
            }

            Assert.IsNull(catalogChange.CataloguedAssetsByPath[^1].ImageData);
        }
    }

    private static void AssertCataloguedAssetsByPathImageDataAssetDeleted(
        string currentDirectory,
        CatalogChangeCallbackEventArgs catalogChange,
        int cataloguedAssetsByPathCount)
    {
        if (cataloguedAssetsByPathCount > 0 && string.Equals(currentDirectory, catalogChange.CataloguedAssetsByPath[0].Folder.Path))
        {
            Assert.IsTrue(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData != null));
        }
        else
        {
            Assert.IsTrue(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData == null)); 
        }
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.IsNotNull(asset.ImageData); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void AssertObservableAssets(string currentDirectory, IReadOnlyList<Asset> expectedAssets, IReadOnlyList<Asset> observableAssets)
    {
        Assert.AreEqual(expectedAssets.Count, observableAssets.Count);

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAssets = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAssets, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            if (string.Equals(currentObservableAssets.Folder.Path, currentDirectory))
            {
                Assert.IsNotNull(currentObservableAssets.ImageData);
            }
            else
            {
                Assert.IsNull(currentObservableAssets.ImageData);
            }
        }
    }

    private static void CheckInstance(
        IReadOnlyList<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        IReadOnlyCollection<Asset> expectedObservableAssets,
        Asset expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;
        Assert.AreEqual(applicationViewModelInstances[0], applicationViewModelInstances[applicationViewModelInstancesCount - 2]);
        // No need to go deeper same instance because ref updated each time
        Assert.AreEqual(applicationViewModelInstances[applicationViewModelInstancesCount - 2], applicationViewModelInstances[applicationViewModelInstancesCount - 1]);

        CheckAfterNotifyCatalogChanges(
            applicationViewModelInstances[0],
            expectedLastDirectoryInspected,
            expectedAppTitleAssetsCount,
            expectedObservableAssets,
            expectedCurrentAsset,
            expectedFolder,
            expectedCanGoToNextAsset);
    }

    private void GoToFolderEmulation(string assetsDirectory)
    {
        _applicationViewModel!.CurrentFolder = assetsDirectory;
        Asset[] assets = _application!.GetAssets(assetsDirectory);
        _applicationViewModel!.SetAssets(assets);
    }
}
