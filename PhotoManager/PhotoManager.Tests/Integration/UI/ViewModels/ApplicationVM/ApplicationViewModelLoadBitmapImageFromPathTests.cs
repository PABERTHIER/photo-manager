using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelLoadBitmapImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;
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

        _asset1 = new()
        {
            FolderId = Guid.Empty,
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
            FolderId = Guid.Empty,
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
            FolderId = Guid.Empty,
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
            FolderId = Guid.Empty,
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
        PhotoManager.Application.Application application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    [Test]
    public async Task LoadBitmapImageFromPath_CataloguedAssets_ReturnsBitmapImage()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

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

            Asset[] expectedObservableAssets = [_asset1!, _asset2!, _asset3!, _asset4!];

            const string expectedStatusMessage = "The catalog process has ended.";

            BitmapImage image1 = _applicationViewModel!.LoadBitmapImageFromPath();

            Assert.That(image1, Is.Not.Null);
            Assert.That(image1.StreamSource, Is.Null);
            Assert.That(image1.Rotation, Is.EqualTo(_asset1!.ImageRotation));
            Assert.That(image1.Width, Is.EqualTo(_asset1.Pixel.Asset.Width));
            Assert.That(image1.Height, Is.EqualTo(_asset1.Pixel.Asset.Height));
            Assert.That(image1.PixelWidth, Is.EqualTo(_asset1.Pixel.Asset.Width));
            Assert.That(image1.PixelHeight, Is.EqualTo(_asset1.Pixel.Asset.Height));
            Assert.That(image1.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image1.DecodePixelHeight, Is.EqualTo(0));

            CheckAfterChanges(
                _applicationViewModel!,
                0,
                assetsDirectory,
                1,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset1,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
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

            CheckInstance(
                applicationViewModelInstances,
                0,
                assetsDirectory,
                1,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset1,
                false,
                true);

            _applicationViewModel!.GoToAsset(_applicationViewModel.ObservableAssets[1]);

            BitmapImage image2 = _applicationViewModel!.LoadBitmapImageFromPath();

            Assert.That(image2, Is.Not.Null);
            Assert.That(image2.StreamSource, Is.Null);
            Assert.That(image2.Rotation, Is.EqualTo(_asset2!.ImageRotation));
            Assert.That((int)image2.Width, Is.EqualTo(_asset2.Pixel.Asset.Width));
            Assert.That((int)image2.Height, Is.EqualTo(_asset2.Pixel.Asset.Height));
            Assert.That(image2.PixelWidth, Is.EqualTo(_asset2.Pixel.Asset.Width));
            Assert.That(image2.PixelHeight, Is.EqualTo(_asset2.Pixel.Asset.Height));
            Assert.That(image2.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image2.DecodePixelHeight, Is.EqualTo(0));

            CheckAfterChanges(
                _applicationViewModel!,
                1,
                assetsDirectory,
                2,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset2,
                true,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(22));
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
            // First GoToAsset
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                1,
                assetsDirectory,
                2,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset2,
                true,
                true);

            _applicationViewModel!.GoToAsset(_applicationViewModel.ObservableAssets[2]);

            BitmapImage image3 = _applicationViewModel!.LoadBitmapImageFromPath();

            Assert.That(image3, Is.Not.Null);
            Assert.That(image3.StreamSource, Is.Null);
            Assert.That(image3.Rotation, Is.EqualTo(_asset3!.ImageRotation));
            Assert.That((int)image3.Width, Is.EqualTo(_asset3.Pixel.Asset.Width));
            Assert.That((int)image3.Height, Is.EqualTo(_asset3.Pixel.Asset.Height));
            Assert.That(image3.PixelWidth, Is.EqualTo(_asset3.Pixel.Asset.Width));
            Assert.That(image3.PixelHeight, Is.EqualTo(_asset3.Pixel.Asset.Height));
            Assert.That(image3.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image3.DecodePixelHeight, Is.EqualTo(0));

            CheckAfterChanges(
                _applicationViewModel!,
                2,
                assetsDirectory,
                3,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset3,
                true,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(27));
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
            // First GoToAsset
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            // Second GoToAsset
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                2,
                assetsDirectory,
                3,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset3,
                true,
                true);

            _applicationViewModel!.GoToAsset(_applicationViewModel.ObservableAssets[3]);

            NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _applicationViewModel!.LoadBitmapImageFromPath());

            Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));

            CheckAfterChanges(
                _applicationViewModel!,
                3,
                assetsDirectory,
                4,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset4,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
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
            // First GoToAsset
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            // Second GoToAsset
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("AppTitle"));
            // Third GoToAsset
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                3,
                assetsDirectory,
                4,
                expectedObservableAssets,
                expectedStatusMessage,
                _asset4,
                true,
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

    [Test]
    public void LoadBitmapImageFromPath_NoCataloguedAssets_ThrowsNullReferenceException()
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _applicationViewModel!.LoadBitmapImageFromPath());

            Assert.That(exception?.Message, Is.EqualTo("CurrentAsset is null"));

            CheckAfterChanges(_applicationViewModel!, 0, _dataDirectory!, 1, [], null, null, false, false);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(applicationViewModelInstances, 0, _dataDirectory!, 1, [], null, null, false, false);

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

    private void CheckBeforeChanges(string expectedRootDirectory)
    {
        Assert.That(_applicationViewModel!.SortAscending, Is.True);
        Assert.That(_applicationViewModel!.Product, Is.Null);
        Assert.That(_applicationViewModel!.Version, Is.Null);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.EqualTo(0));
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.Null);
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.Null);
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.Null);
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo($"  - {expectedRootDirectory} - image 1 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.Null);
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        int expectedViewerPosition,
        string expectedLastDirectoryInspected,
        int expectedCurrentImageAppTitlePosition,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.Product, Is.Null);
        Assert.That(applicationViewModelInstance.Version, Is.Null);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(expectedViewerPosition));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.Null);
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.Null);
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.Null);
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo($"  - {expectedLastDirectoryInspected} - image {expectedCurrentImageAppTitlePosition} of {expectedAssets.Length} - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedCurrentAsset.Folder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.EqualTo(expectedCanGoToPreviousAsset));
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        int expectedViewerPosition,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2], Is.EqualTo(applicationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1],
                Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));
        }

        if (applicationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                applicationViewModelInstances[0],
                expectedViewerPosition,
                expectedLastDirectoryInspected,
                expectedAppTitleAssetsCount,
                expectedAssets,
                expectedStatusMessage,
                expectedCurrentAsset,
                expectedCanGoToPreviousAsset,
                expectedCanGoToNextAsset);
        }
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets, ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAssets = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAssets, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            if (string.Equals(currentObservableAssets.Folder.Path, currentDirectory))
            {
                Assert.That(currentObservableAssets.ImageData, Is.Not.Null);
            }
            else
            {
                Assert.That(currentObservableAssets.ImageData, Is.Null);
            }
        }
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }
}
