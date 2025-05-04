using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels;

[TestFixture]
public class FolderNavigationViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private FolderNavigationViewModel? _folderNavigationViewModel;
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
    }

    [TearDown]
    public void TearDown()
    {
        _folderNavigationViewModel = null;
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

        UserConfigurationService userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (_application);
    }

    [Test]
    public async Task Constructor_CataloguedAssetsAndSourceFolderAndLastSelectedFolderAndRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Folder? folder = _assetRepository!.AddFolder(assetsDirectory);
            List<string> recentTargetPaths = [assetsDirectory];

            _applicationViewModel!.MoveAssetsLastSelectedFolder = folder;

            _folderNavigationViewModel = new (_applicationViewModel, _application!, folder, recentTargetPaths);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, folder, folder, [..recentTargetPaths]);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            CheckAfterChanges(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                folder,
                true,
                folder,
                null,
                [..recentTargetPaths]);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationViewModelPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));

            CheckInstance(
                folderNavigationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                folder,
                true,
                folder,
                null,
                [..recentTargetPaths]);

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
    public void Constructor_NoCataloguedAssetsAndSourceFolderIsNotNullAndLastSelectedFolderIsSourceAndRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);
            List<string> recentTargetPaths = [assetsDirectory];

            _applicationViewModel!.MoveAssetsLastSelectedFolder = folder;

            _folderNavigationViewModel = new (_applicationViewModel, _application!, folder, recentTargetPaths);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, folder, folder, [..recentTargetPaths]);

            Assert.That(folderNavigationViewModelInstances, Is.Empty);
            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Is.Empty);

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
    public void Constructor_NoCataloguedAssetsAndSourceFolderIsNotNullAndLastSelectedFolderIsSourceAndNoRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _applicationViewModel!.MoveAssetsLastSelectedFolder = folder;

            _folderNavigationViewModel = new (_applicationViewModel, _application!, folder, []);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, folder, folder, []);

            Assert.That(folderNavigationViewModelInstances, Is.Empty);
            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Is.Empty);

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
    public void Constructor_NoCataloguedAssetsAndSourceFolderIsNotNullAndLastSelectedFolderIsNotSourceAndNoRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");
        string otherDirectory = Path.Combine(_dataDirectory!, "Toto");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Folder folder1 = _assetRepository!.AddFolder(assetsDirectory);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            _applicationViewModel!.MoveAssetsLastSelectedFolder = folder2;

            _folderNavigationViewModel = new (_applicationViewModel, _application!, folder1, []);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, folder2, folder1, []);

            Assert.That(folderNavigationViewModelInstances, Is.Empty);
            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Is.Empty);

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
    public void Constructor_NoCataloguedAssetsAndSourceFolderIsNotNullAndLastSelectedFolderIsNullAndNoRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _folderNavigationViewModel = new (_applicationViewModel!, _application!, folder, []);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, null, folder, []);

            Assert.That(folderNavigationViewModelInstances, Is.Empty);
            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Is.Empty);

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
    public void Constructor_NoCataloguedAssetsAndSourceFolderIsNullAndLastSelectedFolderIsNullAndNoRecentTargetPaths_SetsProperties()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplication(100, assetsDirectory, 200, 150, false, false, false, false);

        try
        {
            _folderNavigationViewModel = new (_applicationViewModel!, _application!, null!, []);

            (
                List<string> notifyPropertyChangedEvents,
                List<string> notifyApplicationViewModelPropertyChangedEvents,
                List<FolderNavigationViewModel> folderNavigationViewModelInstances,
                List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
            ) = NotifyPropertyChangedEvents();

            CheckBeforeChanges(assetsDirectory, null, null, []);

            Assert.That(folderNavigationViewModelInstances, Is.Empty);
            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(notifyApplicationViewModelPropertyChangedEvents, Is.Empty);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private
        (List<string> notifyPropertyChangedEvents,
        List<string> notifyApplicationViewModelPropertyChangedEvents,
        List<FolderNavigationViewModel> folderNavigationViewModelInstances,
        List<Folder> folderAddedEvents,
        List<Folder> folderRemovedEvents)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<FolderNavigationViewModel> folderNavigationViewModelInstances = [];

        _folderNavigationViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            folderNavigationViewModelInstances.Add((FolderNavigationViewModel)sender!);
        };

        List<string> notifyApplicationViewModelPropertyChangedEvents = [];
        _applicationViewModel!.PropertyChanged += delegate(object? _, PropertyChangedEventArgs e)
        {
            notifyApplicationViewModelPropertyChangedEvents.Add(e.PropertyName!);
        };

        List<Folder> folderAddedEvents = [];

        _folderNavigationViewModel!.ApplicationViewModel.FolderAdded += delegate(object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = [];

        _folderNavigationViewModel!.ApplicationViewModel.FolderRemoved += delegate(object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (
            notifyPropertyChangedEvents,
            notifyApplicationViewModelPropertyChangedEvents,
            folderNavigationViewModelInstances,
            folderAddedEvents,
            folderRemovedEvents);
    }

    private void CheckBeforeChanges(
        string expectedRootDirectory,
        Folder? expectedMoveAssetsLastSelectedFolder,
        Folder? expectedSourceFolder,
        ObservableCollection<string> expectedRecentTargetPaths)
    {
        // From ApplicationViewModel
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SortAscending, Is.True);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.IsRefreshingFolders, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ViewerPosition, Is.EqualTo(0));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SelectedAssets, Is.Empty);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ObservableAssets, Is.Empty);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.GlobalAssetsCounterWording, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ExecutionTimeWording, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.TotalFilesCountWording, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AppTitle,
            Is.EqualTo($"PhotoManager v1.0.0 - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.StatusMessage, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CurrentAsset, Is.Null);

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(_folderNavigationViewModel!.ApplicationViewModel.MoveAssetsLastSelectedFolder!.Id,
                Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(_folderNavigationViewModel!.ApplicationViewModel.MoveAssetsLastSelectedFolder!.Path,
                Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(_folderNavigationViewModel!.ApplicationViewModel.MoveAssetsLastSelectedFolder, Is.Null);
        }

        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CanGoToPreviousAsset, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CanGoToNextAsset, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Version, Is.EqualTo("v1.0.0"));

        // From FolderNavigationViewModel
        if (expectedSourceFolder != null)
        {
            Assert.That(_folderNavigationViewModel!.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
            Assert.That(_folderNavigationViewModel!.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        }
        else
        {
            Assert.That(_folderNavigationViewModel!.SourceFolder, Is.Null);
        }

        Assert.That(_folderNavigationViewModel!.SelectedFolder, Is.Null);

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder!.Id, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder, Is.Null);
        }

        Assert.That(_folderNavigationViewModel!.CanConfirm, Is.False);
        Assert.That(_folderNavigationViewModel!.HasConfirmed, Is.False);

        for (int i = 0; i < expectedRecentTargetPaths.Count; i++)
        {
            Assert.That(_folderNavigationViewModel!.RecentTargetPaths[i], Is.EqualTo(expectedRecentTargetPaths[i]));
        }

        Assert.That(_folderNavigationViewModel!.TargetPath, Is.Null);
    }

    private static void CheckAfterChanges(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToNextAsset,
        Folder? expectedSourceFolder,
        Folder? expectedSelectedFolder,
        ObservableCollection<string> expectedRecentTargetPaths)
    {
        // From ApplicationViewModel
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SortAscending, Is.True);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.IsRefreshingFolders, Is.False);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ViewerPosition, Is.EqualTo(0));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SelectedAssets, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, folderNavigationViewModelInstance.ApplicationViewModel.ObservableAssets);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.GlobalAssetsCounterWording, Is.Null);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ExecutionTimeWording, Is.Null);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.TotalFilesCountWording, Is.Null);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(
                folderNavigationViewModelInstance.ApplicationViewModel.CurrentAsset!,
                expectedCurrentAsset,
                expectedCurrentAsset.FullPath,
                expectedLastDirectoryInspected,
                expectedFolder);
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CurrentAsset, Is.Null);
        }

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.MoveAssetsLastSelectedFolder!.Id,
                Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.MoveAssetsLastSelectedFolder!.Path,
                Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.MoveAssetsLastSelectedFolder, Is.Null);
        }

        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CanGoToPreviousAsset, Is.False);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Version, Is.EqualTo("v1.0.0"));

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
            Assert.That(folderNavigationViewModelInstance.SelectedFolder!.Id, Is.EqualTo(expectedSelectedFolder.Id));
            Assert.That(folderNavigationViewModelInstance.SelectedFolder.Path, Is.EqualTo(expectedSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);
        }

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Id, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        }

        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.False);
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.False);

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

        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.Null);
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
        List<FolderNavigationViewModel> folderNavigationViewModelInstances,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        Folder? expectedMoveAssetsLastSelectedFolder,
        bool expectedCanGoToNextAsset,
        Folder? expectedSourceFolder,
        Folder? expectedSelectedFolder,
        ObservableCollection<string> expectedRecentTargetPaths)
    {
        int folderNavigationViewModelInstancesCount = folderNavigationViewModelInstances.Count;

        if (folderNavigationViewModelInstancesCount > 1)
        {
            Assert.That(folderNavigationViewModelInstances[folderNavigationViewModelInstancesCount - 2],
                Is.EqualTo(folderNavigationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(folderNavigationViewModelInstances[folderNavigationViewModelInstancesCount - 1],
                Is.EqualTo(folderNavigationViewModelInstances[folderNavigationViewModelInstancesCount - 2]));
        }

        if (folderNavigationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                folderNavigationViewModelInstances[0],
                expectedLastDirectoryInspected,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedMoveAssetsLastSelectedFolder,
                expectedCanGoToNextAsset,
                expectedSourceFolder,
                expectedSelectedFolder,
                expectedRecentTargetPaths);
        }
    }
}
