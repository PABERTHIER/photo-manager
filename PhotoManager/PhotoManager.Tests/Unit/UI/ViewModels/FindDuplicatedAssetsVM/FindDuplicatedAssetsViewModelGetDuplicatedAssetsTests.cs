using PhotoManager.UI.Models;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelGetDuplicatedAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [SetUp]
    public void Setup()
    {
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            Hash = string.Empty, // Set in each tests
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
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_2_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
            },
            Hash = string.Empty, // Set in each tests
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
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_3_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_3_JPG, Height = PixelHeightAsset.IMAGE_3_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_3_JPG, Height = ThumbnailHeightAsset.IMAGE_3_JPG }
            },
            Hash = string.Empty, // Set in each tests
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
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_4_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_4_JPG, Height = PixelHeightAsset.IMAGE_4_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_4_JPG, Height = ThumbnailHeightAsset.IMAGE_4_JPG }
            },
            Hash = string.Empty, // Set in each tests
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
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_5_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_5_JPG, Height = PixelHeightAsset.IMAGE_5_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_5_JPG, Height = ThumbnailHeightAsset.IMAGE_5_JPG }
            },
            Hash = string.Empty, // Set in each tests
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
        _asset6 = new()
        {
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = FileNames.IMAGE_5_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_5_JPG, Height = PixelHeightAsset.IMAGE_5_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_5_JPG, Height = ThumbnailHeightAsset.IMAGE_5_JPG }
            },
            Hash = string.Empty, // Set in each tests
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

    private void ConfigureFindDuplicatedAssetsViewModel(
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

        _userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(database, storageServiceMock.Object, _userConfigurationService);
        StorageService storageService = new(_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_assetRepository, storageService, _userConfigurationService);
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _findDuplicatedAssetsViewModel = new(application);
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndThreeSetsAndCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssetsInTheSet()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            // First GetDuplicatedAssets
            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset2);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            // Second GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);

            // Third GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 2;

            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel6);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update 1
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndThreeSetsAndCurrentDuplicatedAssetFromSecondSet_ReturnsOtherDuplicatedAssetsInTheSet()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            // First GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);

            // Second GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 2;

            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel6);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

            // Third GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 0;

            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset2);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update 1
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update 2
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndThreeSetsAndNotCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssetsInTheSet()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);

            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash3);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset1, _asset2], [_asset6, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            // First GetDuplicatedAssets
            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            // Second GetDuplicatedAssets
            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            // Third GetDuplicatedAssets
            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[2][1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset4);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndTwoSetsAndCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssetsInTheSet()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash1);

            _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset6], [_asset3, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            // First GetDuplicatedAssets
            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset2);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            // Second GetDuplicatedAssets
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel6);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition update 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndTwoSetsAndNotCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssetsInTheSet()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash1);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash1);

            _asset3 = _asset3!.WithFolder(folder3).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset6], [_asset3, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            // First GetDuplicatedAssets
            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset2);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            // Second GetDuplicatedAssets
            duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1].Asset);

            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel6);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithMultipleAssetsAndCurrentDuplicatedAsset_ReturnsNotCurrentDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(4));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset2);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset3);
            AssertDuplicatedAsset(duplicatedAssets[2], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(duplicatedAssets[2].Asset, _asset4);
            AssertDuplicatedAsset(duplicatedAssets[3], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(duplicatedAssets[3].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithMultipleAssetsAndNotCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string folder1Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);
            string folder2Directory = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(folder1Directory);
            Folder folder3 = _assetRepository!.AddFolder(folder2Directory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset2 = _asset2!.WithFolder(folder3).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(4));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset3);
            AssertDuplicatedAsset(duplicatedAssets[2], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(duplicatedAssets[2].Asset, _asset4);
            AssertDuplicatedAsset(duplicatedAssets[3], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(duplicatedAssets[3].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndFirstAssetIsTheCurrent_ReturnsNotCurrentDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset5);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndFirstAsset_ReturnsOtherDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset5);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndSecondAssetIsTheCurrent_ReturnsNotCurrentAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndSecondAsset_ReturnsOtherDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset6);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndThirdAssetIsTheCurrent_ReturnsNotCurrentAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 2;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                2,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel3);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                2,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel3);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithSomeAssetsWithSameNameAndThirdAsset_ReturnsOtherDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);
            AssertDuplicatedAsset(duplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[1].Asset, _asset5);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithTwoAssetsAndCurrentDuplicatedAsset_ReturnsNotCurrentDuplicatedAsset()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithTwoAssetsAndCurrentDuplicatedAssetAndNewAssetPosition_ReturnsNotCurrentDuplicatedAsset()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithTwoAssetsAndNotCurrentDuplicatedAsset_ReturnsCurrentDuplicatedAsset()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetWithTwoAssetsAndNotCurrentDuplicatedAssetAndNewAssetPosition_ReturnsCurrentDuplicatedAsset()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // This case cannot happen (having same file in same folder) and shows that those assets cannot be picked up
    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetAndCurrentDuplicatedAssetAndTwoAssetsWithSameNameInSameFolder_ReturnsEmptyList()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // This case cannot happen (having same file in same folder) and shows that those assets cannot be picked up
    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetAndNotCurrentDuplicatedAssetAndTwoAssetsWithSameNameInSameFolder_ReturnsEmptyList()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetNotVisibleAndCurrentDuplicatedAsset_ReturnsNotCurrentDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetNotVisibleAndNotCurrentDuplicatedAsset_ReturnsOtherDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetContainsOneAssetNotVisibleAndCurrentDuplicatedAsset_ReturnsVisibleDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!.Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset3);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndOneSetContainsOneAssetNotVisibleAndNotCurrentDuplicatedAsset_ReturnsVisibleDuplicatedAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1].Asset);

            Assert.That(duplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(duplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(duplicatedAssets[0].Asset, _asset1);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndDuplicatedAssetViewModelIsUnknown_ReturnsEmptyList()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel = new()
            {
                Asset = _asset1!,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel);

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(unknownDuplicatedAssetViewModel.Asset);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatesAndDuplicatedAssetViewModel_ReturnsEmptyList()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset1!,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            List<DuplicatedAssetViewModel> duplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_DuplicatesAndDuplicatedAssetViewModelIsNull_ThrowNullReferenceException()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
                _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(null!));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void GetDuplicatedAssets_NoDuplicatesAndDuplicatedAssetViewModelIsNull_ReturnsEmptyList()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<DuplicatedAssetViewModel> duplicatedAssets = _findDuplicatedAssetsViewModel!.GetDuplicatedAssets(null!);

            Assert.That(duplicatedAssets, Is.Empty);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                [],
                0,
                0,
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    private
        (
        List<string> notifyPropertyChangedEvents,
        List<MessageBoxInformationSentEventArgs> messagesInformationSent,
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        )
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances = [];

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent += delegate (object _, MessageBoxInformationSentEventArgs e)
        {
            messagesInformationSent.Add(e);
        };

        return (notifyPropertyChangedEvents, messagesInformationSent, findDuplicatedAssetsViewModelInstances);
    }

    private void CheckBeforeChanges()
    {
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetPosition, Is.EqualTo(0));
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset, Is.Null);
    }

    private static void CheckAfterChanges(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        AssertDuplicatedAssetSets(findDuplicatedAssetsViewModelInstance, expectedDuplicatedAssetSets);

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition, Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition, Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet, expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset, expectedCurrentDuplicatedAsset);
    }

    private static void AssertDuplicatedAssetSets(
        FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModelInstance,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets)
    {
        if (expectedDuplicatedAssetSets.Count > 0)
        {
            for (int i = 0; i < expectedDuplicatedAssetSets.Count; i++)
            {
                AssertDuplicatedAssetsSet(
                    findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets[i],
                    expectedDuplicatedAssetSets[i]);
            }
        }
        else
        {
            Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSets, Is.Empty);
        }
    }

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet, DuplicatedSetViewModel expectedDuplicatedAssetSet)
    {
        if (expectedDuplicatedAssetSet.Count > 0)
        {
            AssertDuplicatedSet(duplicatedAssetSet, expectedDuplicatedAssetSet);

            for (int i = 0; i < expectedDuplicatedAssetSet.Count; i++)
            {
                AssertDuplicatedAsset(duplicatedAssetSet[i], expectedDuplicatedAssetSet[i]);
            }
        }
        else
        {
            Assert.That(duplicatedAssetSet, Is.Empty);
        }
    }

    private static void AssertDuplicatedSet(
        DuplicatedSetViewModel duplicatedSetViewModel,
        DuplicatedSetViewModel expectedDuplicatedSetViewModel)
    {
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel.FileName));
        Assert.That(duplicatedSetViewModel.FileName, Is.EqualTo(expectedDuplicatedSetViewModel[0].Asset.FileName));

        Assert.That(duplicatedSetViewModel.DuplicatesCount, Is.EqualTo(expectedDuplicatedSetViewModel.DuplicatesCount));

        Assert.That(duplicatedSetViewModel.Visible, Is.EqualTo(expectedDuplicatedSetViewModel.Visible));
    }

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset, DuplicatedAssetViewModel? expectedDuplicatedAsset)
    {
        if (expectedDuplicatedAsset != null)
        {
            AssertAssetPropertyValidity(duplicatedAsset!.Asset, expectedDuplicatedAsset.Asset);

            Assert.That(duplicatedAsset.Visible, Is.EqualTo(expectedDuplicatedAsset.Visible));

            if (expectedDuplicatedAsset.ParentViewModel.Count > 0)
            {
                AssertDuplicatedSet(duplicatedAsset.ParentViewModel, expectedDuplicatedAsset.ParentViewModel);

                for (int i = 0; i < expectedDuplicatedAsset.ParentViewModel.Count; i++)
                {
                    Assert.That(duplicatedAsset.ParentViewModel[i].Visible, Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].Visible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset, expectedDuplicatedAsset.ParentViewModel[i].Asset);
                }
            }
            else
            {
                Assert.That(duplicatedAsset.ParentViewModel, Is.Empty);
            }
        }
        else
        {
            Assert.That(duplicatedAsset, Is.Null);
        }
    }

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset)
    {
        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(expectedAsset.Folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(expectedAsset.Folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(expectedAsset.ThumbnailCreationDateTime));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null);
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(expectedAsset.FullPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(expectedAsset.Folder.Path));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(expectedAsset.FileProperties.Creation.Date));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    private static void CheckInstance(
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances,
        List<DuplicatedSetViewModel> expectedDuplicatedAssetSets,
        int expectedDuplicatedAssetSetsPosition,
        int expectedDuplicatedAssetPosition,
        DuplicatedSetViewModel expectedCurrentDuplicatedAssetSet,
        DuplicatedAssetViewModel? expectedCurrentDuplicatedAsset)
    {
        int findDuplicatedAssetsViewModelInstancesCount = findDuplicatedAssetsViewModelInstances.Count;

        if (findDuplicatedAssetsViewModelInstancesCount > 1)
        {
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 1],
                Is.EqualTo(findDuplicatedAssetsViewModelInstances[findDuplicatedAssetsViewModelInstancesCount - 2]));
        }

        if (findDuplicatedAssetsViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                findDuplicatedAssetsViewModelInstances[0],
                expectedDuplicatedAssetSets,
                expectedDuplicatedAssetSetsPosition,
                expectedDuplicatedAssetPosition,
                expectedCurrentDuplicatedAssetSet,
                expectedCurrentDuplicatedAsset);
        }
    }
}
