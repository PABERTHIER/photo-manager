using PhotoManager.UI.Models;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using Hashes = PhotoManager.Tests.Unit.Constants.Hashes;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelCollapseAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private AssetRepository? _assetRepository;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;
    private Asset _asset5;

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

    private void ConfigureFindDuplicatedAssetsViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth, int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
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
        _findDuplicatedAssetsViewModel = new (application);
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsAllAssetsAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsContainsAllAssetsAndDuplicatedAssetsAndNoSubscribers_CollapsesAssets()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, 200, 150, false, false, false, false);

        try
        {
            CheckBeforeChanges();

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
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
    public void CollapseAssets_DuplicatedAssetsContainsAllAssetsAndDuplicatedAssetSetsPositionIsOnLastSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsContainsTwoAssetsOfLastSetAndDuplicatedAssetSetsPositionIsOnLastSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

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
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

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
    public void CollapseAssets_DuplicatedAssetsContainsSecondAssetOfLastSetAndDuplicatedAssetSetsPositionIsOnLastSetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]]);

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
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsFirstAssetOfLastSetAndDuplicatedAssetSetsPositionIsOnLastSetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0]]);

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
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetSetsPosition
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                1,
                1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsAssetsInCurrentAndNotInCurrentSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

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
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsContainsAllAssetsInCurrentSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
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
                expectedDuplicatedAssetViewModel3);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsTwoAssetsInCurrentSetWithTheCurrentDuplicatedAssetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3, _asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
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
                Asset = _asset2,
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
            // CollapseAssets
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
    public void CollapseAssets_DuplicatedAssetsContainsAllAssetsNotInCurrentSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]
                ]);

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
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

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
            // CollapseAssets

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
    public void CollapseAssets_DuplicatedAssetsContainsAssetsOfEachSetsAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0]
                ]);

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
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                1,
                1,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetAndDuplicatedAssetsHasOnlyOneSet_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsContainsOnlyTheCurrentAssetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
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
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel3);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
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
                expectedDuplicatedAssetViewModel3);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsTheCurrentAssetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset2, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsTwoAssetsInCurrentSetAndDuplicatedAssetsAndCollapseSameAssetsTwice_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset2, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][2]
                ]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
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
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

            // Second Collapse
            _findDuplicatedAssetsViewModel!.CollapseAssets(
            [
                _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0],
                _findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][2]
            ]);

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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 2

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
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetInCurrentSetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset2, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][1]]);

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
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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
            // CollapseAssets

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
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetInCurrentSetAndDuplicatedAssetsAndCollapseSameAssetTwice_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset2, _asset3], [_asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]]);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
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

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            // Second Collapse
            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[0][0]]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(7));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 2

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                1,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsContainsTwoAssetsNotInCurrentSetAndDuplicatedAssets_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]
                ]);

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
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
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
            // CollapseAssets

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
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetNotInCurrentSetAndDuplicatedAssets_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]]);

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
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
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
            // CollapseAssets

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
    public void CollapseAssets_DuplicatedAssetsContainsTwoAssetsNotInCurrentSetAndDuplicatedAssetsAndCollapseSameAssetsTwice_CollapsesAssets()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets(
                [
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                    _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]
                ]);

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
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
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
            // CollapseAssets 1

            // Second Collapse
            _findDuplicatedAssetsViewModel!.CollapseAssets(
            [
                _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0],
                _findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]
            ]);

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
            // CollapseAssets 1
            // CollapseAssets 2

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
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetNotInCurrentSetAndDuplicatedAssetsAndCollapseSameAssetTwice_CollapsesAsset()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]]);

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
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
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
            // CollapseAssets 1

            // Second Collapse
            _findDuplicatedAssetsViewModel!.CollapseAssets([_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]]);

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
            // CollapseAssets 1
            // CollapseAssets 2

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
    public void CollapseAssets_DuplicatedAssetsAndNotDuplicatedAssets_DoesNothing()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            DuplicatedSetViewModel duplicatedAssetSet1 = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            _findDuplicatedAssetsViewModel!.CollapseAssets([duplicatedAssetViewModel1, duplicatedAssetViewModel2]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsContainsOneAssetAndNotDuplicatedAssets_DoesNothing()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            DuplicatedSetViewModel duplicatedAssetSet1 = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            _findDuplicatedAssetsViewModel!.CollapseAssets([duplicatedAssetViewModel2]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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
    public void CollapseAssets_DuplicatedAssetsIsEmptyAndDuplicatedAssets_DoesNothing()
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

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            DuplicatedSetViewModel duplicatedAssetSet1 = [];
            DuplicatedSetViewModel duplicatedAssetSet2 = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

            DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

            DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];
            List<List<Asset>> duplicatedAssetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            _findDuplicatedAssetsViewModel!.CollapseAssets([]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void CollapseAssets_DuplicatedAssetsIsEmptyAndNotDuplicatedAssets_DoesNothing()
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

            _findDuplicatedAssetsViewModel!.CollapseAssets([]);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

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

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent += delegate(object _, MessageBoxInformationSentEventArgs e)
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
