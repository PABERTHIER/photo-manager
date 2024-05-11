using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;
using System.IO.Compression;

namespace PhotoManager.Tests.Integration.Domain.CatalogAssets;

[TestFixture]
public class CatalogAssetsServiceMultipleInstancesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";
    private string? _databaseBackupPath;
    private const string DATABASE_BACKUP_END_PATH = "v1.0_Backups";
    private string? _defaultAssetsDirectory;
    private const string FFMPEG_PATH = "E:\\ffmpeg\\bin\\ffmpeg.exe"; // TODO: Will be removed when the dll of Ffmpeg would have been generated and stored in the project

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

    private const string CREATING_BACKUP_MESSAGE = "Creating catalog backup...";
    private const string UPDATING_BACKUP_MESSAGE = "Updating catalog backup...";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
        _databaseBackupPath = Path.Combine(_databaseDirectory, DATABASE_BACKUP_END_PATH);
        _defaultAssetsDirectory = Path.Combine(_dataDirectory, "Path");

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _blobStorage = new();
        _database = new (new ObjectListStorage(), _blobStorage, new BackupStorage());
    }

    [SetUp]
    public void Setup()
    {
        _asset1 = new()
        {
            FileName = "Image 1_duplicate.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset2 = new()
        {
            FileName = "Image 9.png",
            FileSize = 126277,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset3 = new()
        {
            FileName = "Image 9_duplicate.png",
            FileSize = 126277,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4 = new()
        {
            FileName = "Image_11.heic",
            FileSize = 1411940,
            PixelHeight = 4032,
            PixelWidth = 3024,
            ThumbnailPixelWidth = 112,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2024, 03, 24, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset1Temp = new()
        {
            FileName = "Image 1_duplicate_copied.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset2Temp = new()
        {
            FileName = "Image 1.jpg",
            FileSize = 29857,
            PixelHeight = 720,
            PixelWidth = 1280,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = new DateTime(2023, 01, 07, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset3Temp = new()
        {
            FileName = "Homer.gif",
            FileSize = 64123,
            PixelHeight = 320,
            PixelWidth = 320,
            ThumbnailPixelWidth = 150,
            ThumbnailPixelHeight = 150,
            ThumbnailCreationDateTime = new DateTime(2023, 08, 05, 00, 00, 00),
            ImageRotation = Rotation.Rotate0,
            Hash = "c48b1f61f3a3a004f425d8493d30a50ae14408ed4c5354bf4d0ca40069f91951381a7df32ee7455a6edef0996c95571557a9993021331ff2dfbc3ccc7f0c8ff1",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset4Temp = new()
        {
            FileName = "Homer.jpg",
            FileSize = 6599,
            PixelHeight = 180,
            PixelWidth = 320,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
        };
        _asset5Temp = new()
        {
            FileName = "HomerDuplicated.jpg",
            FileSize = 6599,
            PixelHeight = 180,
            PixelWidth = 320,
            ThumbnailPixelWidth = 200,
            ThumbnailPixelHeight = 112,
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "59742f2cd31c0997be96f9e758799d975f5918f7732f351d66280a708681ea74ccbfa1b61a327835a3f1dbb5ea5f9989484764a10f56f7dd6f32f7b24e286d66",
            IsAssetCorrupted = false,
            AssetCorruptedMessage = null,
            IsAssetRotated = false,
            AssetRotatedMessage = null
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.FFMPEG_PATH, FFMPEG_PATH);

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new (storageService);
        _catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
    }

    // TODO: Do same tests as CatalogAssetsServiceCatalogAssetsAsyncTests but with multiple instances instead of one
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

            List<Asset> expectedAssetsFirstSync = [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!, _asset5Temp!];
            List<Asset> expectedAssetsSecondSync = [_asset4!, _asset3Temp!, _asset4Temp!, _asset5Temp!, _asset2Temp!];

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

            Folder? videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNull(videoFirstFramefolder);

            string blobsPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CheckBackupBefore(backupFilePath);

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

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

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

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2!.Folder = imageDeletedFolder!;
            _asset2!.FolderId = imageDeletedFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;
            _asset5Temp!.Folder = videoFirstFramefolder;
            _asset5Temp!.FolderId = videoFirstFramefolder.FolderId;

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

            List<Folder> expectedFolders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder, videoFirstFramefolder];
            List<string> expectedDirectories = [assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, subSubDirFolder!, videoFirstFramefolder];
            List<Folder> foldersContainingAssets = [rootFolder, imageDeletedFolder, imageUpdatedFolder, subDirFolder, videoFirstFramefolder];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder, [_asset4!]},
                { imageDeletedFolder, [_asset2!]},
                { imageUpdatedFolder, [_asset2Temp!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!, _asset5Temp!]}
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

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync, foldersContainingAssets, thumbnails, assetsImageByteSizeFirstSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingFirstSync,
                assetNameToByteSizeMappingFirstSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
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

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder], _asset2!, imageDeletedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMappingFirstSync[videoFirstFramefolder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFramefolder],
                    expectedAsset,
                    videoFirstFramefolder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
            
            // Second Sync "after closing the app", so new instance

            File.Delete(imagePath2ToCopy);

            _asset2Temp!.ThumbnailCreationDateTime = DateTime.Now.AddDays(10);
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.ThumbnailCreationDateTime);

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

            thumbnails = _testableAssetRepository!.GetThumbnails();

            Dictionary<Folder, List<Asset>> folderToAssetsMappingSecondSync = new()
            {
                { rootFolder, [_asset4!]},
                { subDirFolder, [_asset3Temp!]},
                { videoFirstFramefolder, [_asset4Temp!, _asset5Temp!]},
                { imageUpdatedFolder, [_asset2Temp!]}
            };
            Dictionary<string, int> assetNameToByteSizeMappingSecondSync = new()
            {
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE },
                { _asset3Temp!.FileName, ASSET3_TEMP_IMAGE_BYTE_SIZE },
                { _asset4Temp!.FileName, ASSET4_TEMP_IMAGE_BYTE_SIZE },
                { _asset5Temp!.FileName, ASSET5_TEMP_IMAGE_BYTE_SIZE },
                { _asset2Temp!.FileName, ASSET2_TEMP_IMAGE_BYTE_SIZE }
            };

            // Because stored in the DB and null is converted into an empty string
            _asset4!.AssetCorruptedMessage = "";
            _asset4!.AssetRotatedMessage = "";
            _asset3Temp!.AssetCorruptedMessage = "";
            _asset3Temp!.AssetRotatedMessage = "";
            _asset4Temp!.AssetCorruptedMessage = "";
            _asset4Temp!.AssetRotatedMessage = "";
            _asset5Temp!.AssetCorruptedMessage = "";
            _asset5Temp!.AssetRotatedMessage = "";
            _asset2Temp!.AssetCorruptedMessage = "";
            _asset2Temp!.AssetRotatedMessage = "";

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

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

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

            videoFirstFramefolder = _testableAssetRepository!.GetFolderByPath(firstFrameVideosDirectory);
            Assert.IsNotNull(videoFirstFramefolder);

            assetsInDirectory = Directory.GetFiles(firstFrameVideosDirectory);
            Assert.AreEqual(2, assetsInDirectory.Length);
            Assert.IsTrue(File.Exists(firstFramePath1));
            Assert.IsTrue(File.Exists(firstFramePath2));

            _asset4!.Folder = rootFolder!;
            _asset4!.FolderId = rootFolder!.FolderId;
            _asset2Temp!.Folder = imageUpdatedFolder!;
            _asset2Temp!.FolderId = imageUpdatedFolder!.FolderId;
            _asset3Temp!.Folder = subDirFolder!;
            _asset3Temp!.FolderId = subDirFolder!.FolderId;
            _asset4Temp!.Folder = videoFirstFramefolder!;
            _asset4Temp!.FolderId = videoFirstFramefolder!.FolderId;
            _asset5Temp!.Folder = videoFirstFramefolder;
            _asset5Temp!.FolderId = videoFirstFramefolder.FolderId;

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
            expectedFolders = [rootFolder, subDirFolder, videoFirstFramefolder, videoFirstFramefolder, imageUpdatedFolder];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidityV2(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i], true);
            }

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync, foldersContainingAssets, thumbnails, assetsImageByteSizeSecondSync);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                folders,
                foldersContainingAssets,
                assetsFromRepository,
                folderToAssetsMappingSecondSync,
                assetNameToByteSizeMappingSecondSync);

            Assert.AreEqual(33, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory, folderToAssetsMappingFirstSync[rootFolder], _asset4!, rootFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory, folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingFirstSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory, folderToAssetsMappingFirstSync[subDirFolder], _asset3Temp!, subDirFolder, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);

            CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMappingFirstSync[videoFirstFramefolder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    firstFrameVideosDirectory,
                    folderToAssetsMappingFirstSync[videoFirstFramefolder],
                    expectedAsset,
                    videoFirstFramefolder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [], _asset2!, imageDeletedFolder!, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, imageUpdatedDirectory, ref increment);
            CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder], _asset2Temp!, imageUpdatedFolder, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, subSubDirDirectory, ref increment);
            CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count, foldersInRepository, firstFrameVideosDirectory, ref increment);
            CheckCatalogChangesBackup(catalogChanges, UPDATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);
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
            List<Asset> expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!];
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
            CheckBackupBefore(backupFilePath);

            List<Asset> assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            List<Asset> assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.IsEmpty(assetsFromRepository);

            Dictionary<string, Dictionary<string, byte[]>> thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            CheckBlobsAndTablesBeforeSaveCatalog(blobsPath, tablesPath);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            List<CatalogChangeCallbackEventArgs> catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset1!.Folder = folder!;
            _asset1!.FolderId = folder!.FolderId;
            _asset2!.Folder = folder;
            _asset2!.FolderId = folder.FolderId;
            _asset3!.Folder = folder;
            _asset3!.FolderId = folder.FolderId;
            _asset4!.Folder = folder;
            _asset4!.FolderId = folder.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(4, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(9, catalogChanges.Count);

            int increment = 0;

            Folder[] foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second Sync "after closing the app" to change the directory, so new instance

            assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder1");

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

            imagePath1 = Path.Combine(assetsDirectory, "Image 1.jpg");

            assetPaths = [imagePath1];
            expectedAssets = [_asset2Temp!];
            assetsImageByteSize = [ASSET2_TEMP_IMAGE_BYTE_SIZE];

            assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.AreEqual(1, assetsInDirectory.Length);

            foreach (string assetPath in assetPaths)
            {
                Assert.IsTrue(File.Exists(assetPath));
            }

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNull(folder);

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.IsEmpty(assetsFromRepositoryByPath);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(4, assetsFromRepository.Count);

            thumbnails = _testableAssetRepository!.GetThumbnails();
            Assert.IsEmpty(thumbnails);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            catalogChanges = [];

            await _catalogAssetsService!.CatalogAssetsAsync(catalogChanges.Add);

            folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.IsNotNull(folder);

            _asset2Temp!.Folder = folder!;
            _asset2Temp!.FolderId = folder!.FolderId;

            Assert.IsTrue(_testableAssetRepository!.BackupExists());

            assetsFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.AreEqual(1, assetsFromRepositoryByPath.Count);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.AreEqual(1, assetsFromRepository.Count);

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPaths[i], assetsDirectory, folder);
            }

            folderToAssetsMapping = new() { { folder, expectedAssets } };
            assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET1_IMAGE_BYTE_SIZE } };

            AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder], thumbnails, assetsImageByteSize);
            CheckBlobsAndTablesAfterSaveCatalog(
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.IsFalse(_testableAssetRepository.HasChanges());

            CheckBackupAfter(
                backupFilePath,
                blobsPath,
                tablesPath,
                [folder],
                [folder],
                assetsFromRepository,
                folderToAssetsMapping,
                assetNameToByteSizeMapping);

            Assert.AreEqual(12, catalogChanges.Count);

            increment = 0;

            foldersInRepository = _testableAssetRepository!.GetFolders();

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            foreach (Asset expectedAsset in folderToAssetsMapping[folder])
            {
                CheckCatalogChangesAssetAdded(
                    catalogChanges,
                    assetsDirectory,
                    folderToAssetsMapping[folder],
                    expectedAsset,
                    folder,
                    ref increment);
            }

            CheckCatalogChangesBackup(catalogChanges, CREATING_BACKUP_MESSAGE, ref increment);
            CheckCatalogChangesEnd(catalogChanges, ref increment);

            CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private static void CheckBlobsAndTablesBeforeSaveCatalog(string blobsPath, string tablesPath)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.IsEmpty(blobFiles);
        Assert.IsEmpty(tableFiles);

        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
    }

    private void CheckBlobsAndTablesAfterSaveCatalog(
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.AreEqual(foldersContainingAssets.Count, blobFiles.Length);

        foreach (Folder folder in foldersContainingAssets)
        {
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            Assert.IsTrue(File.Exists(blobFilePath));

            List<Asset> assetsFromRepositoryByFolder = assetsFromRepository.Where(x => x.FolderId == folder.FolderId).ToList();

            Dictionary<string, byte[]>? dataRead = _blobStorage!.ReadFromBinaryFile(blobFilePath);
            Assert.IsNotNull(dataRead);
            Assert.AreEqual(assetsFromRepositoryByFolder.Count, dataRead!.Count);

            for (int j = 0; j < dataRead.Count; j++)
            {
                Assert.IsTrue(dataRead.ContainsKey(assetsFromRepositoryByFolder[j].FileName));
                Assert.IsTrue(assetNameToByteSizeMapping.ContainsKey(assetsFromRepositoryByFolder[j].FileName));
                Assert.AreEqual(assetNameToByteSizeMapping[assetsFromRepositoryByFolder[j].FileName], dataRead[assetsFromRepositoryByFolder[j].FileName].Length);
            }
        }

        Assert.AreEqual(4, tableFiles.Length);
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));

        List<Asset> assetsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.AreEqual(assetsFromRepository.Count, assetsFromDatabase.Count);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        foreach (Asset assetFromDatabase in assetsFromDatabase)
        {
            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == assetFromDatabase.FileName && a.FolderId == assetFromDatabase.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            AssertAssetFromDatabaseValidity(assetFromDatabase, expectedAsset, expectedFolder.FolderId);
        }

        Assert.AreEqual(folders.Count, foldersFromDatabase.Count);

        Dictionary<Guid, Folder> foldersById = folders.ToDictionary(f => f.FolderId, f => f);
        Dictionary<Guid, Folder> foldersFromDatabaseById = foldersFromDatabase.ToDictionary(f => f.FolderId, f => f);

        foreach ((Guid folderId, Folder? expectedFolder) in foldersById)
        {
            Folder actualFolder = foldersFromDatabaseById[folderId];

            Assert.AreEqual(expectedFolder.FolderId, actualFolder.FolderId);
            Assert.AreEqual(expectedFolder.Path, actualFolder.Path);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
    }

    private void CheckBlobsAndTablesAfterSaveCatalogEmpty(string blobsPath, string tablesPath, bool hasEmptyTables, bool hasOneFolder, Folder folder)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.IsEmpty(blobFiles);

        if (hasEmptyTables)
        {
            Assert.AreEqual(4, tableFiles.Length);
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
        }
        else
        {
            Assert.IsEmpty(tableFiles);
        }

        List<Asset> assetsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = _database!.ReadObjectList(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.IsEmpty(assetsFromDatabase);

        if (hasOneFolder)
        {
            Assert.AreEqual(1, foldersFromDatabase.Count);
            Assert.AreEqual(folder.FolderId, foldersFromDatabase[0].FolderId);
            Assert.AreEqual(folder.Path, foldersFromDatabase[0].Path);
        }
        else
        {
            Assert.IsEmpty(foldersFromDatabase);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
    }

    private void CheckBackupBefore(string backupFilePath)
    {
        Assert.IsFalse(File.Exists(backupFilePath));
        Assert.IsFalse(_testableAssetRepository!.BackupExists());
    }

    private void CheckBackupAfter(
        string backupFilePath,
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string backupBlobsDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, _databaseBackupPath!);
        Assert.IsTrue(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(_databasePath!);
        string[] backupDirectories = Directory.GetDirectories(_databaseBackupPath!);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(foldersContainingAssets.Count, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(4, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(foldersContainingAssets.Count, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(4, tablesBackup.Length);

        CheckBlobsAndTablesAfterSaveCatalog(
            backupBlobsDirectory,
            backupTablesDirectory,
            folders,
            foldersContainingAssets,
            assetsFromRepository,
            folderToAssetsMapping,
            assetNameToByteSizeMapping);

        Directory.Delete(backupBlobsDirectory, true);
        Directory.Delete(backupTablesDirectory, true);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));
    }

    private void CheckDefaultEmptyBackup(string backupFilePath, string blobsPath, string tablesPath, bool hasEmptyTables, bool hasOneFolder, Folder folder)
    {
        int expectedTablesCount = hasEmptyTables ? 4 : 0;

        string backupBlobsDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(_databaseBackupPath!, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, _databaseBackupPath!);
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(_databasePath!);
        string[] backupDirectories = Directory.GetDirectories(_databaseBackupPath!);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(0, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(expectedTablesCount, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(0, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(expectedTablesCount, tablesBackup.Length);

        CheckBlobsAndTablesAfterSaveCatalogEmpty(backupBlobsDirectory, backupTablesDirectory, hasEmptyTables, hasOneFolder, folder);
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.AreEqual(expectedAsset.FileName, asset.FileName);
        Assert.AreEqual(folder.FolderId, asset.FolderId);
        Assert.AreEqual(folder, asset.Folder);
        Assert.AreEqual(expectedAsset.FileSize, asset.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, asset.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, asset.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, asset.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, asset.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, asset.ImageRotation);
        Assert.AreEqual(actualDate, asset.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, asset.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, asset.IsAssetCorrupted);
        Assert.AreEqual(expectedAsset.AssetCorruptedMessage, asset.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, asset.IsAssetRotated);
        Assert.AreEqual(expectedAsset.AssetRotatedMessage, asset.AssetRotatedMessage);
        Assert.AreEqual(assetPath, asset.FullPath);
        Assert.AreEqual(folderPath, asset.Folder.Path);
        Assert.IsNotNull(asset.ImageData); // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
        Assert.AreEqual(actualDate, asset.FileCreationDateTime.Date); // Because files are generated by tests (thumbnailCreationDateTime is then the FileModificationDateTime value)  
        Assert.AreEqual(expectedAsset.ThumbnailCreationDateTime.Date, asset.FileModificationDateTime.Date);  // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
    }

    // TODO: Merge with above
    private static void AssertAssetPropertyValidityV2(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder, bool folderHasThumbnails)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.AreEqual(expectedAsset.FileName, asset.FileName);
        Assert.AreEqual(folder.FolderId, asset.FolderId);
        Assert.AreEqual(folder, asset.Folder);
        Assert.AreEqual(expectedAsset.FileSize, asset.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, asset.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, asset.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, asset.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, asset.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, asset.ImageRotation);
        Assert.AreEqual(actualDate, asset.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, asset.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, asset.IsAssetCorrupted);
        Assert.AreEqual(expectedAsset.AssetCorruptedMessage, asset.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, asset.IsAssetRotated);
        Assert.AreEqual(expectedAsset.AssetRotatedMessage, asset.AssetRotatedMessage);
        Assert.AreEqual(assetPath, asset.FullPath);
        Assert.AreEqual(folderPath, asset.Folder.Path);

        if (!folderHasThumbnails)
        {
            Assert.IsNotNull(asset.ImageData); // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
        }
        else
        {
            Assert.IsNull(asset.ImageData); // When blobs already generated, ImageData stays null for next instances of the Service (per directory)
        }

        Assert.AreEqual(actualDate, asset.FileCreationDateTime.Date); // Because files are generated by tests (thumbnailCreationDateTime is then the FileModificationDateTime value)  
        Assert.AreEqual(expectedAsset.ThumbnailCreationDateTime.Date, asset.FileModificationDateTime.Date);  // Unlike in CatalogAssetsServiceCreateAssetTests it is set here
    }

    private static void AssertThumbnailsValidity(
        IReadOnlyList<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        IReadOnlyList<Folder> folders,
        Dictionary<string, Dictionary<string, byte[]>> thumbnails,
        IReadOnlyList<int> assetsImageByteSize)
    {
        Assert.AreEqual(folders.Count, thumbnails.Count);

        int thumbnailsTotalCount = 0;

        for (int i = 0; i < thumbnails.Count; i++)
        {
            Assert.IsTrue(thumbnails.ContainsKey(folders[i].Path));
            thumbnailsTotalCount += thumbnails[folders[i].Path].Count;
        }

        Assert.AreEqual(assetsFromRepository.Count, thumbnailsTotalCount);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        for (int i = 0; i < assetsFromRepository.Count; i++)
        {
            Asset currentAsset = assetsFromRepository[i];

            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == currentAsset.FileName && a.FolderId == currentAsset.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            Assert.IsTrue(thumbnails[expectedFolder.Path].ContainsKey(currentAsset.FileName));

            byte[] assetImageByteSize = thumbnails[expectedFolder.Path][currentAsset.FileName];

            Assert.IsNotNull(assetImageByteSize);
            Assert.AreEqual(assetsImageByteSize[i], assetImageByteSize.Length);
        }
    }

    private static void AssertAssetFromDatabaseValidity(Asset assetFromDatabase, Asset expectedAsset, Guid folderId)
    {
        DateTime actualDate = DateTime.Now.Date;
        DateTime minDate = DateTime.MinValue.Date;

        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FileName);
        Assert.AreEqual(folderId, assetFromDatabase.FolderId);
        Assert.IsNull(assetFromDatabase.Folder);  // Not saved in Db, loaded at the runtime
        Assert.AreEqual(expectedAsset.FileSize, assetFromDatabase.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, assetFromDatabase.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, assetFromDatabase.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, assetFromDatabase.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, assetFromDatabase.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, assetFromDatabase.ImageRotation);
        Assert.AreEqual(actualDate, assetFromDatabase.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, assetFromDatabase.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, assetFromDatabase.IsAssetCorrupted);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, assetFromDatabase.IsAssetRotated);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetRotatedMessage);
        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FullPath); // Folder is not saved in Db, loaded at the runtime
        Assert.IsNull(assetFromDatabase.ImageData); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileCreationDateTime.Date); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileModificationDateTime.Date); // Not saved in Db, loaded at the runtime
    }

    private static void CheckCatalogChangesBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual(expectedMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual(string.Empty, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesInspectingFolder(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason); // TODO: Bad reason
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesFolderAdded(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.FolderCreated, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, foldersCount);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.FolderDeleted, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetAdded(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetUpdated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetUpdated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} updated in catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesAssetDeleted(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyCollection<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        Assert.AreEqual(ReasonEnum.AssetDeleted, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    private static void CheckCatalogChangesEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        int baseIncrement = increment;
        for (int i = increment; i < baseIncrement + 2; i++)
        {
            CatalogChangeCallbackEventArgs catalogChange = catalogChanges[i];
            Assert.IsNull(catalogChange.Asset);
            Assert.IsNull(catalogChange.Folder);
            Assert.AreEqual(0, catalogChange.CataloguedAssetsByPath.Count);
            Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
            Assert.AreEqual(string.Empty, catalogChange.Message);
            Assert.IsNull(catalogChange.Exception);
            increment++;
        }
    }

    private static void RemoveDatabaseBackup(
        List<Folder> folders,
        string blobsPath,
        string tablesPath,
        string backupFilePath)
    {

        // Delete all blobs in Blobs directory
        foreach (Folder folder in folders)
        {
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            File.Delete(blobFilePath);
            Assert.IsFalse(File.Exists(blobFilePath));
        }

        // Delete all tables in Tables directory
        string assetsTablePath = Path.Combine(tablesPath, "assets.db");
        string foldersTablePath = Path.Combine(tablesPath, "folders.db");
        string syncAssetsDirectoriesDefinitionsTablePath = Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db");
        string recentTargetPathsTablePath = Path.Combine(tablesPath, "recenttargetpaths.db");

        File.Delete(assetsTablePath);
        File.Delete(foldersTablePath);
        File.Delete(syncAssetsDirectoriesDefinitionsTablePath);
        File.Delete(recentTargetPathsTablePath);

        Assert.IsFalse(File.Exists(assetsTablePath));
        Assert.IsFalse(File.Exists(foldersTablePath));
        Assert.IsFalse(File.Exists(syncAssetsDirectoriesDefinitionsTablePath));
        Assert.IsFalse(File.Exists(recentTargetPathsTablePath));

        // Delete ZIP file in backup
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));
    }
}
