﻿using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelSortAssetsByCriteriaTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;
    private Asset _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        Guid folderId = Guid.NewGuid();

        _asset1 = new()
        {
            FolderId = folderId,
            Folder = new() { Id = folderId, Path = _dataDirectory! },
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 2.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 3.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 4.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 5.jpg",
            Pixel =
                new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
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
    [TestCase(SortCriteria.FileName, false)]
    [TestCase(SortCriteria.FileSize, true)]
    [TestCase(SortCriteria.FileCreationDateTime, true)]
    [TestCase(SortCriteria.FileModificationDateTime, true)]
    [TestCase(SortCriteria.ThumbnailCreationDateTime, true)]
    public void SortAssetsByCriteria_OneAsset_SortsByCriteria(SortCriteria sortCriteria, bool expectedSortAscending)
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

            Asset[] assets = [_asset1];

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            string expectedAppTitle = sortCriteria switch
            {
                SortCriteria.FileName => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by file name descending",
                SortCriteria.FileSize => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by file size ascending",
                SortCriteria.FileCreationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by file creation ascending",
                SortCriteria.FileModificationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by file modification ascending",
                SortCriteria.ThumbnailCreationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by thumbnail creation ascending",
                _ => $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by  ascending"
            };

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedSortAscending,
                sortCriteria,
                expectedAppTitle,
                assets,
                assets[0],
                assets[0].Folder,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedSortAscending,
                sortCriteria,
                expectedAppTitle,
                assets,
                assets[0],
                assets[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileName_SortsByFileName()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileName;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];
            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Asset[] expectedAssetsUpdated = [_asset5, _asset4, _asset3, _asset2, _asset1];
            string expectedAppTitleUpdated = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file name descending";

            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedAppTitleUpdated,
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                expectedAssetsUpdated[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedAppTitleUpdated,
                expectedAssetsUpdated,
                expectedAssetsUpdated[0],
                expectedAssetsUpdated[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileName_SortsByFileNameAscendingOrDescending()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileName;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAscendingAssets = [_asset1, _asset2, _asset3, _asset4, _asset5];
            string expectedAscendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file name ascending";

            Asset[] expectedDescendingAssets = [_asset5, _asset4, _asset3, _asset2, _asset1];
            string expectedDescendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file name descending";

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

                if (i % 2 == 0) // Descending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        false,
                        sortCriteria,
                        expectedDescendingAppTitle,
                        expectedDescendingAssets,
                        expectedDescendingAssets[0],
                        expectedDescendingAssets[0].Folder,
                        true);
                }
                else // Ascending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        true,
                        sortCriteria,
                        expectedAscendingAppTitle,
                        expectedAscendingAssets,
                        expectedAscendingAssets[0],
                        expectedAscendingAssets[0].Folder,
                        true);
                }
            }

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAscendingAppTitle,
                expectedAscendingAssets,
                expectedAscendingAssets[0],
                expectedAscendingAssets[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileSize_SortsByFileSizeAndThenByFileName()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileSize;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            Asset[] expectedAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file size ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileSize_SortsByFileSizeAscendingOrDescending()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileSize;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAscendingAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAscendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file size ascending";

            Asset[] expectedDescendingAssets = [_asset5, _asset2, _asset4, _asset1, _asset3];
            string expectedDescendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file size descending";

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

                if (i % 2 == 0) // Ascending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        true,
                        sortCriteria,
                        expectedAscendingAppTitle,
                        expectedAscendingAssets,
                        expectedAscendingAssets[0],
                        expectedAscendingAssets[0].Folder,
                        true);
                }
                else // Descending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        false,
                        sortCriteria,
                        expectedDescendingAppTitle,
                        expectedDescendingAssets,
                        expectedDescendingAssets[0],
                        expectedDescendingAssets[0].Folder,
                        true);
                }
            }

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedDescendingAppTitle,
                expectedDescendingAssets,
                expectedDescendingAssets[0],
                expectedDescendingAssets[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileCreationDateTime_SortsByFileCreationDateTimeAndThenByFileName()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileCreationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            Asset[] expectedAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file creation ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileCreationDateTime_SortsByFileCreationDateTimeAscendingOrDescending()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileCreationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAscendingAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAscendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file creation ascending";

            Asset[] expectedDescendingAssets = [_asset5, _asset2, _asset4, _asset1, _asset3];
            string expectedDescendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file creation descending";

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

                if (i % 2 == 0) // Ascending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        true,
                        sortCriteria,
                        expectedAscendingAppTitle,
                        expectedAscendingAssets,
                        expectedAscendingAssets[0],
                        expectedAscendingAssets[0].Folder,
                        true);
                }
                else // Descending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        false,
                        sortCriteria,
                        expectedDescendingAppTitle,
                        expectedDescendingAssets,
                        expectedDescendingAssets[0],
                        expectedDescendingAssets[0].Folder,
                        true);
                }
            }

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedDescendingAppTitle,
                expectedDescendingAssets,
                expectedDescendingAssets[0],
                expectedDescendingAssets[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByFileModificationDateTime_SortsByFileModificationDateTimeAndThenByFileName()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileModificationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            Asset[] expectedAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file modification ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByFileModificationDateTime_SortsByFileModificationDateTimeAscendingOrDescending()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.FileModificationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAscendingAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAscendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file modification ascending";

            Asset[] expectedDescendingAssets = [_asset5, _asset2, _asset4, _asset1, _asset3];
            string expectedDescendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by file modification descending";

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

                if (i % 2 == 0) // Ascending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        true,
                        sortCriteria,
                        expectedAscendingAppTitle,
                        expectedAscendingAssets,
                        expectedAscendingAssets[0],
                        expectedAscendingAssets[0].Folder,
                        true);
                }
                else // Descending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        false,
                        sortCriteria,
                        expectedDescendingAppTitle,
                        expectedDescendingAssets,
                        expectedDescendingAssets[0],
                        expectedDescendingAssets[0].Folder,
                        true);
                }
            }

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedDescendingAppTitle,
                expectedDescendingAssets,
                expectedDescendingAssets[0],
                expectedDescendingAssets[0].Folder,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndSortByThumbnailCreationDateTime_SortsByThumbnailCreationDateTimeAndThenByFileName()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.ThumbnailCreationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            Asset[] expectedAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by thumbnail creation ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedAssets[0],
                expectedAssets[0].Folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                sortCriteria,
                expectedAppTitle,
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
    public void SortAssetsByCriteria_MultiplesAssetsAndMultiplesSortsByThumbnailCreationDateTime_SortsByThumbnailCreationDateTimeAscendingOrDescending()
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

            Asset[] assets = [_asset5, _asset2, _asset1, _asset3, _asset4];

            const SortCriteria sortCriteria = SortCriteria.ThumbnailCreationDateTime;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            Asset[] expectedAscendingAssets = [_asset3, _asset1, _asset4, _asset2, _asset5];
            string expectedAscendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by thumbnail creation ascending";

            Asset[] expectedDescendingAssets = [_asset5, _asset2, _asset4, _asset1, _asset3];
            string expectedDescendingAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 5 - sorted by thumbnail creation descending";

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

                if (i % 2 == 0) // Ascending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        true,
                        sortCriteria,
                        expectedAscendingAppTitle,
                        expectedAscendingAssets,
                        expectedAscendingAssets[0],
                        expectedAscendingAssets[0].Folder,
                        true);
                }
                else // Descending
                {
                    CheckAfterChanges(
                        _applicationViewModel!,
                        _dataDirectory!,
                        false,
                        sortCriteria,
                        expectedDescendingAppTitle,
                        expectedDescendingAssets,
                        expectedDescendingAssets[0],
                        expectedDescendingAssets[0].Folder,
                        true);
                }
            }

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                false,
                sortCriteria,
                expectedDescendingAppTitle,
                expectedDescendingAssets,
                expectedDescendingAssets[0],
                expectedDescendingAssets[0].Folder,
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
    [TestCase(SortCriteria.FileName, false)]
    [TestCase(SortCriteria.FileSize, true)]
    [TestCase(SortCriteria.FileCreationDateTime, true)]
    [TestCase(SortCriteria.FileModificationDateTime, true)]
    [TestCase(SortCriteria.ThumbnailCreationDateTime, true)]
    public void SortAssetsByCriteria_NoCataloguedAssets_DoesNothing(SortCriteria sortCriteria, bool expectedSortAscending)
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

            Asset[] assets = [];

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            string expectedAppTitle = sortCriteria switch
            {
                SortCriteria.FileName => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by file name descending",
                SortCriteria.FileSize => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by file size ascending",
                SortCriteria.FileCreationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by file creation ascending",
                SortCriteria.FileModificationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by file modification ascending",
                SortCriteria.ThumbnailCreationDateTime => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by thumbnail creation ascending",
                _ => $"PhotoManager v1.0.0 - {_dataDirectory} - image 0 of 0 - sorted by  ascending"
            };

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                expectedSortAscending,
                sortCriteria,
                expectedAppTitle,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            // SortAssetsByCriteria
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                expectedSortAscending,
                sortCriteria,
                expectedAppTitle,
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
    public void SortAssetsByCriteria_InvalidSortCriteria_ThrowsArgumentOutOfRangeException()
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

            Asset[] assets = [_asset1];
            const SortCriteria invalidSortCriteria = (SortCriteria)999;

            _applicationViewModel!.SetAssets(_dataDirectory!, assets);

            ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => _applicationViewModel!.SortAssetsByCriteria(invalidSortCriteria));

            Assert.That(exception?.Message, Is.EqualTo($"Unknown sort criteria (Parameter '{nameof(SortCriteria)}')"));

            string expectedAppTitle = $"PhotoManager v1.0.0 - {_dataDirectory} - image 1 of 1 - sorted by file name ascending";

            CheckAfterChanges(
                _applicationViewModel!,
                _dataDirectory!,
                true,
                invalidSortCriteria,
                expectedAppTitle,
                assets,
                assets[0],
                assets[0].Folder,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("SortCriteria"));

            CheckInstance(
                applicationViewModelInstances,
                _dataDirectory!,
                true,
                invalidSortCriteria,
                expectedAppTitle,
                assets,
                assets[0],
                assets[0].Folder,
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
            Is.EqualTo($"PhotoManager v1.0.0 - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo("v1.0.0"));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        bool expectedSortAscending,
        SortCriteria expectedSortCriteria,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.EqualTo(expectedSortAscending));
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(expectedSortCriteria));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
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
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo("v1.0.0"));
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
        bool expectedSortAscending,
        SortCriteria expectedSortCriteria,
        string expectedAppTitle,
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
                expectedSortCriteria,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToNextAsset);
        }
    }
}
