using PhotoManager.UI.Models;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Unit.UI.Windows.FindDuplicatedAssetsWindw;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Window
// The goal is to test what does FindDuplicatedAssetsWindow
[TestFixture]
public class FindDuplicatedAssetsWindowDeleteLabelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

#pragma warning disable CS0067 // Event is never used
    private event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;
    private event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
#pragma warning restore CS0067 // Event is never used
    private event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssets;

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
    }

    [SetUp]
    public void Setup()
    {
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Set in each tests
            Folder = new() { Id = Guid.Empty, Path = "" }, // Set in each tests
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 2.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 3.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 4.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
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
            FileName = "Image 5.jpg",
            Pixel =
                new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
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

        _userConfigurationService = new (configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new (database, storageServiceMock.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, _userConfigurationService);
        PhotoManager.Application.Application application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _findDuplicatedAssetsViewModel = new (application);
    }

    [Test]
    public void DeleteLabel_DuplicatesAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndNewSetPositionAndNewAssetPositionAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 2;

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
            // DuplicatedAssetSetsPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset5);

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
    public void DeleteLabel_DuplicatesAndFirstAssetOfTheSetAndDuplicateIsInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0]);

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
            // DuplicatedAssetPosition updated
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndMiddleAssetOfTheSetAndDuplicateIsInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

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
    public void DeleteLabel_DuplicatesAndLastAssetOfTheSetAndDuplicateIsInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2]);

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
                Visible = Visibility.Collapsed,
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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

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
    public void DeleteLabel_DuplicatesAndFirstAssetOfTheSetAndDuplicateIsNotInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][0]);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

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
    public void DeleteLabel_DuplicatesAndMiddleAssetOfTheSetAndDuplicateIsNotInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][1]);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset4);

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
    public void DeleteLabel_DuplicatesAndLastAssetOfTheSetAndDuplicateIsNotInCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.DuplicatedAssetSets[1][2]);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

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
    public void DeleteLabel_DuplicatesAndOneSetWithMultipleAssetsAndDeleteAll_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4, _asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            // First Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Second Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[2]);

            expectedDuplicatedAssetSet = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 2

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Third Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                3,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
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
            // CollapseAssets 3
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);
            Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Fourth Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                3,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
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
            // CollapseAssets 3
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 4

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(4));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);
            Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);
            Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Fifth Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[4]);

            expectedDuplicatedAssetSet = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                3,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
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
            // CollapseAssets 3
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 4
            // CollapseAssets 5

            Assert.That(messagesInformationSent, Has.Count.EqualTo(2));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));
            Assert.That(messagesInformationSent[1].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[1].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(5));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);
            Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset3);
            Assert.That(deleteDuplicatedAssetsEvents[3], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[3][0], _asset2);
            Assert.That(deleteDuplicatedAssetsEvents[4], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[4][0], _asset5);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                3,
                expectedDuplicatedAssetSet,
                expectedDuplicatedAssetViewModel4);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void DeleteLabel_DuplicatesAndOneSetWithTwoAssetsAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndOneSetWithTwoAssetsAndNewAssetPositionAndCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

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

    [Test]
    public void DeleteLabel_DuplicatesAndOneSetWithTwoAssetsAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[1]);

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

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset3);

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
    public void DeleteLabel_DuplicatesAndOneSetWithTwoAssetsAndNewAssetPositionAndNotCurrentDuplicatedAsset_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAssetSet[0]);

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
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

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
            // DuplicatedAssetPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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

    [Test]
    public void DeleteLabel_DuplicatesAndTwoSetsAndDeleteAllTillNotVisibleAndFirstCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            // First Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Second Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet1 = [];
            expectedDuplicatedAssetSet2 = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                0,
                expectedDuplicatedAssetSet2,
                expectedDuplicatedAssetViewModel4);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
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
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset3);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Third Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet1 = [];
            expectedDuplicatedAssetSet2 = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
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
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 3
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset3);
            Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset2);

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
    public void DeleteLabel_DuplicatesAndTwoSetsAndDeleteAllTillNotVisibleAndSecondCurrentSet_SendsDeleteDuplicatedAssetsEventAndCollapsesAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;

            // First Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
            // DuplicatedAssetSetsPosition update
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Second Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet1 = [];
            expectedDuplicatedAssetSet2 = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 2
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(2));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

            // Third Delete
            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

            expectedDuplicatedAssetSet1 = [];
            expectedDuplicatedAssetSet2 = [];

            expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            expectedDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            expectedDuplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            expectedDuplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                expectedDuplicatedAssetSet1,
                expectedDuplicatedAssetViewModel1);

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
            // CollapseAssets 1
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 2
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets 3

            Assert.That(messagesInformationSent, Has.Count.EqualTo(1));
            Assert.That(messagesInformationSent[0].Message, Is.EqualTo("All duplicates have been deleted. \nGood Job ;)"));
            Assert.That(messagesInformationSent[0].Caption, Is.EqualTo("Information"));

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(3));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset2);
            Assert.That(deleteDuplicatedAssetsEvents[1], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[1][0], _asset4);
            Assert.That(deleteDuplicatedAssetsEvents[2], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[2][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndOneSetNotVisible_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2].Visible = Visibility.Collapsed;

            Delete(_findDuplicatedAssetsViewModel.CurrentDuplicatedAsset!);

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
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

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

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndNewSetPositionAndNewAssetPositionAndUnknownDuplicateFromNotVisibleSet_SendsDeleteDuplicatedAssetsEventAndResetsAssetPosition()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

            // Only one duplicate in the set, then set is not visible
            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel = new()
            {
                Asset = _asset1,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel);

            Delete(unknownDuplicatedAssetViewModel);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset4,
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
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

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
            // DuplicatedAssetSetsPosition updated
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentDuplicatedAsset"));
            // DuplicatedAssetPosition updated
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

            Assert.That(refreshAssetsCounterEvents, Is.Empty);

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
    public void DeleteLabel_DuplicatesAndNewAssetPositionAndUnknownDuplicateFromNotVisibleSet_SendsDeleteDuplicatedAssetsEventAndResetsAssetPosition()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset2 = _asset2.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 1;

            DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

            // Only one duplicate in the set, then set is not visible
            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel = new()
            {
                Asset = _asset1,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel);

            Delete(unknownDuplicatedAssetViewModel);

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
            // DuplicatedAssetPosition updated
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CurrentDuplicatedAsset"));
            // CollapseAssets

            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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

    [Test]
    public void DeleteLabel_DuplicatesAndUnknownDuplicateFromVisibleSet_SendsDeleteDuplicatedAssetsEventAndAndResetsPosition()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset2 = _asset2.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset2, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel3 = new()
            {
                Asset = _asset5,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel3);

            Delete(unknownDuplicatedAssetViewModel1);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_DuplicatesAndUnknownDuplicateFromNotVisibleSet_SendsDeleteDuplicatedAssetsEventAndResetsPosition()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset2 = _asset2.WithFolder(folder).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder).WithHash(hash);
            _asset5 = _asset5.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel unknownDuplicatedAssetSet = [];

            // Only one duplicate in the set, then set is not visible
            DuplicatedAssetViewModel unknownDuplicatedAssetViewModel = new()
            {
                Asset = _asset1,
                ParentViewModel = unknownDuplicatedAssetSet
            };
            unknownDuplicatedAssetSet.Add(unknownDuplicatedAssetViewModel);

            Delete(unknownDuplicatedAssetViewModel);

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
            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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
    public void DeleteLabel_NoDuplicates_SendsDeleteDuplicatedAssetsEventAndDoesNothing()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

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

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel);

            Delete(duplicatedAssetViewModel);

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

            Assert.That(getExemptedFolderPathEvents, Is.Empty);

            Assert.That(deleteDuplicatedAssetsEvents, Has.Count.EqualTo(1));
            Assert.That(deleteDuplicatedAssetsEvents[0], Has.Length.EqualTo(1));
            AssertAssetPropertyValidity(deleteDuplicatedAssetsEvents[0][0], _asset1);

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

        DeleteDuplicatedAssets += delegate(object _, Asset[] asset)
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

    private void Delete(DuplicatedAssetViewModel duplicatedAssetViewModel)
    {
        DeleteDuplicatedAssets?.Invoke(this, [duplicatedAssetViewModel.Asset]);

        _findDuplicatedAssetsViewModel!.CollapseAssets([duplicatedAssetViewModel]);
    }
}
