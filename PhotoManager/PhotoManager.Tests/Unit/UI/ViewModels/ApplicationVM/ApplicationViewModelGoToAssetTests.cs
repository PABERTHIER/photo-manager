using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelGoToAssetTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private ApplicationViewModel? _applicationViewModel;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;
    private Asset _asset5;
    private Asset _asset6;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        Guid folderId = Guid.NewGuid();

        _asset1 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2020,
                Creation = new (2010, 1, 1, 20, 20, 20, 20, 20),
                Modification = new (2011, 1, 1, 20, 20, 20, 20, 20)
            },
            ThumbnailCreationDateTime = new (2010, 1, 1, 20, 20, 20, 20, 20),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = FileNames.IMAGE_2_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2048,
                Creation = new (2020, 6, 1),
                Modification = new (2020, 7, 1)
            },
            ThumbnailCreationDateTime = new (2020, 6, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = FileNames.IMAGE_3_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_3_JPG, Height = PixelHeightAsset.IMAGE_3_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_3_JPG, Height = ThumbnailHeightAsset.IMAGE_3_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2000,
                Creation = new (2010, 1, 1),
                Modification = new (2011, 1, 1)
            },
            ThumbnailCreationDateTime = new (2010, 1, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = FileNames.IMAGE_4_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_4_JPG, Height = PixelHeightAsset.IMAGE_4_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_4_JPG, Height = ThumbnailHeightAsset.IMAGE_4_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2030,
                Creation = new (2010, 8, 1),
                Modification = new (2011, 9, 1)
            },
            ThumbnailCreationDateTime = new (2010, 8, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = FileNames.IMAGE_5_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_5_JPG, Height = PixelHeightAsset.IMAGE_5_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_5_JPG, Height = ThumbnailHeightAsset.IMAGE_5_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2048,
                Creation = new (2020, 6, 1),
                Modification = new (2020, 7, 1)
            },
            ThumbnailCreationDateTime = new (2020, 6, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset6 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = Path.Combine(_dataDirectory!, Directories.FOLDER_1) },
            FileName = FileNames.IMAGE_6_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_6_JPG, Height = PixelHeightAsset.IMAGE_6_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_6_JPG, Height = ThumbnailHeightAsset.IMAGE_6_JPG }
            },
            Hash = string.Empty,
            ImageData = new(),
            FileProperties = new()
            {
                Size = 2048,
                Creation = new (2021, 6, 1),
                Modification = new (2021, 7, 1)
            },
            ThumbnailCreationDateTime = new (2021, 6, 1),
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
        AssetRepository assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (assetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new (assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    [Test]
    public void GoToAsset_AssetIsInCurrentDirectory_GoesToAsset()
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

            int expectedViewerPosition = 1;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 2 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            // First GoToAsset
            _applicationViewModel!.GoToAsset(_asset2);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            // Second GoToAsset
            expectedViewerPosition = 4;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 5 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset5);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));

            // Third GoToAsset
            expectedViewerPosition = 4;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 5 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset5);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            // GoToAsset 3
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));

            // Fourth GoToAsset
            expectedViewerPosition = 0;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset1);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(22));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            // GoToAsset 3
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            // GoToAsset 4
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
    public void GoToAsset_AssetIsNotInCurrentDirectory_DoesNothing()
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            _applicationViewModel!.GoToAsset(_asset6);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
    public void GoToAsset_NoObservableAssets_DoesNothing()
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 0 of 0 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset2);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                [],
                null,
                null!,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                [],
                null,
                null!,
                false,
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
    public void GoToAsset_AssetIsNull_ThrowsNullReferenceException()
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _applicationViewModel!.GoToAsset(null!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
    public void GoToAsset_AppModeAndAssetIsInCurrentDirectory_ChangesAppModeAndGoesToAsset()
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

            int expectedViewerPosition = 1;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - {_asset2.FileName} - image 2 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            // First GoToAsset
            _applicationViewModel!.GoToAsset(_asset2, AppMode.Viewer);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));

            // Second GoToAsset
            expectedViewerPosition = 4;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - {_asset5.FileName} - image 5 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset5, AppMode.Viewer);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));

            // Third GoToAsset
            expectedViewerPosition = 4;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 5 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset5, AppMode.Thumbnails);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                true,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(25));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            // GoToAsset 3
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));

            // Fourth GoToAsset
            expectedViewerPosition = 0;
            expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset1, AppMode.Thumbnails);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(30));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // GoToAsset 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            // GoToAsset 2
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            // GoToAsset 3
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            // GoToAsset 4
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("ViewerPosition"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
    [TestCase(AppMode.Thumbnails)]
    [TestCase(AppMode.Viewer)]
    public void GoToAsset_AppModeAndAssetIsNotInCurrentDirectory_DoesNothing(AppMode appMode)
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            _applicationViewModel!.GoToAsset(_asset6, appMode);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
    [TestCase(AppMode.Thumbnails)]
    [TestCase(AppMode.Viewer)]
    public void GoToAsset_AppModeAndNoObservableAssets_DoesNothing(AppMode appMode)
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 0 of 0 - sorted by file name ascending";

            _applicationViewModel!.GoToAsset(_asset2, appMode);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                [],
                null,
                null!,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                [],
                null,
                null!,
                false,
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
    [TestCase(AppMode.Thumbnails)]
    [TestCase(AppMode.Viewer)]
    public void GoToAsset_AppModeAndAssetIsNull_ThrowsNullReferenceException(AppMode appMode)
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

            const int expectedViewerPosition = 0;
            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {_dataDirectory!} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _applicationViewModel!.GoToAsset(null!, appMode));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                expectedAssets[expectedViewerPosition].Folder,
                false,
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
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        int expectedViewerPosition,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
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
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(string.Empty));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.EqualTo(expectedCanGoToPreviousAsset));
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(expectedAsset.ThumbnailCreationDateTime));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(assetPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(folderPath));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(expectedAsset.FileProperties.Creation.Date));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets, ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToPreviousAsset,
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
                expectedAppMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToPreviousAsset,
                expectedCanGoToNextAsset);
        }
    }
}
