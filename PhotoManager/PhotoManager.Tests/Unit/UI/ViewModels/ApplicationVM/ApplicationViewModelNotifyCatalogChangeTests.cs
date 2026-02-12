using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelNotifyCatalogChangeTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private ApplicationViewModel? _applicationViewModel;
    private AssetRepository? _assetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

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
                Creation = new(2010, 1, 1, 20, 20, 20, 20, 20),
                Modification = new(2011, 1, 1, 20, 20, 20, 20, 20)
            },
            ThumbnailCreationDateTime = new(2010, 1, 1, 20, 20, 20, 20, 20),
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
                Creation = new(2020, 6, 1),
                Modification = new(2020, 7, 1)
            },
            ThumbnailCreationDateTime = new(2020, 6, 1),
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
                Creation = new(2010, 1, 1),
                Modification = new(2011, 1, 1)
            },
            ThumbnailCreationDateTime = new(2010, 1, 1),
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
                Creation = new(2010, 8, 1),
                Modification = new(2011, 9, 1)
            },
            ThumbnailCreationDateTime = new(2010, 8, 1),
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
                Creation = new(2020, 6, 1),
                Modification = new(2020, 7, 1)
            },
            ThumbnailCreationDateTime = new(2020, 6, 1),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    private void ConfigureApplicationViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new(userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, storageService, assetHashCalculatorService,
            userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, storageService, assetCreationService,
            userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            storageService);
        _applicationViewModel = new(application);
    }

    // AssetCreated SECTION (Start) ----------------------------------------------------------------------------------------------
    [Test]
    public void NotifyCatalogChange_CataloguedAssetsAndOneNewAssetAndCurrentFolder_NotifiesCatalogChangeAndAddsAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 6 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the newAsset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(newAsset, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!, newAsset];

            string statusMessage = $"Image {newAsset.Folder.Path} added to catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneNewAssetAndCurrentFolderAndPassingAllFields_NotifiesCatalogChangeAndAddsAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 6 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the newAsset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(newAsset, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!, newAsset];

            string statusMessage = $"Image {newAsset.Folder.Path} added to catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = newAsset,
                Folder = folder,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage,
                Exception = new()
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneNewAssetAndCurrentFolderAndNotPassingCataloguedAssetsByPath_NotifiesCatalogChangeAndAddsAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 6 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the newAsset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(newAsset, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!, newAsset];

            string statusMessage = $"Image {newAsset.Folder.Path} added to catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_NoCataloguedAssetsAndNewAssetsAndCurrentFolder_NotifiesCatalogChangeAndAddsAssets()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 1 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);
            _asset5 = _asset5!.WithFolder(folder);

            Asset[] assets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            // To Mock the ImageData because the assets are only notified and the files do not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);

            // First NotifyCatalogChange
            Asset[] expectedAssets = [_asset1];

            string statusMessage = $"Image {_asset1.Folder.Path} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = assets[0],
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            // Second NotifyCatalogChange
            expectedAssets = [_asset1, _asset2];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 2 - sorted by file name ascending";
            statusMessage = $"Image {_asset2.Folder.Path} added to catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = assets[1],
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));

            // Third NotifyCatalogChange
            expectedAssets = [_asset1, _asset2, _asset3];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 3 - sorted by file name ascending";
            statusMessage = $"Image {_asset3.Folder.Path} added to catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = assets[2],
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 3
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));

            // Fourth NotifyCatalogChange
            expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 4 - sorted by file name ascending";
            statusMessage = $"Image {_asset4.Folder.Path} added to catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = assets[3],
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 3
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 4
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));

            // Fifth NotifyCatalogChange
            expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";
            statusMessage = $"Image {_asset5.Folder.Path} added to catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = assets[4],
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 3
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 4
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 5
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_NoCataloguedAssetsAndOneNewAssetAndCurrentFolder_NotifiesCatalogChangeAndAddsAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 1 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the newAsset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(newAsset, assetData);

            Asset[] expectedAssets = [newAsset];

            string statusMessage = $"Image {newAsset.Folder.Path} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = [newAsset],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_CataloguedAssetsAndOneNewAssetAndNotCurrentFolder_UpdatesStatusMessage()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = $"Image {newAsset.Folder.Path} added to catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = newAsset,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_CataloguedAssetsAndNewAssetIsNullAndCurrentFolder_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = string.Empty;

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = null,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    // AssetCreated SECTION (End) ------------------------------------------------------------------------------------------------

    // AssetUpdated SECTION (Start) ----------------------------------------------------------------------------------------------
    [Test]
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneUpdatedAssetAndCurrentFolder_NotifiesCatalogChangeAndUpdatesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the updated asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!, _asset3!];

            string statusMessage = $"Image {_asset3!.Folder.Path} updated in catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneUpdatedAssetAndCurrentFolderAndPassingAllFields_NotifiesCatalogChangeAndUpdatesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the updated asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!, _asset3!];

            string statusMessage = $"Image {_asset3!.Folder.Path} updated in catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! },
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage,
                Exception = new()
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneUpdatedAssetAndCurrentFolderAndNotPassingCataloguedAssetsByPath_NotifiesCatalogChangeAndUpdatesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the updated asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!, _asset3!];

            string statusMessage = $"Image {_asset3!.Folder.Path} updated in catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndUpdatedAssetsAndCurrentFolder_NotifiesCatalogChangeAndUpdatesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);
            _asset5 = _asset5!.WithFolder(folder);

            Asset[] assets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            // To Mock the ImageData because the assets are only notified and the files do not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            // First NotifyCatalogChange
            Asset[] expectedAssets = [_asset1, _asset2, _asset4, _asset5, _asset3];

            string statusMessage = $"Image {_asset3.Folder.Path} updated in catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            // Second NotifyCatalogChange
            expectedAssets = [_asset2, _asset4, _asset5, _asset3, _asset1];

            statusMessage = $"Image {_asset1.Folder.Path} updated in catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset1,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));

            // Third NotifyCatalogChange
            expectedAssets = [_asset2, _asset4, _asset5, _asset3, _asset1];

            statusMessage = $"Image {_asset1.Folder.Path} updated in catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset1,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 3
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_OneCataloguedAssetAndOneUpdatedAssetAndCurrentFolder_NotifiesCatalogChangeAndUpdatesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 1 - sorted by file name ascending";

            // To Mock the ImageData because the updated asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset1!, assetData);

            Asset[] expectedAssets = [_asset1!];

            string statusMessage = $"Image {_asset1!.Folder.Path} updated in catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset1,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_NoCataloguedAssetsAndOneUpdatedAssetAndCurrentFolder_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_JPG, Height = PixelHeightAsset.IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_JPG, Height = ThumbnailHeightAsset.IMAGE_JPG }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the updated asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(asset, assetData);

            string statusMessage = $"Image {asset.Folder.Path} updated in catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_CataloguedAssetsAndOneUpdatedAssetAndNotCurrentFolder_UpdatesStatusMessage()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset updatedAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = $"Image {updatedAsset.Folder.Path} updated in catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = updatedAsset,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_CataloguedAssetsAndUpdatedAssetIsNullAndCurrentFolder_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = string.Empty;

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = null,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetUpdated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    // AssetUpdated SECTION (End) ------------------------------------------------------------------------------------------------

    // AssetDeleted SECTION (Start) ----------------------------------------------------------------------------------------------
    [Test]
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneDeletedAssetAndCurrentFolder_NotifiesCatalogChangeAndDeletesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 4 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the deleted asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!];

            string statusMessage = $"Image {_asset3!.Folder.Path} deleted from catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneDeletedAssetAndCurrentFolderAndPassingAllFields_NotifiesCatalogChangeAndDeletesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 4 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the deleted asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!];

            string statusMessage = $"Image {_asset3!.Folder.Path} deleted from catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! },
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage,
                Exception = new()
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndOneDeletedAssetAndCurrentFolderAndNotPassingCataloguedAssetsByPath_NotifiesCatalogChangeAndDeletesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 4 - sorted by file name ascending";

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            // To Mock the ImageData because the deleted asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset3!, assetData);

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset4!, _asset5!];

            string statusMessage = $"Image {_asset3!.Folder.Path} deleted from catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_CataloguedAssetsAndDeletedAssetsAndCurrentFolder_NotifiesCatalogChangeAndDeletesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 4 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);
            _asset5 = _asset5!.WithFolder(folder);

            Asset[] assets = [_asset1, _asset2, _asset3, _asset4, _asset5];

            // To Mock the ImageData because the assets are only notified and the files do not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(_asset1, assetData);
            _assetRepository.AddAsset(_asset2, assetData);
            _assetRepository.AddAsset(_asset3, assetData);
            _assetRepository.AddAsset(_asset4, assetData);
            _assetRepository.AddAsset(_asset5, assetData);

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            // First NotifyCatalogChange
            Asset[] expectedAssets = [_asset1, _asset2, _asset4, _asset5];

            string statusMessage = $"Image {_asset3.Folder.Path} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            // Second NotifyCatalogChange
            expectedAssets = [_asset2, _asset4, _asset5];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 3 - sorted by file name ascending";
            statusMessage = $"Image {_asset1.Folder.Path} deleted from catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset1,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));

            // Third NotifyCatalogChange
            expectedAssets = [_asset2, _asset4];

            expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 2 - sorted by file name ascending";
            statusMessage = $"Image {_asset5.Folder.Path} deleted from catalog.";

            catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset5,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 1
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 2
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange 3
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void
        NotifyCatalogChange_OneCataloguedAssetAndOneDeletedAssetAndCurrentFolder_NotifiesCatalogChangeAndDeletesAsset()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";

            // To Mock the ImageData because the deleted asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository!.AddAsset(_asset1!, assetData);

            Asset[] assets = [_asset1!];

            string statusMessage = $"Image {_asset1!.Folder.Path} deleted from catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset1,
                CataloguedAssetsByPath = [],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_NoCataloguedAssetsAndOneDeletedAssetAndCurrentFolder_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            Asset asset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.IMAGE_JPG, Height = PixelHeightAsset.IMAGE_JPG },
                    Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_JPG, Height = ThumbnailHeightAsset.IMAGE_JPG }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            // To Mock the ImageData because the deleted asset is the only one notified and the file does not exist
            byte[] assetData = [1, 2, 3];
            _assetRepository.AddAsset(asset, assetData);

            string statusMessage = $"Image {asset.Folder.Path} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = asset,
                CataloguedAssetsByPath = [asset],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_CataloguedAssetsAndOneDeletedAssetAndNotCurrentFolder_UpdatesStatusMessage()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            Asset deletedAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = FileNames.NEW_IMAGE_JPG,
                Pixel = new()
                {
                    Asset = new() { Width = PixelWidthAsset.NEW_IMAGE_JPG, Height = PixelHeightAsset.NEW_IMAGE_JPG },
                    Thumbnail = new()
                    {
                        Width = ThumbnailWidthAsset.NEW_IMAGE_JPG,
                        Height = ThumbnailHeightAsset.NEW_IMAGE_JPG
                    }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = $"Image {deletedAsset.Folder.Path} deleted from catalog.";

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = deletedAsset,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    public void NotifyCatalogChange_CataloguedAssetsAndDeletedAssetIsNullAndCurrentFolder_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            string statusMessage = string.Empty;

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = null,
                CataloguedAssetsByPath = [.. expectedAssets],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
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
    // AssetDeleted SECTION (End) ------------------------------------------------------------------------------------------------

    // FolderCreated SECTION (Start) ----------------------------------------------------------------------------------------------
    [Test]
    public void NotifyCatalogChange_FolderCreatedAndOtherDirectory_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderCreatedAndCurrentDirectory_NotifiesCatalogChangeAndEmitsEvent()
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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {_dataDirectory} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        NotifyCatalogChange_FolderCreatedAndCataloguedAssetsAndOtherDirectory_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} added to catalog.";

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        NotifyCatalogChange_FolderCreatedAndOtherDirectoryAndPassingAllFields_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Folder = folder,
                CataloguedAssetsByPath = [],
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage,
                Exception = new()
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(folder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderCreatedAndFolderIsNull_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = string.Empty;

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = null,
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderCreatedAndNoSubscribers_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {_dataDirectory} added to catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderCreated,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
    // FolderCreated SECTION (End) ------------------------------------------------------------------------------------------------

    // FolderDeleted SECTION (Start) ----------------------------------------------------------------------------------------------
    [Test]
    public void NotifyCatalogChange_FolderDeletedAndOtherDirectory_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderDeletedAndCurrentDirectory_NotifiesCatalogChangeAndEmitsEvent()
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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {_dataDirectory} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        NotifyCatalogChange_FolderDeletedAndCataloguedAssetsAndOtherDirectory_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} deleted from catalog.";

            Asset[] expectedAssets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];

            _applicationViewModel!.SetAssets(_dataDirectory!, expectedAssets);

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage
            };

            _applicationViewModel.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void
        NotifyCatalogChange_FolderDeletedAndOtherDirectoryAndPassingAllFields_NotifiesCatalogChangeAndEmitsEvent()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(otherDirectory);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {otherDirectory} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Asset = _asset3,
                Folder = folder,
                CataloguedAssetsByPath = [],
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage,
                Exception = new()
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);

            Assert.That(folderRemovedEvents, Has.Count.EqualTo(1));
            Assert.That(folderRemovedEvents[0], Is.EqualTo(folder));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderDeletedAndFolderIsNull_UpdatesStatusMessage()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = string.Empty;

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = null,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_FolderDeletedAndNoSubscribers_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = $"Folder {_dataDirectory} deleted from catalog.";

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Folder = folder,
                Reason = CatalogChangeReason.FolderDeleted,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
    // FolderDeleted SECTION (End) ------------------------------------------------------------------------------------------------

    [Test]
    [TestCase(CatalogChangeReason.FolderInspectionInProgress)]
    [TestCase(CatalogChangeReason.FolderInspectionCompleted)]
    [TestCase(CatalogChangeReason.AssetNotCreated)]
    [TestCase(CatalogChangeReason.BackupCreationStarted)]
    [TestCase(CatalogChangeReason.BackupUpdateStarted)]
    [TestCase(CatalogChangeReason.NoBackupChangesDetected)]
    [TestCase(CatalogChangeReason.BackupCompleted)]
    [TestCase(CatalogChangeReason.CatalogProcessCancelled)]
    [TestCase(CatalogChangeReason.CatalogProcessFailed)]
    [TestCase(CatalogChangeReason.CatalogProcessEnded)]
    public void NotifyCatalogChange_OtherReasons_UpdatesStatusMessage(CatalogChangeReason reason)
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";
            string statusMessage = string.Empty;

            CatalogChangeCallbackEventArgs catalogChangeCallbackEventArgs = new()
            {
                Reason = reason,
                Message = statusMessage
            };

            _applicationViewModel!.NotifyCatalogChange(catalogChangeCallbackEventArgs);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            // NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                statusMessage,
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
        }
    }

    [Test]
    public void NotifyCatalogChange_CatalogChangeCallbackEventArgsIsNull_ThrowsNullReferenceException()
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

            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {_dataDirectory} - image 0 of 0 - sorted by file name ascending";

            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() => _applicationViewModel!.NotifyCatalogChange(null!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedAppTitle,
                string.Empty,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedAppTitle,
                string.Empty,
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
        string? expectedStatusMessage,
        Asset[] expectedAssets,
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
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets,
            applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset,
                expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
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

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
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

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath,
                currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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
        string? expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
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
                expectedStatusMessage,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToNextAsset);
        }
    }
}
