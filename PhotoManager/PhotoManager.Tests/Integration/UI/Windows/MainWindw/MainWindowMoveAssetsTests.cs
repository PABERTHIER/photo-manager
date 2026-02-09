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
public class MainWindowMoveAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FolderNavigationViewModel? _mainFolderNavigationViewModel;
    private FolderNavigationViewModel? _folderNavigationViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private Asset? _asset1Temp;
    private Asset? _asset2Temp;

    private Folder? _sourceFolder;
    private Folder? _selectedFolder;
    private bool _hasConfirmed;

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
        _asset1Temp = new()
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
            ImageData = new(),
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
            ImageData = new(),
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
        _hasConfirmed = false;
        _selectedFolder = null;
        _sourceFolder = null;
        _mainFolderNavigationViewModel = null;
        _folderNavigationViewModel = null;
    }

    private void ConfigureApplicationViewModel(
        int catalogBatchSize,
        string assetsDirectory,
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new(userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_assetRepository, storageService, userConfigurationService);
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new(_application);

        _sourceFolder = new() { Id = Guid.NewGuid(), Path = _applicationViewModel!.CurrentFolderPath };
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsTrueAndAllAssets_CopiesAssets(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[0], observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            _hasConfirmed = true;

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(File.Exists(imagePath1NewDestination), Is.True);
            Assert.That(File.Exists(imagePath2NewDestination), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsFalseAndAllAssets_MovesAssets(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[0], observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            _hasConfirmed = true;

            string result = MoveAssets();

            Assert.That(result, appMode == AppMode.Viewer ? Is.EqualTo("ShowImage for ViewerUserControl") : Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(File.Exists(imagePath1NewDestination), Is.True);
            Assert.That(File.Exists(imagePath2NewDestination), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                _selectedFolder,
                false,
                false);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                _selectedFolder,
                false,
                false,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                _selectedFolder,
                false,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            }
            else
            {
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
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                _selectedFolder,
                false,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsTrueAndSecondAsset_CopiesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.GoToNextAsset();
            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 2 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset2Temp.FileName} - image 2 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            _hasConfirmed = true;

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                1,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                true,
                false);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                1,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                true,
                false,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                1,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                true,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
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
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
                // GoToNextAsset
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(23));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // GoToNextAsset
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                1,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                true,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsFalseAndSecondAsset_MovesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.GoToNextAsset();
            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 1 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp];

            _hasConfirmed = true;

            string result = MoveAssets();

            Assert.That(result, appMode == AppMode.Viewer ? Is.EqualTo("ShowImage for ViewerUserControl") : Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                false);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                false,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(26));
                // CatalogAssets + NotifyCatalogChange
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
                // GoToNextAsset
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(30));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // GoToNextAsset
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("ViewerPosition"));
                Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("CanGoToPreviousAsset"));
                Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("CanGoToNextAsset"));
                Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("CurrentAsset"));
                Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsTrueAndFirstAsset_CopiesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[0]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            _hasConfirmed = true;

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(File.Exists(imagePath1NewDestination), Is.True);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset1TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                _selectedFolder,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasConfirmedPreserveOriginalFilesIsFalseAndFirstAsset_MovesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);
            string imagePath1NewDestination = Path.Combine(newDestinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);
            string imagePath2NewDestination = Path.Combine(newDestinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);
            Assert.That(File.Exists(imagePath1NewDestination), Is.False);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[0]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 1 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset2Temp.FileName} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset2Temp];

            _hasConfirmed = true;

            string result = MoveAssets();

            Assert.That(result, appMode == AppMode.Viewer ? Is.EqualTo("ShowImage for ViewerUserControl") : Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(File.Exists(imagePath1NewDestination), Is.True);
            Assert.That(File.Exists(imagePath2NewDestination), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(_selectedFolder!.Path, asset1TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2TempFileName));

            assetsInRepository = _assetRepository!.GetAssetsByPath(newDestinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                false,
                false);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                false,
                false,
                folder,
                _selectedFolder,
                true,
                true,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                false,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            }
            else
            {
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
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                _selectedFolder,
                false,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasNotConfirmedPreserveOriginalFilesIsTrue_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                folder,
                _selectedFolder,
                true,
                false,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndHasNotConfirmedPreserveOriginalFilesIsFalse_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);
        string newDestinationDirectory = Path.Combine(destinationDirectory, Directories.FINAL_DESTINATION);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(newDestinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            _selectedFolder = _assetRepository!.GetFolderByPath(newDestinationDirectory);
            Assert.That(_selectedFolder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = MoveAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            CheckFolderNavigationViewModelAfterChanges(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                folder,
                _selectedFolder,
                true,
                false,
                [],
                _selectedFolder!.Path);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
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
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(_selectedFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndNoSelectedAssetsAndPreserveOriginalFilesIsTrue_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            Assert.That(_folderNavigationViewModel, Is.Null);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
                // CatalogAssets + NotifyCatalogChange
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
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_CataloguedAssetsAndNoSelectedAssetsPreserveOriginalFilesIsFalse_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, Directories.DESTINATION_TO_COPY);

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory, null);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = FileNames.IMAGE_1_JPG;
            const string asset2TempFileName = FileNames.IMAGE_9_PNG;

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp!.WithFolder(folder!);
            _asset2Temp = _asset2Temp!.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {destinationDirectory} - {_asset1Temp.FileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = MoveAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            Assert.That(_folderNavigationViewModel, Is.Null);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
                // CatalogAssets + NotifyCatalogChange
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
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
                // CatalogAssets + NotifyCatalogChange
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
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                null,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_NoCataloguedAssetsAndPreserveOriginalFilesIsTrue_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory, null);

            Directory.CreateDirectory(assetsDirectory);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {assetsDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            string result = CopyAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false);

            Assert.That(_folderNavigationViewModel, Is.Null);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false);

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
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task MoveAssets_NoCataloguedAssetsAndPreserveOriginalFilesIsFalse_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory, null);

            Directory.CreateDirectory(assetsDirectory);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager {Constants.VERSION} - {assetsDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            string result = MoveAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false);

            Assert.That(_folderNavigationViewModel, Is.Null);

            CheckMainFolderNavigationViewModel(
                _mainFolderNavigationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                0,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                null,
                false,
                false);

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

    private void CheckBeforeChanges(string expectedRootDirectory, Folder? expectedMoveAssetsLastSelectedFolder)
    {
        // From ApplicationViewModel
        Assert.That(_applicationViewModel!.SortAscending, Is.True);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.EqualTo(0));
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

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder!.Id, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        }

        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(expectedAppMode));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(expectedThumbnailsVisible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(expectedViewerVisible));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(expectedViewerPosition));
        AssertSelectedAssets(expectedSelectedAssets, applicationViewModelInstance.SelectedAssets);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
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

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            // Because the VM is doing a Guid.NewGuid()
            Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder!.Id, Is.Not.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        }

        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.EqualTo(expectedCanGoToPreviousAsset));
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckFolderNavigationViewModelAfterChanges(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset,
        Folder? expectedSourceFolder,
        Folder? expectedSelectedFolder,
        bool expectedCanConfirm,
        bool expectedHasConfirmed,
        ObservableCollection<string> expectedRecentTargetPaths,
        string? expectedTargetPath)
    {
        // From ApplicationViewModel
        CheckAfterChanges(
            folderNavigationViewModelInstance.ApplicationViewModel,
            expectedLastDirectoryInspected,
            expectedAppMode,
            expectedThumbnailsVisible,
            expectedViewerVisible,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedAssets,
            expectedCurrentAsset,
            expectedMoveAssetsLastSelectedFolder,
            expectedCanGoToPreviousAsset,
            expectedCanGoToNextAsset);

        // From FolderNavigationViewModel
        if (expectedSourceFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
            Assert.That(folderNavigationViewModelInstance.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.SourceFolder, Is.Null);
        }


        if (expectedSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.SelectedFolder!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(folderNavigationViewModelInstance.SelectedFolder.Path, Is.EqualTo(expectedSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);
        }

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Id, Is.Not.EqualTo(folderNavigationViewModelInstance.SelectedFolder!.Id));
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        }

        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.EqualTo(expectedCanConfirm));
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.EqualTo(expectedHasConfirmed));

        if (expectedRecentTargetPaths.Count == 0)
        {
            Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Is.Empty);
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Has.Count.EqualTo(expectedRecentTargetPaths.Count));

            for (int i = 0; i < expectedRecentTargetPaths.Count; i++)
            {
                Assert.That(folderNavigationViewModelInstance.RecentTargetPaths[i], Is.EqualTo(expectedRecentTargetPaths[i]));
            }
        }

        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.EqualTo(expectedTargetPath));
    }

    private static void CheckMainFolderNavigationViewModel(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset,
        Folder expectedSourceFolder)
    {
        // From ApplicationViewModel
        CheckAfterChanges(
            folderNavigationViewModelInstance.ApplicationViewModel,
            expectedLastDirectoryInspected,
            expectedAppMode,
            expectedThumbnailsVisible,
            expectedViewerVisible,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedAssets,
            expectedCurrentAsset,
            expectedMoveAssetsLastSelectedFolder,
            expectedCanGoToPreviousAsset,
            expectedCanGoToNextAsset);

        // From FolderNavigationViewModel
        Assert.That(folderNavigationViewModelInstance.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
        Assert.That(folderNavigationViewModelInstance.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        }

        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.False);
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.False);
        Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.Null);
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

    private static void AssertSelectedAssets(Asset[] expectedAssets, Asset[] selectedAssets)
    {
        Assert.That(selectedAssets, Has.Length.EqualTo(expectedAssets.Length));

        for (int i = 0; i < selectedAssets.Length; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentSelectedAsset = selectedAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentSelectedAsset, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            Assert.That(currentSelectedAsset.ImageData, Is.Not.Null);
        }
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets, ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

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
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToPreviousAsset,
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
                expectedAppMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedViewerPosition,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedMoveAssetsLastSelectedFolder,
                expectedCanGoToPreviousAsset,
                expectedCanGoToNextAsset);
        }
    }

    private void MainWindowsInit()
    {
        _mainFolderNavigationViewModel = new(_applicationViewModel!, _sourceFolder!, []);

        CancellationTokenSource cancellationTokenSource = new();

        Assert.That(cancellationTokenSource.IsCancellationRequested, Is.False);
        Assert.That(cancellationTokenSource.Token.CanBeCanceled, Is.True);
        Assert.That(cancellationTokenSource.Token.IsCancellationRequested, Is.False);
    }

    private void Confirm()
    {
        if (_folderNavigationViewModel!.CanConfirm)
        {
            _folderNavigationViewModel!.HasConfirmed = true;
        }
    }

    private string CopyAssets()
    {
        return MoveAssets(preserveOriginalFiles: true);
    }

    private string MoveAssets()
    {
        // ReSharper disable once IntroduceOptionalParameters.Local
        return MoveAssets(preserveOriginalFiles: false);
    }

    private string MoveAssets(bool preserveOriginalFiles)
    {
        Asset[] assets = _applicationViewModel!.SelectedAssets;

        if (assets.Length > 0)
        {
            string initialSelectedPath = Init(assets);

            Assert.That(initialSelectedPath, Is.EqualTo(assets[0].Folder.Path));

            FolderSelected(_selectedFolder!.Path);

            if (_folderNavigationViewModel!.SelectedFolder != null && _hasConfirmed)
            {
                Confirm();

                if (_folderNavigationViewModel!.HasConfirmed)
                {
                    bool result = _application!.MoveAssets(assets, _folderNavigationViewModel!.SelectedFolder, preserveOriginalFiles);

                    if (result)
                    {
                        _applicationViewModel!.MoveAssetsLastSelectedFolder = _folderNavigationViewModel!.SelectedFolder;
                        _applicationViewModel!.IsRefreshingFolders = true;
                        _applicationViewModel!.IsRefreshingFolders = false;

                        if (!preserveOriginalFiles)
                        {
                            _applicationViewModel!.RemoveAssets(assets);

                            if (_applicationViewModel!.AppMode == AppMode.Viewer)
                            {
                                return ShowImage();
                            }
                        }
                    }
                }
            }
        }

        return string.Empty;
    }

    private string Init(Asset[] assets)
    {
        _folderNavigationViewModel = new(
            _applicationViewModel!,
            assets[0].Folder,
            _application!.GetRecentTargetPaths());

        return _folderNavigationViewModel!.LastSelectedFolder != null
            ? _folderNavigationViewModel!.LastSelectedFolder.Path
            : _folderNavigationViewModel!.SourceFolder.Path;
    }

    private void FolderSelected(string selectedPath)
    {
        _folderNavigationViewModel!.TargetPath = selectedPath;
    }

    private string ShowImage()
    {
        return _applicationViewModel!.AppMode == AppMode.Viewer ? "ShowImage for ViewerUserControl" : "ShowImage for ThumbnailsUserControl";
    }
}
