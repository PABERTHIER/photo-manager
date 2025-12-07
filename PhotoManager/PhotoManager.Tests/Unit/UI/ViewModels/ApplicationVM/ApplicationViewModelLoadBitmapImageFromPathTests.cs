using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using FileSize = PhotoManager.Tests.Unit.Constants.FileSize;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Unit.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelLoadBitmapImageFromPathTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private ApplicationViewModel? _applicationViewModel;
    private AssetRepository? _assetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
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
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate90, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)]
    [TestCase(Rotation.Rotate270, PixelHeightAsset.IMAGE_1_JPG, PixelWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG)]
    // [TestCase(null, PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG, ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)]
    public void LoadBitmapImageFromPath_ValidRotationAndPath_ReturnsBitmapImage(
        Rotation rotation,
        int expectedWith,
        int expectedHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight)
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

            const string fileName = FileNames.IMAGE_1_JPG;
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
                    Thumbnail = new() { Width = expectedThumbnailPixelWidth, Height = expectedThumbnailPixelHeight }
                },
                FileProperties = new()
                {
                    Size = FileSize.IMAGE_1_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = Hashes.IMAGE_1_JPG,
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

            const string fileName = FileNames.NON_EXISTENT_IMAGE_JPG;
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
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_JPG, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_JPG }
                },
                FileProperties = new()
                {
                    Size = FileSize.NON_EXISTENT_IMAGE_JPG,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = Hashes.NON_EXISTENT_IMAGE_JPG,
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

            const string fileName = FileNames.IMAGE_1_JPG;
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
                ImageRotation = rotation,
                Hash = Hashes.IMAGE_1_JPG,
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

    // TODO: Migrate from MagickImage to BitmapImage ?
    [Test]
    public void LoadBitmapImageFromPath_HeicImageFormat_ReturnsBitmapImage()
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

            const string fileName = FileNames.IMAGE_11_HEIC;
            string filePath = Path.Combine(_dataDirectory!, fileName);
            const Rotation rotation = Rotation.Rotate0;

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
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
                ImageRotation = rotation,
                Hash = Hashes.IMAGE_11_HEIC,
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
            Assert.That(image.Width, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC)); // Wrong width (getting the height value instead)
            Assert.That(image.Height, Is.EqualTo(5376)); // Wrong height
            Assert.That(image.PixelWidth, Is.EqualTo(PixelWidthAsset.IMAGE_11_HEIC));
            Assert.That(image.PixelHeight, Is.EqualTo(PixelHeightAsset.IMAGE_11_HEIC));
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
        int expectedAppTitleAssetsCount,
        Asset[] expectedAssets,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 1 of {expectedAppTitleAssetsCount} - sorted by file name ascending"));
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
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
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

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }
}
