using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
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
    [TestCase(Rotation.Rotate0, 1280, 720)]
    [TestCase(Rotation.Rotate90, 720, 1280)]
    [TestCase(Rotation.Rotate180, 1280, 720)]
    [TestCase(Rotation.Rotate270, 720, 1280)]
    // [TestCase(null, 1280, 720)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(Rotation rotation, int expectedWith, int expectedHeight)
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

            const string fileName = "Image 1.jpg";
            string filePath = Path.Combine(_dataDirectory!, fileName);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = expectedWith, Height = expectedHeight },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = _expectedFileModificationDateTime
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = [1, 2, 3];

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            BitmapImage image = _applicationViewModel!.LoadBitmapImageFromPath();

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWith));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWith));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));

            string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            CheckInstance(applicationViewModelInstances, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

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
    public void LoadBitmapImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
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

            const string fileName = "ImageDoesNotExist.jpg";
            string filePath = Path.Combine(_dataDirectory!, fileName);
            const Rotation rotation = Rotation.Rotate90;

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 1000, Height = 1000 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                FileProperties = new()
                {
                    Size = 29857,
                    Creation = DateTime.Now,
                    Modification = _expectedFileModificationDateTime
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = [1, 2, 3];

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            BitmapImage image = _applicationViewModel!.LoadBitmapImageFromPath();

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.EqualTo(0));
            Assert.That(image.DecodePixelHeight, Is.EqualTo(0));

            string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            CheckInstance(applicationViewModelInstances, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

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
    public void LoadBitmapImageFromPath_InvalidRotation_ThrowsArgumentException()
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

            const string fileName = "Image 1.jpg";
            string filePath = Path.Combine(_dataDirectory!, fileName);
            const Rotation rotation = (Rotation)999;

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
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
                ImageRotation = rotation,
                Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = [1, 2, 3];

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _applicationViewModel!.LoadBitmapImageFromPath());

            Assert.That(exception?.Message, Is.EqualTo($"'{rotation}' is not a valid value for property 'Rotation'."));

            string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            CheckInstance(applicationViewModelInstances, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

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
    public void LoadBitmapImageFromPath_InvalidImageFormat_ThrowsNotSupportedException()
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

            const string fileName = "Image_11.heic";
            string filePath = Path.Combine(_dataDirectory!, fileName);
            const Rotation rotation = Rotation.Rotate90;

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 3024, Height = 4030 },
                    Thumbnail = new() { Width = 112, Height = 150 }
                },
                FileProperties = new()
                {
                    Size = 1411940,
                    Creation = DateTime.Now,
                    Modification = _expectedFileModificationDateTime
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = [1, 2, 3];

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            NotSupportedException? exception = Assert.Throws<NotSupportedException>(() => _applicationViewModel!.LoadBitmapImageFromPath());

            Assert.That(exception?.Message, Is.EqualTo("No imaging component suitable to complete this operation was found."));

            string expectedStatusMessage = $"Image {asset.FullPath} added to catalog.";

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            CheckInstance(applicationViewModelInstances, _dataDirectory!, 1, [asset], expectedStatusMessage, asset, false);

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
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
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
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.Null);
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.Null);
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.Null);
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo($"  - {expectedLastDirectoryInspected} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending"));
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
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
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
                expectedLastDirectoryInspected,
                expectedAppTitleAssetsCount,
                expectedAssets,
                expectedStatusMessage,
                expectedCurrentAsset,
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
