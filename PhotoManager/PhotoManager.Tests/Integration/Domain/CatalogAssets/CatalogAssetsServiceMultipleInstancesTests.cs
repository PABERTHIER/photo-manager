using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ImageByteSizes = PhotoManager.Tests.Integration.Constants.ImageByteSizes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.Domain.CatalogAssets;

[TestFixture]
public class CatalogAssetsServiceMultipleInstancesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private string? _databaseBackupPath;
    // private string? _defaultAssetsDirectory;

    private CatalogAssetsService? _catalogAssetsService;
    private BlobStorage? _blobStorage;
    private Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestableAssetRepository? _testableAssetRepository;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    // private Asset? _asset1Temp;
    private Asset? _asset2Temp;
    private Asset? _asset3Temp;
    private Asset? _asset4Temp;
    private Asset? _asset5Temp;

    private const int ASSET1_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_1_DUPLICATE_JPG;
    private const int ASSET2_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_9_PNG;
    private const int ASSET3_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_9_DUPLICATE_PNG;
    private const int ASSET4_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_11_HEIC;

    // private const int ASSET1_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_1_DUPLICATE_COPIED_JPG;
    private const int ASSET2_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_1_JPG;
    private const int ASSET3_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.HOMER_GIF;
    private const int ASSET4_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.HOMER_JPG;
    private const int ASSET5_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.HOMER_DUPLICATED_JPG;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
        _databaseBackupPath = Path.Combine(_databaseDirectory, Constants.DATABASE_BACKUP_END_PATH);
        // _defaultAssetsDirectory = Path.Combine(_dataDirectory, Directories.DEFAULT_ASSETS);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath);

        _blobStorage = new();
        _database = new(new ObjectListStorage(), _blobStorage, new BackupStorage());
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
        // _asset1Temp = new()
        // {
        //     FolderId = Guid.Empty, // Initialised later
        //     Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
        //     FileName = FileNames.IMAGE_1_DUPLICATE_COPIED_JPG,
        //     Pixel = new()
        //     {
        //         Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_COPIED_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_COPIED_JPG },
        //         Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_COPIED_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_COPIED_JPG }
        //     },
        //     FileProperties = new()
        //     {
        //         Size = FileSize.IMAGE_1_DUPLICATE_COPIED_JPG,
        //         Creation = DateTime.Now,
        //         Modification = ModificationDate.Default
        //     },
        //     ThumbnailCreationDateTime = DateTime.Now,
        //     ImageRotation = Rotation.Rotate0,
        //     Hash = Hashes.IMAGE_1_DUPLICATE_COPIED_JPG,
        //     Metadata = new()
        //     {
        //         Corrupted = new() { IsTrue = false, Message = null },
        //         Rotated = new() { IsTrue = false, Message = null }
        //     }
        // };
        _asset2Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
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
        _asset3Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.HOMER_GIF,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.HOMER_GIF, Height = PixelHeightAsset.HOMER_GIF },
                Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_GIF, Height = ThumbnailHeightAsset.HOMER_GIF }
            },
            FileProperties = new()
            {
                Size = FileSize.HOMER_GIF,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.HOMER_GIF,
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
            FileName = FileNames.HOMER_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.HOMER_JPG, Height = PixelHeightAsset.HOMER_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_JPG, Height = ThumbnailHeightAsset.HOMER_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.HOMER_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.HOMER_JPG,
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
            FileName = FileNames.HOMER_DUPLICATED_JPG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.HOMER_DUPLICATED_JPG,
                    Height = PixelHeightAsset.HOMER_DUPLICATED_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.HOMER_DUPLICATED_JPG, Height = ThumbnailHeightAsset.HOMER_DUPLICATED_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.HOMER_DUPLICATED_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.HOMER_DUPLICATED_JPG,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureCatalogAssetService(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
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
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        _catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
    }

    // TODO: Do same tests as CatalogAssetsServiceTests but with multiple instances instead of one
    [Test]
    [Ignore("Tests about two instances will be written later")]
    public async Task
        CatalogAssetsAsync_AssetsImageAndVideosAndRootCatalogFolderExistsAndSubDirAndUpdateAndDeleteTwoInstances_SyncTheAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

        try
        {
            Directory.CreateDirectory(assetsDirectory);

            string imageDeletedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_DELETED);
            string imageUpdatedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_UPDATED);
            string subDirDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_SUB_DIR);
            string subSubDirDirectory = Path.Combine(subDirDirectory, Directories.FOLDER_SUB_SUB_DIR);

            Directory.CreateDirectory(imageDeletedDirectory);
            Directory.CreateDirectory(imageUpdatedDirectory);
            Directory.CreateDirectory(subDirDirectory);
            Directory.CreateDirectory(subSubDirDirectory);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath4 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
            string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, FileNames.IMAGE_1_JPG);
            string imagePath4ToCopy = Path.Combine(subDirDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, FileNames.HOMER_DUPLICATED_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);
            string firstFramePath2 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_DUPLICATED_JPG);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);
            File.Copy(imagePath3, imagePath3ToCopy);
            File.Copy(imagePath4, imagePath4ToCopy);
            File.Copy(videoPath1, videoPath1ToCopy);
            File.Copy(videoPath1, videoPath2ToCopy);

            List<string> assetPaths =
            [
                imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, videoPath1ToCopy,
                videoPath2ToCopy
            ];
            List<string> assetPathsAfterSync =
            [
                imagePath1ToCopy, imagePath2ToCopy, imagePath3ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2
            ];

            List<int> assetsImageByteSizeFirstSync =
            [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET2_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE
            ];
            List<int> assetsImageByteSizeSecondSync =
            [
                ASSET4_IMAGE_BYTE_SIZE,
                ASSET3_TEMP_IMAGE_BYTE_SIZE,
                ASSET4_TEMP_IMAGE_BYTE_SIZE,
                ASSET5_TEMP_IMAGE_BYTE_SIZE,
                ASSET2_TEMP_IMAGE_BYTE_SIZE
            ];

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

            string blobsPath = Path.Combine(_databasePath!,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsInRootFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageDeletedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInImageUpdatedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> assetsInSubSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            List<Asset> videoFirstFramesFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
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

            List<Asset> expectedAssetsFirstSync =
                [_asset4!, _asset2!, _asset2Temp!, _asset3Temp!, _asset4Temp!, _asset5Temp!];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsInRootFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsInRootFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageDeletedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInImageUpdatedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            videoFirstFramesFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(6));

            List<Folder> expectedFolders =
            [
                rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!,
                videoFirstFrameFolder!
            ];
            List<string> expectedDirectories =
            [
                assetsDirectory, imageDeletedDirectory, imageUpdatedDirectory, subDirDirectory,
                firstFrameVideosDirectory, firstFrameVideosDirectory
            ];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i],
                    expectedAssetsFirstSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            List<Folder> folders =
            [
                rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, subSubDirFolder!,
                videoFirstFrameFolder!
            ];
            List<Folder> foldersContainingAssetsFirstSync =
                [rootFolder!, imageDeletedFolder!, imageUpdatedFolder!, subDirFolder!, videoFirstFrameFolder!];
            Dictionary<Folder, List<Asset>> folderToAssetsMappingFirstSync = new()
            {
                { rootFolder!, [_asset4!] },
                { imageDeletedFolder!, [_asset2!] },
                { imageUpdatedFolder!, [_asset2Temp!] },
                { subDirFolder!, [_asset3Temp!] },
                { videoFirstFrameFolder!, [_asset4Temp!, _asset5Temp!] }
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

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingFirstSync,
                foldersContainingAssetsFirstSync, thumbnails, assetsImageByteSizeFirstSync);
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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory,
                folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory,
                folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory,
                folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory,
                folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, firstFrameVideosDirectory, ref increment);

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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges,
                CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second Sync "after closing the app", so new instance

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, true);

            thumbnails = _testableAssetRepository!.GetThumbnails();

            List<Folder> foldersContainingAssetsSecondSync =
                [imageDeletedFolder!, imageUpdatedFolder!, videoFirstFrameFolder!];

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
                { rootFolder!, [_asset4!] },
                { subDirFolder!, [_asset3Temp!] },
                { videoFirstFrameFolder!, [_asset4Temp!, _asset5Temp!] },
                { imageUpdatedFolder!, [_asset2Temp!] }
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

            assetsInImageDeletedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageDeletedDirectory);
            Assert.That(assetsInImageDeletedFolderFromRepositoryByPath, Is.Empty);

            assetsInImageUpdatedFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(imageUpdatedDirectory);
            Assert.That(assetsInImageUpdatedFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subDirDirectory);
            Assert.That(assetsInSubDirFolderFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsInSubSubDirFolderFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(subSubDirDirectory);
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            videoFirstFramesFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            assetPathsAfterSync =
                [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, firstFramePath2, imagePath3ToCopy];
            expectedFolders =
                [rootFolder!, subDirFolder!, videoFirstFrameFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories =
            [
                assetsDirectory, subDirDirectory, firstFrameVideosDirectory, firstFrameVideosDirectory,
                imageUpdatedDirectory
            ];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i],
                    expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMappingSecondSync,
                foldersContainingAssetsSecondSync, thumbnails, assetsImageByteSizeSecondSync);
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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, assetsDirectory,
                folderToAssetsMappingFirstSync[rootFolder!], _asset4!, rootFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageDeletedDirectory,
                folderToAssetsMappingFirstSync[imageDeletedFolder!], _asset2!, imageDeletedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, imageUpdatedDirectory,
                folderToAssetsMappingFirstSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetAdded(catalogChanges, subDirDirectory,
                folderToAssetsMappingFirstSync[subDirFolder!], _asset3Temp!, subDirFolder!, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subSubDirDirectory, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderAdded(catalogChanges, folders.Count, foldersInRepository,
                firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, firstFrameVideosDirectory, ref increment);

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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges,
                CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second part (second sync)
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, assetsDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageDeletedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetDeleted(catalogChanges, imageDeletedDirectory, [],
                _asset2!, imageDeletedFolder!, false, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, imageUpdatedDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesAssetUpdated(catalogChanges, imageUpdatedDirectory,
                folderToAssetsMappingSecondSync[imageUpdatedFolder!], _asset2Temp!, imageUpdatedFolder!, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, subSubDirDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, folders.Count,
                foldersInRepository, firstFrameVideosDirectory, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges,
                CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
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
    [Ignore(
        "Issue because it kept the previous directory and assets still exist in there, need to find a solution to handle this case")]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CatalogAssetsAsync_AssetsAndRootCatalogFolderExistsAndSyncTwoDifferentDirectories_SyncTheAssets(
        bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        try
        {
            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

            List<string> assetPaths = [imagePath1, imagePath2, imagePath3, imagePath4];
            List<int> assetsImageByteSize =
                [ASSET1_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE, ASSET3_IMAGE_BYTE_SIZE, ASSET4_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory, Has.Length.EqualTo(4));

            foreach (string assetPath in assetPaths)
            {
                Assert.That(File.Exists(assetPath), Is.True);
            }

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Null);

            string blobsPath = Path.Combine(_databasePath!,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            string tablesPath = Path.Combine(_databasePath!,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables);

            string backupFileName = DateTime.Now.Date.ToString("yyyyMMdd") + ".zip";
            string backupFilePath = Path.Combine(_databaseBackupPath!, backupFileName);
            CatalogAssetsAsyncAsserts.CheckBackupBefore(_testableAssetRepository, backupFilePath);

            List<Asset> assetsFromRepositoryByPath =
                _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i],
                    expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, expectedAssets } };
            Dictionary<string, int> assetNameToByteSizeMapping = new()
            {
                { _asset1!.FileName, ASSET1_IMAGE_BYTE_SIZE },
                { _asset2!.FileName, ASSET2_IMAGE_BYTE_SIZE },
                { _asset3!.FileName, ASSET3_IMAGE_BYTE_SIZE },
                { _asset4!.FileName, ASSET4_IMAGE_BYTE_SIZE }
            };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!],
                thumbnails, assetsImageByteSize);
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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository,
                assetsDirectory, ref increment);

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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges,
                CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            // Second Sync "after closing the app" to change the directory, so new instance

            assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_1);

            ConfigureCatalogAssetService(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

            imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidityAndImageData(assetsFromRepository[i],
                    expectedAssets[i], assetPaths[i], assetsDirectory, folder!);
            }

            folderToAssetsMapping = new() { { folder!, expectedAssets } };
            assetNameToByteSizeMapping = new() { { _asset2Temp!.FileName, ASSET1_IMAGE_BYTE_SIZE } };

            CatalogAssetsAsyncAsserts.AssertThumbnailsValidity(assetsFromRepository, folderToAssetsMapping, [folder!],
                thumbnails, assetsImageByteSize);
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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository,
                assetsDirectory, ref increment);

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

            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesBackup(catalogChanges,
                CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);

            CatalogAssetsAsyncAsserts.CheckCatalogChangesInspectingFolder(catalogChanges, 1, foldersInRepository,
                assetsDirectory,
                ref increment); // Keep the previous events + new sync but same content so no new asset added
            CatalogAssetsAsyncAsserts.CheckCatalogChangesFolderInspected(catalogChanges, assetsDirectory,
                ref increment);
            CatalogAssetsAsyncAsserts.CheckCatalogChangesEnd(catalogChanges, ref increment);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
