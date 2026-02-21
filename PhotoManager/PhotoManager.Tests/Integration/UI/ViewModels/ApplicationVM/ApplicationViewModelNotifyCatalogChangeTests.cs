using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
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

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelNotifyCatalogChangeTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private string? _databaseBackupPath;
    private string? _defaultAssetsDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private BlobStorage? _blobStorage;
    private Database? _database;

    private Mock<IPathProviderService>? _pathProviderServiceMock;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    private Asset? _asset1Temp;
    private Asset? _asset2Temp;
    private Asset? _asset3Temp;
    private Asset? _asset4Temp;
    private Asset? _asset5Temp;

    private const int ASSET1_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_1_DUPLICATE_JPG;
    private const int ASSET2_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_9_PNG;
    private const int ASSET3_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_9_DUPLICATE_PNG;
    private const int ASSET4_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_11_HEIC;

    private const int ASSET1_TEMP_IMAGE_BYTE_SIZE = ImageByteSizes.IMAGE_1_DUPLICATE_COPIED_JPG;
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
        _defaultAssetsDirectory = Path.Combine(_dataDirectory, Directories.DEFAULT_ASSETS);

        _pathProviderServiceMock = new();
        _pathProviderServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

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
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
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
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
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
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
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
        _asset1Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_COPIED_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_COPIED_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_COPIED_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_COPIED_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_COPIED_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_COPIED_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_COPIED_JPG,
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
                Modification = DateTime.Now
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
                Asset = new() { Width = PixelWidthAsset.HOMER_DUPLICATED_JPG, Height = PixelHeightAsset.HOMER_DUPLICATED_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.HOMER_DUPLICATED_JPG, Height = ThumbnailHeightAsset.HOMER_DUPLICATED_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.HOMER_DUPLICATED_JPG,
                Creation = DateTime.Now,
                Modification = DateTime.Now
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

        _userConfigurationService = new(configurationRootMock.Object);
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _testableAssetRepository = new(_database!, _pathProviderServiceMock!.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_testableAssetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_testableAssetRepository, fileOperationsService, _userConfigurationService);
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService);
        _applicationViewModel = new(_application);
    }

    // ADD SECTION (Start) ------------------------------------------------------------------------------------------------
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task NotifyCatalogChange_AssetsAndRootCatalogFolderExists_NotifiesChanges(bool analyseVideos)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

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

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                expectedAppTitle,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public async Task NotifyCatalogChange_AssetsImageAndVideosAndRootCatalogFolderExists_NotifiesChanges()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);
            string videoPath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_1_S_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);
            string videoPath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_1_S_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(22));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);
            string videoPath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_1_S_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);
            string videoPath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_1_S_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!] } };
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(19));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(2, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, FileNames.IMAGE_1_TEMP_JPG);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
                    folderToAssetsMapping[folder!][..(i + 1)],
                    folderToAssetsMapping[folder!][i],
                    folder!,
                    ref increment);
            }

            NotifyCatalogChangeAssetNotCreated(
                catalogChanges,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                imagePath1ToCopy,
                ref increment);

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(catalogBatchSize, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

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
            CancellationToken cancellationToken = new(canceled);

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

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetUpdated(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(30));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Null);

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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!] } };
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(23));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imagePath1ToCopyTemp = Path.Combine(assetsDirectory, FileNames.IMAGE_1_TEMP_JPG);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {folderToAssetsMappingUpdated[folder!].Count} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(19));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetUpdated(catalogChanges, assetsDirectory, assetsDirectory, expectedAssetsUpdated, _asset1Temp, folder!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(30));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {expectedAssetsUpdated.Count} - sorted by file name ascending",
                expectedAssetsUpdated,
                _asset1Temp,
                folder!,
                false,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Null);

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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMapping[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[videoFirstFrameFolder!],
                folderToAssetsMapping[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string videoPath1 = Path.Combine(_dataDirectory!, FileNames.HOMER_MP4);

            string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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

            Dictionary<Folder, List<Asset>> folderToAssetsMapping = new() { { folder!, [_asset3Temp!, _asset2Temp!] } };
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssets[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing folder
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(2));

            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(firstFrameVideosDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                videoFirstFrameFolder!,
                false);

            GoToFolderEmulation(assetsDirectory);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(23));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMappingFirstSync[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[folder!],
                folderToAssetsMappingFirstSync[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[folder!],
                folderToAssetsMappingFirstSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {expectedAssetsSecondSync.Count} - sorted by file name ascending",
                expectedAssetsSecondSync,
                _asset1Temp,
                folder!,
                false,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[folder!],
                folderToAssetsMappingSecondSync[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[folder!],
                folderToAssetsMappingSecondSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsThirdSync[i], assetPathsAfterSecondSync[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {expectedAssetsThirdSync.Count} - sorted by file name ascending",
                expectedAssetsThirdSync,
                _asset1Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingThirdSync[folder!],
                folderToAssetsMappingThirdSync[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(36));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingThirdSync[folder!],
                folderToAssetsMappingThirdSync[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imagePath1ToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.HOMER_GIF);
            string imagePath2ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_GIF);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {expectedAssetsUpdated.Count} - sorted by file name ascending",
                expectedAssetsUpdated,
                _asset1Temp,
                folder!,
                false,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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

            CancellationToken cancellationToken = new(true);
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

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(24));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string tempDirectory = Path.Combine(assetsDirectory, Directories.TEMP_FOLDER);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder1!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {folderToAssetsMapping[folder1!].Count} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder1!],
                folderToAssetsMapping[folder1!][0],
                folder1!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder1!],
                folderToAssetsMapping[folder1!][0],
                folder1!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                assetsFromRepositoryByPath2,
                _asset1Temp,
                folder2!,
                false,
                ref increment);
            NotifyCatalogChangeFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder1!],
                folderToAssetsMappingUpdated[folder1!][0],
                folder1!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(28));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder1!],
                folderToAssetsMappingUpdated[folder1!][0],
                folder1!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder2));
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string tempDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_TO_DELETE);

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

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePath2 = Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG);
            string destinationFilePathToCopy1 = Path.Combine(tempDirectory, _asset2Temp!.FileName);
            string destinationFilePathToCopy2 = Path.Combine(tempDirectory, _asset2!.FileName);
            File.Copy(imagePath1, destinationFilePathToCopy1);
            File.Copy(imagePath2, destinationFilePathToCopy2);

            List<string> assetPaths = [destinationFilePathToCopy1, destinationFilePathToCopy2];

            List<int> assetsImageByteSizeFirstSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeSecondSync = [ASSET2_TEMP_IMAGE_BYTE_SIZE, ASSET2_IMAGE_BYTE_SIZE];
            List<int> assetsImageByteSizeThirdSync = [ASSET2_IMAGE_BYTE_SIZE];

            string[] assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1, Is.Empty);

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
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderCreated(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Is.Empty);

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
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(2));

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, tempDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Is.Empty);

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
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 2, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [_asset2],
                _asset2Temp,
                folder2!,
                false,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(23));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Fourth sync

            assetsInDirectory1 = Directory.GetFiles(assetsDirectory);
            Assert.That(assetsInDirectory1, Is.Empty);

            await _applicationViewModel!.CatalogAssets(catalogChanges.Add);

            folder1 = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder1, Is.Not.Null);

            Folder? folder2Updated = _testableAssetRepository!.GetFolderByPath(tempDirectory);
            Assert.That(folder2Updated, Is.Null);

            Dictionary<Folder, List<Asset>> folderToAssetsMappingFourthSync = [];
            Dictionary<string, int> assetNameToByteSizeMappingFourthSync = [];

            Assert.That(_testableAssetRepository!.BackupExists(), Is.True);

            assetsFromRepositoryByPath1 = _testableAssetRepository.GetCataloguedAssetsByPath(assetsDirectory);
            Assert.That(assetsFromRepositoryByPath1, Is.Empty);

            assetsFromRepositoryByPath2 = _testableAssetRepository.GetCataloguedAssetsByPath(tempDirectory);
            Assert.That(assetsFromRepositoryByPath2, Is.Empty);

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                tempDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                _asset2,
                folder2!,
                false,
                ref increment);
            NotifyCatalogChangeFolderDeleted(catalogChanges, 1, foldersInRepository.Length, tempDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, tempDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.UPDATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(31));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder1!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder2));

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder2));
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imageDeletedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_DELETED);
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, FileNames.IMAGE_9_PNG);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(23));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subSubDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

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
            Assert.That(assetsInDirectory, Is.Empty);
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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                [],
                _asset2!,
                imageDeletedFolder!,
                false,
                ref increment);
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(40));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[38], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[39], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subSubDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(1));

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageDeletedDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageUpdatedDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subDirDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subSubDirDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                subSubDirFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(2));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                true);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(64));
            Assert.That(notifyPropertyChangedEvents[40], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[41], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[42], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[43], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[44], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[45], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[46], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[47], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[48], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[49], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[51], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[52], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[53], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[54], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[55], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[56], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[57], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[58], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[59], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[60], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[61], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[62], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[63], Is.EqualTo("AppTitle"));
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imageDeletedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_DELETED);
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, FileNames.IMAGE_9_PNG);

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
            string imagePath3ToCopy = Path.Combine(imageUpdatedDirectory, FileNames.IMAGE_1_JPG);
            string imagePath4ToCopy = Path.Combine(subDirDirectory, FileNames.HOMER_GIF);
            string videoPath1ToCopy = Path.Combine(assetsDirectory, FileNames.HOMER_MP4);
            string videoPath2ToCopy = Path.Combine(subSubDirDirectory, FileNames.HOMER_MP4);

            string firstFrameVideosDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;
            string firstFramePath1 = Path.Combine(firstFrameVideosDirectory, FileNames.HOMER_JPG);

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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(22));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subSubDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

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
            Assert.That(assetsInDirectory, Is.Empty);
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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(1));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(4));

            assetPathsAfterSync = [imagePath1ToCopy, imagePath4ToCopy, firstFramePath1, imagePath3ToCopy];
            expectedFolders = [rootFolder!, subDirFolder!, videoFirstFrameFolder!, imageUpdatedFolder!];
            expectedDirectories = [assetsDirectory, subDirDirectory, firstFrameVideosDirectory, imageUpdatedDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                [],
                _asset2!,
                imageDeletedFolder!,
                false,
                ref increment);
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(39));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[38], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subSubDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(videoFirstFrameFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(1));

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageDeletedDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageUpdatedDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subDirDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subSubDirDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                subSubDirFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                false);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(63));
            Assert.That(notifyPropertyChangedEvents[39], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[40], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[41], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[43], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[44], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[45], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[46], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[47], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[49], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[50], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[51], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[52], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[53], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[55], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[56], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[57], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[58], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[59], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[61], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[62], Is.EqualTo("AppTitle"));
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);
        string imageDeletedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_DELETED);
        string imagePath2ToCopy = Path.Combine(imageDeletedDirectory, FileNames.IMAGE_9_PNG);

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

            string imageUpdatedDirectory = Path.Combine(assetsDirectory, Directories.FOLDER_IMAGE_UPDATED);
            string subDirDirectory = Path.Combine(assetsDirectory, Directories.Z_FOLDER_SUB_DIR);
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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(22));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingFirstSync[rootFolder!],
                folderToAssetsMappingFirstSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(videoFirstFrameFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(subSubDirFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Second Sync

            File.Delete(imagePath2ToCopy);

            _asset2Temp.FileProperties = _asset2Temp.FileProperties with { Modification = DateTime.Now.AddDays(10) };
            File.SetLastWriteTime(imagePath3ToCopy, _asset2Temp.FileProperties.Modification);

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
            Assert.That(assetsInSubSubDirFolderFromRepositoryByPath, Is.Empty);

            videoFirstFramesFromRepositoryByPath = _testableAssetRepository.GetCataloguedAssetsByPath(firstFrameVideosDirectory);
            Assert.That(videoFirstFramesFromRepositoryByPath, Has.Count.EqualTo(2));

            assetsFromRepository = _testableAssetRepository.GetCataloguedAssets();
            Assert.That(assetsFromRepository, Has.Count.EqualTo(5));

            assetPathsAfterSync = [imagePath1ToCopy, firstFramePath1, imagePath4ToCopy, imagePath3ToCopy, firstFramePath2];
            expectedFolders = [rootFolder!, videoFirstFrameFolder!, subDirFolder!, imageUpdatedFolder!, videoFirstFrameFolder!];
            expectedDirectories = [assetsDirectory, firstFrameVideosDirectory, subDirDirectory, imageUpdatedDirectory, firstFrameVideosDirectory];

            for (int i = 0; i < assetsFromRepository.Count; i++)
            {
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsSecondSync[i], assetPathsAfterSync[i], expectedDirectories[i], expectedFolders[i]);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, imageDeletedDirectory, ref increment);
            NotifyCatalogChangeAssetDeleted(
                catalogChanges,
                imageDeletedDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                [],
                _asset2!,
                imageDeletedFolder!,
                false,
                ref increment);
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(40));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[38], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[39], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(5));
            Assert.That(folderAddedEvents[0], Is.EqualTo(imageDeletedFolder));
            Assert.That(folderAddedEvents[1], Is.EqualTo(imageUpdatedFolder));
            Assert.That(folderAddedEvents[2], Is.EqualTo(videoFirstFrameFolder));
            Assert.That(folderAddedEvents[3], Is.EqualTo(subDirFolder));
            Assert.That(folderAddedEvents[4], Is.EqualTo(subSubDirFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            // Changing to imageDeletedFolder -> imageDeletedDirectory
            Assert.That(_applicationViewModel!.ObservableAssets, Has.Count.EqualTo(1));

            GoToFolderEmulation(imageDeletedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(imageDeletedDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageDeletedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageDeletedDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                imageDeletedFolder!,
                false);

            // Changing to imageUpdatedFolder -> imageUpdatedDirectory
            GoToFolderEmulation(imageUpdatedDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(imageUpdatedDirectory, folderToAssetsMappingSecondSync[imageUpdatedFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                imageUpdatedDirectory,
                $"PhotoManager {Constants.VERSION} - {imageUpdatedDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[imageUpdatedFolder!],
                folderToAssetsMappingSecondSync[imageUpdatedFolder!][0],
                imageUpdatedFolder!,
                false);

            // Changing to videoFirstFrameFolder -> firstFrameVideosDirectory
            GoToFolderEmulation(firstFrameVideosDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(2));
            AssertObservableAssets(firstFrameVideosDirectory, folderToAssetsMappingSecondSync[videoFirstFrameFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                firstFrameVideosDirectory,
                $"PhotoManager {Constants.VERSION} - {firstFrameVideosDirectory} - image 1 of 2 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!],
                folderToAssetsMappingSecondSync[videoFirstFrameFolder!][0],
                videoFirstFrameFolder!,
                true);

            // Changing to subDirFolder -> subDirDirectory
            GoToFolderEmulation(subDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(subDirDirectory, folderToAssetsMappingSecondSync[subDirFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subDirDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[subDirFolder!],
                folderToAssetsMappingSecondSync[subDirFolder!][0],
                subDirFolder!,
                false);

            // Changing to subSubDirFolder -> subSubDirDirectory
            GoToFolderEmulation(subSubDirDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Is.Empty);
            AssertObservableAssets(subSubDirDirectory, [], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                subSubDirDirectory,
                $"PhotoManager {Constants.VERSION} - {subSubDirDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                subSubDirFolder!,
                false);

            // Changing to rootFolder -> assetsDirectory
            GoToFolderEmulation(assetsDirectory);

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
            AssertObservableAssets(assetsDirectory, folderToAssetsMappingSecondSync[rootFolder!], _applicationViewModel!.ObservableAssets);
            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 1 - sorted by file name ascending",
                folderToAssetsMappingSecondSync[rootFolder!],
                folderToAssetsMappingSecondSync[rootFolder!][0],
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(64));
            Assert.That(notifyPropertyChangedEvents[40], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[41], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[42], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[43], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[44], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[45], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[46], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[47], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[48], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[49], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[51], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[52], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[53], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[54], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[55], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[56], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[57], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[58], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[59], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[60], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[61], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[62], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[63], Is.EqualTo("AppTitle"));
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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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

            NotifyCatalogChangeFolderDeleted(catalogChanges, 0, foldersInRepository.Length, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0].Path, Is.EqualTo(_defaultAssetsDirectory));
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
        string assetsDirectory = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);

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

            NotifyCatalogChangeFolderDeleted(catalogChanges, 0, foldersInRepository.Length, assetsDirectory, ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0].Path, Is.EqualTo(assetsDirectory));
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
            CancellationToken cancellationToken = new(true);

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

            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, _defaultAssetsDirectory!, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _defaultAssetsDirectory!,
                $"PhotoManager {Constants.VERSION} - {_defaultAssetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                folder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeNotifyCatalogChanges(assetsDirectory);

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangesNoBackupChanges(catalogChanges, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(25));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
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

            string imagePath1 = Path.Combine(assetsDirectory, FileNames.IMAGE_1_DUPLICATE_JPG);
            string imagePath2 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_PNG);
            string imagePath3 = Path.Combine(assetsDirectory, FileNames.IMAGE_9_DUPLICATE_PNG);
            string imagePath4 = Path.Combine(assetsDirectory, FileNames.IMAGE_11_HEIC);

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

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment);

            for (int i = 0; i < folderToAssetsMapping[folder!].Count; i++)
            {
                NotifyCatalogChangeAssetCreated(
                    catalogChanges,
                    assetsDirectory,
                    assetsDirectory,
                    $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of {i + 1} - sorted by file name ascending",
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
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending",
                folderToAssetsMapping[folder!],
                folderToAssetsMapping[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

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
                CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(assetsFromRepository[i], expectedAssetsUpdated[i], assetPathsUpdated[i], assetsDirectory, folder!);
            }

            Assert.That(assetsFromRepository[0].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[1].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[2].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[3].ImageData, Is.Not.Null);
            Assert.That(assetsFromRepository[4].ImageData, Is.Null);

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

            foldersInRepository = _testableAssetRepository!.GetFolders();

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, 1, foldersInRepository, assetsDirectory, ref increment); // Keep the previous events + new sync but same content so no new asset added
            NotifyCatalogChangeAssetCreated(
                catalogChanges,
                assetsDirectory,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                expectedAssetsUpdated,
                _asset1Temp,
                folder!,
                ref increment);
            NotifyCatalogChangeFolderInspectionCompleted(catalogChanges, assetsDirectory, ref increment);
            NotifyCatalogChangeBackup(catalogChanges, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            Assert.That(File.Exists(oldBackupFilePath), Is.True);
            Assert.That(File.Exists(backupFilePath), Is.True);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(25));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 5 - sorted by file name ascending",
                folderToAssetsMappingUpdated[folder!],
                folderToAssetsMappingUpdated[folder!][0],
                folder!,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_ASSETS_DIRECTORY);

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

            string imagePath = Path.Combine(_dataDirectory!, FileNames.IMAGE_1_JPG);
            string imagePathToCopy = Path.Combine(assetsDirectory, FileNames.IMAGE_1_JPG);

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

            UnauthorizedAccessException unauthorizedAccessException = new($"Access to the path '{assetsDirectory}' is denied.");
            Exception[] expectedExceptions = [unauthorizedAccessException];
            Type typeOfService = typeof(CatalogAssetsService);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);

            NotifyCatalogChangeFolderInspectionInProgress(catalogChanges, folders.Count, foldersInRepository, assetsDirectory, ref increment);
            NotifyCatalogChangeException(catalogChanges, unauthorizedAccessException, ref increment);
            NotifyCatalogChangeEnd(catalogChanges, ref increment);

            CheckAfterNotifyCatalogChanges(
                _applicationViewModel!,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                rootFolder!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending",
                [],
                null,
                rootFolder!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
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

        _applicationViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            applicationViewModelInstances.Add((ApplicationViewModel)sender!);
        };

        List<Folder> folderAddedEvents = [];

        _applicationViewModel.FolderAdded += delegate (object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = [];

        _applicationViewModel.FolderRemoved += delegate (object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private void CheckBeforeNotifyCatalogChanges(string expectedRootDirectory)
    {
        Assert.That(_applicationViewModel!.SortAscending, Is.True);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.Zero);
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckAfterNotifyCatalogChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        List<Asset> expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.Zero);
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo("The catalog process has ended."));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private void NotifyCatalogChangeFolderInspectionInProgress(List<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(folders, Has.Count.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder, Is.EqualTo(folders.First(x => x.Id == catalogChange.Folder!.Id)));
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderInspectionInProgress));
        Assert.That(catalogChange.Message, Is.EqualTo($"Inspecting folder {assetsDirectory}."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Inspecting folder {assetsDirectory}."));
        increment++;
    }

    private void NotifyCatalogChangeFolderInspectionCompleted(List<CatalogChangeCallbackEventArgs> catalogChanges, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderInspectionCompleted));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder inspection for {assetsDirectory}, subfolders included, has been completed."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Folder inspection for {assetsDirectory}, subfolders included, has been completed."));
        increment++;
    }

    private void NotifyCatalogChangeFolderCreated(List<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(folders, Has.Count.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder, Is.EqualTo(folders.First(x => x.Id == catalogChange.Folder!.Id)));
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder {assetsDirectory} added to catalog."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Folder {assetsDirectory} added to catalog."));
        increment++;
    }

    private void NotifyCatalogChangeFolderDeleted(List<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(foldersCount, Is.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderDeleted));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder {assetsDirectory} deleted from catalog."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Folder {assetsDirectory} deleted from catalog."));
        increment++;
    }

    private void NotifyCatalogChangeAssetCreated(
        List<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        string expectedAppTitle,
        List<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.That(catalogChange.Asset, Is.Not.Null);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageData(expectedAsset, currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAsset.FullPath} added to catalog."));
        Assert.That(catalogChange.Exception, Is.Null);

        // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
        if (string.Equals(expectedAsset.FullPath, catalogChange.Asset!.FullPath))
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Null);
        }
        else
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Not.Null);
        }

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset.Folder.Path, currentDirectory))
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Not.Null);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Null);
            Assert.That(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList(), Is.Empty);
        }

        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Image {expectedAsset.FullPath} added to catalog."));
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo(expectedAppTitle));
        increment++;
    }

    private void NotifyCatalogChangeAssetNotCreated(
        List<CatalogChangeCallbackEventArgs> catalogChanges,
        string currentDirectory,
        string expectedAppTitle,
        List<Asset> expectedAssets,
        string expectedAssetPath,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(cataloguedAssetsByPathCount, Is.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        Assert.That(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData != null), Is.True);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetNotCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAssetPath} not added to catalog (corrupted)."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);

        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Image {expectedAssetPath} not added to catalog (corrupted)."));
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo(expectedAppTitle));
        increment++;
    }

    private void NotifyCatalogChangeAssetUpdated(
        List<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        List<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.That(catalogChange.Asset, Is.Not.Null);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageData(expectedAsset, currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetUpdated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAsset.FullPath} updated in catalog."));
        Assert.That(catalogChange.Exception, Is.Null);

        // Cases when having multiple sync, assets in the firsts sync has ImageData loaded, unlike the new ones (added, updated)
        if (string.Equals(expectedAsset.FullPath, catalogChange.Asset!.FullPath))
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Null);
        }
        else
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Not.Null);
        }

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset.Folder.Path, currentDirectory))
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Not.Null);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Null);
            Assert.That(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList(), Is.Empty);
        }

        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo($"Image {expectedAsset.FullPath} updated in catalog."));
        Assert.That(_applicationViewModel!.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {currentDirectory} - image 1 of {expectedAssets.Count} - sorted by file name ascending"));
        increment++;
    }

    private void NotifyCatalogChangeAssetDeleted(
        List<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        string currentDirectory,
        string expectedAppTitle,
        List<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        bool isCorrupted,
        ref int increment)
    {
        string expectedStatusMessage = isCorrupted ? $"Image {expectedAsset.FullPath} deleted from catalog (corrupted)." : $"Image {expectedAsset.FullPath} deleted from catalog.";

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;

        Assert.That(catalogChange.Asset, Is.Not.Null);
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(cataloguedAssetsByPathCount, Is.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPathPropertyValidity(expectedAssets, catalogChange, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPathImageDataAssetDeleted(currentDirectory, catalogChange, cataloguedAssetsByPathCount);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetDeleted));
        Assert.That(catalogChange.Message, Is.EqualTo(expectedStatusMessage));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);

        // While the user has not clicked on another folder, ImageData stays null for all other assets
        if (string.Equals(catalogChange.Asset!.Folder.Path, currentDirectory))
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Not.Null);
            AssertObservableAssets(currentDirectory, expectedAssets, _applicationViewModel!.ObservableAssets);
        }
        else
        {
            Assert.That(catalogChange.Asset!.ImageData, Is.Null);
            Assert.That(_applicationViewModel!.ObservableAssets.Where(x => string.Equals(x.Folder.Path, catalogChange.Asset.Folder.Path)).ToList(), Is.Empty);
        }

        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(expectedStatusMessage));
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo(expectedAppTitle));
        increment++;
    }

    private void NotifyCatalogChangeBackup(List<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeReason catalogChangeReason = string.Equals(expectedMessage, CatalogAssetsAsyncAsserts.CREATING_BACKUP_MESSAGE) ? CatalogChangeReason.BackupCreationStarted : CatalogChangeReason.BackupUpdateStarted;

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(catalogChangeReason));
        Assert.That(catalogChange.Message, Is.EqualTo(expectedMessage));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(expectedMessage));
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.BackupCompleted));
        Assert.That(catalogChange.Message, Is.EqualTo("Backup completed successfully."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo("Backup completed successfully."));
        increment++;
    }

    private void NotifyCatalogChangesNoBackupChanges(List<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.NoBackupChangesDetected));
        Assert.That(catalogChange.Message, Is.EqualTo("No changes made to the backup."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo("No changes made to the backup."));
        increment++;
    }

    private void NotifyCatalogChangeEnd(List<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessEnded));
        Assert.That(catalogChange.Message, Is.EqualTo("The catalog process has ended."));
        Assert.That(catalogChange.Exception, Is.Null);

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo("The catalog process has ended."));
        increment++;
    }

    private void NotifyCatalogChangeException(List<CatalogChangeCallbackEventArgs> catalogChanges, Exception exceptionExpected, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessFailed));
        Assert.That(catalogChange.Message, Is.EqualTo("The catalog process has failed."));
        Assert.That(catalogChange.Exception, Is.Not.Null);
        Assert.That(catalogChange.Exception!.Message, Is.EqualTo(exceptionExpected.Message));
        Assert.That(catalogChange.Exception.GetType(), Is.EqualTo(exceptionExpected.GetType()));

        _applicationViewModel!.NotifyCatalogChange(catalogChange);
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo("The catalog process has failed."));
        increment++;
    }

    private static void AssertCataloguedAssetsByPathPropertyValidity(List<Asset> expectedAssets, CatalogChangeCallbackEventArgs catalogChange, int cataloguedAssetsByPathCount)
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
                    Assert.That(currentCataloguedAssetsByPath.ImageData, Is.Null);
                }
                else if (!string.Equals(currentDirectory, currentCataloguedAssetsByPath.Folder.Path)) // All assets in other directories have ImageData null
                {
                    Assert.That(currentCataloguedAssetsByPath.ImageData, Is.Null);
                }
                else
                {
                    Assert.That(currentCataloguedAssetsByPath.ImageData, Is.Not.Null);
                }
            }

            Assert.That(catalogChange.CataloguedAssetsByPath[^1].ImageData, Is.Null);
        }
    }

    private static void AssertCataloguedAssetsByPathImageDataAssetDeleted(
        string currentDirectory,
        CatalogChangeCallbackEventArgs catalogChange,
        int cataloguedAssetsByPathCount)
    {
        if (cataloguedAssetsByPathCount > 0 && string.Equals(currentDirectory, catalogChange.CataloguedAssetsByPath[0].Folder.Path))
        {
            Assert.That(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData != null), Is.True);
        }
        else
        {
            Assert.That(catalogChange.CataloguedAssetsByPath.All(asset => asset.ImageData == null), Is.True);
        }
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void AssertObservableAssets(string currentDirectory, List<Asset> expectedAssets, ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Count));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            if (string.Equals(currentObservableAsset.Folder.Path, currentDirectory))
            {
                Assert.That(currentObservableAsset.ImageData, Is.Not.Null);
            }
            else
            {
                Assert.That(currentObservableAsset.ImageData, Is.Null);
            }
        }
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        List<Asset> expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2], Is.EqualTo(applicationViewModelInstances[0]));
        // No need to go deeper, same instance because ref updated each time
        Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1], Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));

        CheckAfterNotifyCatalogChanges(
            applicationViewModelInstances[0],
            expectedLastDirectoryInspected,
            expectedAppTitle,
            expectedAssets,
            expectedCurrentAsset,
            expectedFolder,
            expectedCanGoToNextAsset);
    }

    private void GoToFolderEmulation(string assetsDirectory)
    {
        Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);
        _applicationViewModel!.SetAssets(assetsDirectory, assets);
    }
}
