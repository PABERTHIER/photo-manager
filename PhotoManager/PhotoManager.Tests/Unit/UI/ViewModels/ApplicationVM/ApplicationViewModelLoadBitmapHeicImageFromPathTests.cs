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
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelLoadBitmapHeicImageFromPathTests
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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

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
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);
        _applicationViewModel = new(application);
    }

    [Test]
    [TestCase(Rotation.Rotate0, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate90, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate180, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    [TestCase(Rotation.Rotate270, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    // [TestCase(null, PixelWidthAsset.IMAGE_11_HEIC, PixelHeightAsset.IMAGE_11_HEIC)]
    public void LoadBitmapHeicImageFromPath_ValidPathAndRotationAndNotRotatedImage_ReturnsBitmapImage(Rotation rotation, int expectedWidth, int expectedHeight)
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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = expectedWidth, Height = expectedHeight },
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

            byte[] assetData = File.ReadAllBytes(filePath);

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            BitmapImage image = _applicationViewModel!.LoadBitmapHeicImageFromPath();

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);

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
    [TestCase(FileNames.IMAGE_11_90_DEG_HEIC, Rotation.Rotate90, PixelWidthAsset.IMAGE_11_90_DEG_HEIC, PixelHeightAsset.IMAGE_11_90_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_90_DEG_HEIC, ThumbnailHeightAsset.IMAGE_11_90_DEG_HEIC, FileSize.IMAGE_11_90_DEG_HEIC, Hashes.IMAGE_11_90_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_180_DEG_HEIC, Rotation.Rotate180, PixelWidthAsset.IMAGE_11_180_DEG_HEIC, PixelHeightAsset.IMAGE_11_180_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_180_DEG_HEIC, ThumbnailHeightAsset.IMAGE_11_180_DEG_HEIC, FileSize.IMAGE_11_180_DEG_HEIC, Hashes.IMAGE_11_180_DEG_HEIC)]
    [TestCase(FileNames.IMAGE_11_270_DEG_HEIC, Rotation.Rotate270, PixelWidthAsset.IMAGE_11_270_DEG_HEIC, PixelHeightAsset.IMAGE_11_270_DEG_HEIC, ThumbnailWidthAsset.IMAGE_11_270_DEG_HEIC, ThumbnailHeightAsset.IMAGE_11_270_DEG_HEIC, FileSize.IMAGE_11_270_DEG_HEIC, Hashes.IMAGE_11_270_DEG_HEIC)]
    public void LoadBitmapHeicImageFromPath_ValidPathAndRotationAndRotatedImage_ReturnsBitmapImage(
        string fileName,
        Rotation rotation,
        int expectedWidth,
        int expectedHeight,
        int expectedThumbnailPixelWidth,
        int expectedThumbnailPixelHeight,
        long expectedFileSize,
        string expectedHash)
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

            string filePath = Path.Combine(_dataDirectory!, fileName);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = expectedWidth, Height = expectedHeight },
                    Thumbnail = new() { Width = expectedThumbnailPixelWidth, Height = expectedThumbnailPixelHeight }
                },
                FileProperties = new()
                {
                    Size = expectedFileSize,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = expectedHash,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = File.ReadAllBytes(filePath);

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            BitmapImage image = _applicationViewModel!.LoadBitmapHeicImageFromPath();

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Not.Null);
            Assert.That(image.Rotation, Is.EqualTo(rotation));
            Assert.That(image.Width, Is.EqualTo(expectedWidth));
            Assert.That(image.Height, Is.EqualTo(expectedHeight));
            Assert.That(image.PixelWidth, Is.EqualTo(expectedWidth));
            Assert.That(image.PixelHeight, Is.EqualTo(expectedHeight));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);

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
    public void LoadBitmapHeicImageFromPath_ImageDoesNotExist_ReturnsDefaultBitmapImage()
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

            const string fileName = FileNames.NON_EXISTENT_IMAGE_HEIC;
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
                    Asset = new() { Width = PixelWidthAsset.NON_EXISTENT_IMAGE_HEIC, Height = PixelHeightAsset.NON_EXISTENT_IMAGE_HEIC },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.NON_EXISTENT_IMAGE_HEIC, Height = ThumbnailHeightAsset.NON_EXISTENT_IMAGE_HEIC }
                },
                FileProperties = new()
                {
                    Size = FileSize.NON_EXISTENT_IMAGE_HEIC,
                    Creation = DateTime.Now,
                    Modification = ModificationDate.Default
                },
                ThumbnailCreationDateTime = DateTime.Now,
                ImageRotation = rotation,
                Hash = Hashes.NON_EXISTENT_IMAGE_HEIC,
                Metadata = new()
                {
                    Corrupted = new() { IsTrue = false, Message = null },
                    Rotated = new() { IsTrue = false, Message = null }
                }
            };

            byte[] assetData = File.ReadAllBytes(Path.Combine(_dataDirectory!, FileNames.IMAGE_11_HEIC));

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            BitmapImage image = _applicationViewModel!.LoadBitmapHeicImageFromPath();

            Assert.That(image, Is.Not.Null);
            Assert.That(image.StreamSource, Is.Null);
            Assert.That(image.Rotation, Is.EqualTo(Rotation.Rotate0));
            Assert.That(image.DecodePixelWidth, Is.Zero);
            Assert.That(image.DecodePixelHeight, Is.Zero);

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
    public void LoadBitmapHeicImageFromPath_InvalidRotation_ThrowsArgumentException()
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
            const Rotation rotation = (Rotation)999;

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

            byte[] assetData = File.ReadAllBytes(filePath);

            _assetRepository.AddAsset(asset, assetData);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = $"Image {filePath} added to catalog."
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            ArgumentException? exception = Assert.Throws<ArgumentException>(() => _applicationViewModel!.LoadBitmapHeicImageFromPath());

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
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.Zero);
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
