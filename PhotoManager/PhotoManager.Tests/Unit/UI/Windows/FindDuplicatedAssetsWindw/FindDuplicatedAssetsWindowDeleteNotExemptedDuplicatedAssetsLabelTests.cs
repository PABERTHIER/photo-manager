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

namespace PhotoManager.Tests.Unit.UI.Windows.FindDuplicatedAssetsWindw;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Window
// The goal is to test what does FindDuplicatedAssetsWindow
[TestFixture]
public class FindDuplicatedAssetsWindowDeleteNotExemptedDuplicatedAssetsLabelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

#pragma warning disable CS0067 // Event is never used
    private event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;
#pragma warning restore CS0067 // Event is never used
    private event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
    private event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssets;

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
        string exemptedFolderPath,
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH, exemptedFolderPath);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(_userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        _assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, _userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(_assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(_assetRepository, fileOperationsService, _userConfigurationService);
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService,
            fileOperationsService, imageProcessingService);
        _findDuplicatedAssetsViewModel = new(application);
    }

    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesAllOtherMatchingDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSetsAndNewPositions_SendsDeleteDuplicatedAssetsEventAndCollapsesAllOtherMatchingDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel.DuplicatedAssetPosition = 1;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
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
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSetsAndCurrentAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesMatchingDuplicate()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSetsAndCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesMatchingDuplicate()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset1, _asset4], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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
                Asset = _asset4,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesMatchingDuplicate()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 2;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
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
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetThatHasDuplicates_SendsDeleteDuplicatedAssetsEventAndCollapsesAllOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(hash);
            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

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
            // CollapseAssets

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(5));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset3);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][2], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][3], _asset5);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][4], _asset6);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatchingOfTwoAssetsDifferentFolders_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash);
            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset2, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatchingOfTwoAssetsSameFolder_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);
            _asset2 = _asset2!.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset2, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsTwoSameDuplicatesAndTwoSets_SendsDeleteDuplicatedAssetsEventAndCollapsesMatchingAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset4, _asset1, _asset3], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatching_SendsDeleteDuplicatedAssetsEventAndCollapsesAssetsInTheRootDirectory()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4!.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

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
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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

    // This case cannot happen (having same file in same folder)
    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasTwoAssetsWithSameNameAndTwoSetsOfDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset5 = _asset5!.WithFolder(exemptedFolder).WithHash(hash2);
            _asset6 = _asset6!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset6,
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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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

    // This case cannot happen (having same file in same folder)
    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetMatchingAndTwoSetsOfDuplicatesWithTwoAssetsWithSameName_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset4 = _asset4!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash2);
            _asset6 = _asset6!.WithFolder(folder1).WithHash(hash2);

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash1);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset4, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset6,
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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset5);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset6);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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

    // Visibility start ------------------------------------------------------------------------------------------------
    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsAssetsThatHasOneCollapsedDuplicatesAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsOneAssetCollapsedThatHasOneDuplicatesAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsOneCollapsedAssetAndOneDuplicateIsCollapsedAndTwoSets_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset5, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2
                ];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset5);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsOneCollapsedAssetThatHasOneCollapsedDuplicateAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSetsAndAllOtherAssetsAreCollapsed_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][1].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasTwoAssetsCollapsedThatHaveDuplicatesAndThreeSets_SendsDeleteDuplicatedAssetsEventAndCollapsesAllOtherMatchingDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                2,
                0,
                expectedDuplicatedAssetSet3,
                expectedDuplicatedAssetViewModel5);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderContainsAssetsAndThreeSetsWithOneCollapsed_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicates()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_5_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4!.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5!.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6!.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][1].Visible = Visibility.Collapsed;

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset1,
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
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new()
            {
                Asset = _asset6,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
                [
                    expectedDuplicatedAssetSet1,
                    expectedDuplicatedAssetSet2,
                    expectedDuplicatedAssetSet3
                ];

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(2));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][1], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    // Visibility end --------------------------------------------------------------------------------------------------

    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasAssetsWithSomeDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_3_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash3);
            _asset4 = _asset4!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5], [_asset1, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasAssetsWithNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;
            const string hash3 = Hashes.IMAGE_3_JPG;
            const string hash4 = Hashes.IMAGE_4_JPG;

            _asset1 = _asset1!.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(exemptedFolder).WithHash(hash3);
            _asset4 = _asset4!.WithFolder(exemptedFolder).WithHash(hash4);

            _asset2 = _asset2!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

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
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetAndOneSetsContainsOneAssetWithSameNameAndHash_SendsDeleteDuplicatedAssetsEventAndCollapsesOtherDuplicate()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset6 = _asset6!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset5 = _asset5!.WithFolder(folder).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
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
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset5);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderHasOneAssetAndOneSetsContainsOneAssetWithSameNameButDifferentHash_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset6 = _asset6!.WithFolder(exemptedFolder).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

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
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsTheSameAsTheAssetsDirectoryAndTwoSetsOfDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsTheSameAsTheOtherDirectoryAndTwoSetsOfDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.FOLDER_1);

        string exemptedFolderPath = Path.Combine(otherDirectory);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder1).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsTheSameAsTheAssetsDirectoryAndOneSetOfDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderDoesNotContainAssetsAndDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

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

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderDoesNotContainAssetsAndNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.TEST_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderIsEmptyAndDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

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

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderIsEmptyAndNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.FOLDER_2);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathDoesNotExistAndDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

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

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathDoesNotExistAndNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, Directories.NON_EXISTENT_FOLDER);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(exemptedFolderPath));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsEmptyAndDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, string.Empty, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

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

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(string.Empty));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsEmptyAndNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, string.Empty, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.EqualTo(string.Empty));

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsNullAndDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, null!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

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

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.Null);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteAllNotExemptedLabel_ExemptedFolderPathIsNullAndNoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, null!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        List<string> getExemptedFolderPathEvents = NotifyGetExemptedFolderPath(_userConfigurationService!.PathSettings.ExemptedFolderPath);
        List<Asset[]> deleteDuplicatedAssetsEvents = NotifyDeleteDuplicatedAssets();
        List<string> refreshAssetsCounterEvents = NotifyRefreshAssetsCounter();

        try
        {
            CheckBeforeChanges();

            DeleteNotExemptedDuplicatedAssets();

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

            Assert.That(getExemptedFolderPathEvents, Has.Count.EqualTo(1));
            Assert.That(getExemptedFolderPathEvents[0], Is.Null);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Is.Empty);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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

    private List<string> NotifyGetExemptedFolderPath(string exemptedFolderPathToReturn)
    {
        List<string> getExemptedFolderPathEvents = [];

        GetExemptedFolderPath += delegate
        {
            getExemptedFolderPathEvents.Add(exemptedFolderPathToReturn);

            return exemptedFolderPathToReturn;
        };

        return getExemptedFolderPathEvents;
    }

    private List<Asset[]> NotifyDeleteDuplicatedAssets()
    {
        List<Asset[]> deleteDuplicatedAssetsEvents = [];

        DeleteDuplicatedAssets += delegate (object _, Asset[] asset)
        {
            deleteDuplicatedAssetsEvents.Add(asset);
        };

        return deleteDuplicatedAssetsEvents;
    }

    private List<string> NotifyRefreshAssetsCounter()
    {
        List<string> refreshAssetsCounterEvents = [];

        RefreshAssetsCounter += delegate
        {
            refreshAssetsCounterEvents.Add(string.Empty);
        };

        return refreshAssetsCounterEvents;
    }

    private void CheckBeforeChanges()
    {
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Is.Empty);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition, Is.Zero);
        Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetPosition, Is.Zero);
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

    private void DeleteNotExemptedDuplicatedAssets()
    {
        string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;

        List<DuplicatedAssetViewModel> assetsToDelete = _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        _findDuplicatedAssetsViewModel!.CollapseAssets(assetsToDelete);
    }
}
