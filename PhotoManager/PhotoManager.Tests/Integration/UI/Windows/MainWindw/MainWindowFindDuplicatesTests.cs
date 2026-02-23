using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.UI.Windows.MainWindw;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Window
// The goal is to test what does MainWindow
[TestFixture]
public class MainWindowFindDuplicatesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private FolderNavigationViewModel? _folderNavigationViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private event GetExemptedFolderPathEventHandler? GetExemptedFolderPathEvent;
    private event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssetsEvent;
    private event RefreshAssetsCounterEventHandler? RefreshAssetsCounterEvent;

    private event EventTriggeredHandler? GetExemptedFolderPathEventTriggered;
    private event EventTriggeredWithAssetsHandler? DeleteDuplicatedAssetsEventTriggered;
    private event EventTriggeredHandler? RefreshAssetsCounterEventTriggered;

    private delegate void EventTriggeredHandler(object sender);
    private delegate void EventTriggeredWithAssetsHandler(object sender, Asset[] assets);

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset1Temp;

    private Folder? _sourceFolder;

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
        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
            ImageData = new(),
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
    }

    [TearDown]
    public void TearDown()
    {
        _sourceFolder = null;
        _folderNavigationViewModel = null;
        _findDuplicatedAssetsViewModel = null;

        GetExemptedFolderPathEvent = null;
        DeleteDuplicatedAssetsEvent = null;
        RefreshAssetsCounterEvent = null;
        GetExemptedFolderPathEventTriggered = null;
        DeleteDuplicatedAssetsEventTriggered = null;
        RefreshAssetsCounterEventTriggered = null;
    }

    private void ConfigureApplicationViewModel(
        int catalogBatchSize,
        string assetsDirectory,
        string exemptedFolderPath,
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        bool usingDHash,
        bool usingMD5Hash,
        bool usingPHash,
        bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, exemptedFolderPath);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
        _applicationViewModel = new(_application);

        _sourceFolder = new() { Id = Guid.NewGuid(), Path = _applicationViewModel!.CurrentFolderPath };
    }

    [Test]
    public async Task
        FindDuplicates_CataloguedAssetsAndRefreshAndDeleteNotExempted_ShowsDuplicatesHaveBeenFoundAndSendsEvents()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(exemptedFolderPath);

            string imagePath1 = Path.Combine(_dataDirectory!, FileNames.IMAGE_9_PNG);
            string imagePath1ToCopy = Path.Combine(exemptedFolderPath, FileNames.IMAGE_9_PNG);

            File.Copy(imagePath1, imagePath1ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            _asset1Temp = _asset1Temp!.WithFolder(exemptedFolder!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("Duplicates found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Not.Null);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset1Temp,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            DeleteNotExemptedDuplicatedAssets();
            RefreshAssetsCounter();

            CheckFindDuplicatedAssetsViewModel(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(20));
            // CatalogAssets + NotifyCatalogChange
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

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset3);

            Assert.That(refreshAssetsCounterEvents, Has.Count.EqualTo(1));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task FindDuplicates_CataloguedAssetsAndDeleteAll_ShowsDuplicatesHaveBeenFoundAndSendsEvents()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(exemptedFolderPath);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("Duplicates found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Not.Null);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            DeleteAll(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            CheckFindDuplicatedAssetsViewModel(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(19));
            // CatalogAssets + NotifyCatalogChange
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

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task FindDuplicates_CataloguedAssetsAndRefreshAndDelete_ShowsDuplicatesHaveBeenFoundAndSendsEvents()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(exemptedFolderPath);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("Duplicates found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Not.Null);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            RefreshAssetsCounter();
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);
            RefreshAssetsCounter();

            CheckFindDuplicatedAssetsViewModel(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(19));
            // CatalogAssets + NotifyCatalogChange
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

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Has.Count.EqualTo(2));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task FindDuplicates_CataloguedAssetsAndDelete_ShowsDuplicatesHaveBeenFoundAndSendsEvents()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(exemptedFolderPath);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("Duplicates found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Not.Null);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            CheckFindDuplicatedAssetsViewModel(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(19));
            // CatalogAssets + NotifyCatalogChange
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

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public async Task
        FindDuplicates_NoCataloguedAssetsAndRefreshAndDelete_ShowsNoDuplicatesHaveBeenFoundAndDoesNotSendEvents()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("No duplicates have been found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Null);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset2!,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            RefreshAssetsCounter();
            Delete(duplicatedAssetViewModel);
            DeleteAll(duplicatedAssetViewModel);
            DeleteNotExemptedDuplicatedAssets();
            RefreshAssetsCounter();

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Is.Empty);
            Assert.That(deleteDuplicatedAssetsEvents, Is.Empty);
            Assert.That(refreshAssetsCounterEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public async Task FindDuplicates_NoCataloguedAssets_ShowsNoDuplicatesHaveBeenFound()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string exemptedFolderPath = Path.Combine(assetsDirectory, Directories.FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, exemptedFolderPath, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        (
                List<string> getExemptedFolderPathEvents,
                List<Asset[]> deleteDuplicatedAssetsEvents,
                List<string> refreshAssetsCounterEvents
            )
            = NotifyFindDuplicatedAssetsWindowsEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);
            Directory.CreateDirectory(exemptedFolderPath);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? exemptedFolder = _assetRepository!.GetFolderByPath(exemptedFolderPath);
            Assert.That(exemptedFolder, Is.Not.Null);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            string result = FindDuplicates();

            Assert.That(result, Is.EqualTo("No duplicates have been found."));
            Assert.That(_findDuplicatedAssetsViewModel, Is.Null);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                [],
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(exemptedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Is.Empty);
            Assert.That(deleteDuplicatedAssetsEvents, Is.Empty);
            Assert.That(refreshAssetsCounterEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
        }
    }

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

    private
        (List<string> getExemptedFolderPathEvents,
        List<Asset[]> deleteDuplicatedAssetsEvents,
        List<string> refreshAssetsCounterEvents)
        NotifyFindDuplicatedAssetsWindowsEvents()
    {
        List<string> getExemptedFolderPathEvents = [];
        List<Asset[]> deleteDuplicatedAssetsEvents = [];
        List<string> refreshAssetsCounterEvents = [];

        GetExemptedFolderPathEventTriggered += delegate
        {
            getExemptedFolderPathEvents.Add(string.Empty);
        };

        DeleteDuplicatedAssetsEventTriggered += delegate (object _, Asset[] assets)
        {
            deleteDuplicatedAssetsEvents.Add(assets);
        };

        RefreshAssetsCounterEventTriggered += delegate
        {
            refreshAssetsCounterEvents.Add(string.Empty);
        };

        return (getExemptedFolderPathEvents, deleteDuplicatedAssetsEvents, refreshAssetsCounterEvents);
    }

    private void CheckBeforeChanges(string expectedRootDirectory)
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
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
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
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets,
            applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo("The catalog process has ended."));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset);
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

    private static void CheckFolderNavigationViewModel(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset,
        Folder expectedSourceFolder)
    {
        CheckAfterChanges(
            folderNavigationViewModelInstance.ApplicationViewModel,
            expectedLastDirectoryInspected,
            expectedAppTitle,
            expectedAssets,
            expectedCurrentAsset,
            expectedCanGoToNextAsset);

        Assert.That(folderNavigationViewModelInstance.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
        Assert.That(folderNavigationViewModelInstance.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);
        Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.False);
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.False);
        Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.Null);
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2],
                Is.EqualTo(applicationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1],
                Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));
        }

        if (applicationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                applicationViewModelInstances[0],
                expectedLastDirectoryInspected,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedCanGoToNextAsset);
        }
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset,
                currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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

    private static void CheckFindDuplicatedAssetsViewModel(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        AssertDuplicatedAssetSets(findDuplicatedAssetsViewModelInstance, expectedDuplicatedAssetSets);

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition,
            Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition,
            Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet,
            expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset,
            expectedCurrentDuplicatedAsset);
    }

    private static void AssertDuplicatedAssetSets(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets)
    {
        if (expectedDuplicatedAssetSets.Count > 0)
        {
            for (int i = 0; i < expectedDuplicatedAssetSets.Count; i++)
            {
                AssertDuplicatedAssetsSet(
                    findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets[i],
                    expectedDuplicatedAssetSets[i]);
            }
        }
        else
        {
            Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets, Is.Empty);
        }
    }

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet,
        DuplicatedSetViewModel expectedDuplicatedAssetSet)
    {
        if (expectedDuplicatedAssetSet.Count > 0)
        {
            AssertDuplicatedSet(duplicatedAssetSet, expectedDuplicatedAssetSet);

            for (int i = 0; i < expectedDuplicatedAssetSet.Count; i++)
            {
                AssertDuplicatedAsset(duplicatedAssetSet[i], expectedDuplicatedAssetSet[i]);
            }
        }
        else
        {
            Assert.That(duplicatedAssetSet, Is.Empty);
        }
    }

    private static void AssertDuplicatedSet(
        DuplicatedSetViewModel duplicatedSetViewModel,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));

        Assert.That(duplicatedSetViewModel.Visible, Is.EqualTo(expectedDuplicatedSetViewModel.Visible));
    }

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset,
        DuplicatedAssetViewModel? expectedDuplicatedAsset)
    {
        if (expectedDuplicatedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAsset!.Asset, expectedDuplicatedAsset.Asset);

            Assert.That(duplicatedAsset.Visible, Is.EqualTo(expectedDuplicatedAsset.Visible));

            if (expectedDuplicatedAsset.ParentViewModel.Count > 0)
            {
                AssertDuplicatedSet(duplicatedAsset.ParentViewModel, expectedDuplicatedAsset.ParentViewModel);

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    Assert.That(duplicatedAsset.ParentViewModel[i].Visible,
                        Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].Visible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset,
                        expectedDuplicatedAsset.ParentViewModel[i].Asset);
                }
            }
            else
            {
                Assert.That(duplicatedAsset.ParentViewModel, Is.Empty);
            }
        }
        else
        {
            Assert.That(duplicatedAsset, Is.Null);
        }
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
            asset,
            expectedAsset,
            expectedAsset.FullPath,
            expectedAsset.Folder.Path,
            expectedAsset.Folder);
        // Unlike below (Application, CatalogAssetsService), it is set here for assets in the current directory
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null);
    }

    private void MainWindowsInit()
    {
        _folderNavigationViewModel = new(_applicationViewModel!, _sourceFolder!, []);

        CancellationTokenSource cancellationTokenSource = new();

        Assert.That(cancellationTokenSource.IsCancellationRequested, Is.False);
        Assert.That(cancellationTokenSource.Token.CanBeCanceled, Is.True);
        Assert.That(cancellationTokenSource.Token.IsCancellationRequested, Is.False);
    }

    private void RefreshAssetsCounter()
    {
        RefreshAssetsCounterEvent?.Invoke(this);
    }

    private void Delete(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        DeleteDuplicatedAssetsEvent?.Invoke(this, [duplicatedAssetViewModel.Asset]);

        _findDuplicatedAssetsViewModel?.CollapseAssets([duplicatedAssetViewModel]);
    }

    private void DeleteAll(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        List<DuplicatedAssetViewModel> assetsToDelete =
            _findDuplicatedAssetsViewModel?.GetDuplicatedAssets(duplicatedAssetViewModel.Asset) ?? [];

        DeleteDuplicatedAssetsEvent?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel?.CollapseAssets(assetsToDelete);
    }

    private void DeleteNotExemptedDuplicatedAssets()
    {
        string exemptedFolderPath = GetExemptedFolderPathEvent?.Invoke(this) ?? string.Empty;

        List<DuplicatedAssetViewModel> assetsToDelete =
            _findDuplicatedAssetsViewModel?.GetNotExemptedDuplicatedAssets(exemptedFolderPath) ?? [];

        DeleteDuplicatedAssetsEvent?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel?.CollapseAssets(assetsToDelete);
    }

    private string FindDuplicates()
    {
        List<List<Asset>> assetsSets = _application!.GetDuplicatedAssets();

        if (assetsSets.Count > 0)
        {
            _findDuplicatedAssetsViewModel = new(_application);
            _findDuplicatedAssetsViewModel.SetDuplicates(assetsSets);

            GetExemptedFolderPathEvent += GetExemptedFolderPath;
            DeleteDuplicatedAssetsEvent += DeleteDuplicatedAssets;
            RefreshAssetsCounterEvent += RefreshAssetsCounter;

            return "Duplicates found.";
        }

        return "No duplicates have been found.";
    }

    // The three methods below are only here to certify that the event is well triggered
    private string GetExemptedFolderPath(object sender)
    {
        GetExemptedFolderPathEventTriggered!.Invoke(sender);
        return _applicationViewModel!.GetExemptedFolderPath();
    }

    private void DeleteDuplicatedAssets(object sender, Asset[] assets)
    {
        DeleteDuplicatedAssetsEventTriggered!.Invoke(sender, assets);
    }

    private void RefreshAssetsCounter(object sender)
    {
        RefreshAssetsCounterEventTriggered!.Invoke(sender);
    }
}
