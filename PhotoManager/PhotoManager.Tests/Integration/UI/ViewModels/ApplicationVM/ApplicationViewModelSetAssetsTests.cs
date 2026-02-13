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

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelSetAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

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
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
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
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
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
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
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
    }

    [Test]
    public async Task SetAssets_CataloguedAssetsAndRootDirectory_SetsAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

            _applicationViewModel!.SetAssets(assetsDirectory, assets);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

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
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
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
    public async Task SetAssets_CataloguedAssetsAndOtherDirectory_UpdatesCurrentFolderPathAndSetsAssets()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            // Mock to update the path of each asset
            Folder folder = new() { Id = Guid.NewGuid(), Path = otherDirectory };

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            _asset1.ImageData = new();
            _asset2.ImageData = new();
            _asset3.ImageData = new();
            _asset4.ImageData = new();

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {otherDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            _applicationViewModel!.SetAssets(otherDirectory, expectedAssets);

            CheckAfterChanges(
                _applicationViewModel!,
                otherDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
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
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                otherDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
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
    public async Task SetAssets_NoCataloguedAssetsAndRootDirectory_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

            _applicationViewModel!.SetAssets(assetsDirectory, assets);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
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
    public async Task SetAssets_NoCataloguedAssetsAndOtherDirectory_UpdatesCurrentFolderPath()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {otherDirectory} - image 0 of 0 - sorted by file name ascending";

            Asset[] assets = _application!.GetAssetsByPath(assetsDirectory);

            _applicationViewModel!.SetAssets(otherDirectory, assets);

            CheckAfterChanges(
                _applicationViewModel!,
                otherDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                otherDirectory,
                true,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
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

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        bool expectedSortAscending,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.EqualTo(expectedSortAscending));
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
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

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

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
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
        bool expectedSortAscending,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2], Is.EqualTo(applicationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1], Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));
        }

        if (applicationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                applicationViewModelInstances[0],
                expectedLastDirectoryInspected,
                expectedSortAscending,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToNextAsset);
        }
    }
}
