using PhotoManager.UI.Models;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Unit.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelGetNotExemptedDuplicatedAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;
    private Asset _asset5;
    private Asset _asset6;

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
        _asset6 = new()
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
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _findDuplicatedAssetsViewModel = new (_application);
    }

    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSets_ReturnsAllOtherMatchingDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSetsAndNewPositions_ReturnsAllOtherMatchingDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel.DuplicatedAssetPosition = 1;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
            // DuplicatedAssetPosition update
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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSetsAndCurrentAsset_ReturnsMatchingDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSetsAndCurrentSet_ReturnsMatchingDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset1, _asset4], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetThatHasOneDuplicateAndThreeSets_ReturnsMatchingDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 2;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetThatHasDuplicates_ReturnsAllOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3.WithFolder(folder1).WithHash(hash);
            _asset4 = _asset4.WithFolder(folder2).WithHash(hash);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash);
            _asset5 = _asset5.WithFolder(folder1).WithHash(hash);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset1, _asset2, _asset3, _asset4, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel6);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(5));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset2);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset3);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[2], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[2].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[3], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[3].Asset, _asset5);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[4], expectedDuplicatedAssetViewModel6);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[4].Asset, _asset6);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatchingOfTwoAssetsDifferentFolders_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash);
            _asset2 = _asset2.WithFolder(folder2).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset2, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatchingOfTwoAssetsSameFolder_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4.WithFolder(folder).WithHash(hash);
            _asset2 = _asset2.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset2, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel4);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel2);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsTwoSameDuplicatesAndTwoSets_ReturnsMatchingAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash1);

            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset4, _asset1, _asset3], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsTwoSameDuplicatesAndOneSetMatching_ReturnsAssetInTheRootDirectory()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash);

            _asset4 = _asset4.WithFolder(folder).WithHash(hash);

            List<List<Asset>> assetsSets = [[_asset4, _asset1, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel3);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);

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

    // This case cannot happen (having same file in same folder)
    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasTwoAssetsWithSameNameAndTwoSetsOfDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset5 = _asset5.WithFolder(exemptedFolder).WithHash(hash2);
            _asset6 = _asset6.WithFolder(exemptedFolder).WithHash(hash2);

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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

    // This case cannot happen (having same file in same folder)
    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetMatchingAndTwoSetsOfDuplicatesWithTwoAssetsWithSameName_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset4 = _asset4.WithFolder(exemptedFolder).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash2);
            _asset6 = _asset6.WithFolder(folder1).WithHash(hash2);

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder2).WithHash(hash1);

            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset4, _asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset5);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel5);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset6);

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

    // Visibility start ------------------------------------------------------------------------------------------------
    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsAssetsThatHasOneCollapsedDuplicatesAndThreeSets_ReturnsOtherDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsOneAssetCollapsedThatHasOneDuplicatesAndThreeSets_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsOneCollapsedAssetAndOneDuplicateIsCollapsedAndTwoSets_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset5, _asset3]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset2);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel4);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset5);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsOneCollapsedAssetThatHasOneCollapsedDuplicateAndThreeSets_ReturnsOtherDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasTwoAssetsThatHaveDuplicatesAndThreeSetsAndAllOtherAssetsAreCollapsed_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset!.Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][1].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasTwoAssetsCollapsedThatHaveDuplicatesAndThreeSets_ReturnsAllOtherMatchingDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderContainsAssetsAndThreeSetsWithOneCollapsed_ReturnsOtherDuplicatedAssets()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "4e50d5c7f1a64b5d61422382ac822641ad4e5b943aca9ade955f4655f799558bb0ae9c342ee3ead0949b32019b25606bd16988381108f56bb6c6dd673edaa1e4";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash2);

            _asset4 = _asset4.WithFolder(folder2).WithHash(hash1);
            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);

            _asset5 = _asset5.WithFolder(folder1).WithHash(hash3);
            _asset6 = _asset6.WithFolder(folder2).WithHash(hash3);

            List<List<Asset>> assetsSets = [[_asset4, _asset1], [_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][0].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[2][1].Visible = Visibility.Collapsed;

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet3 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                Visible = Visibility.Collapsed,
                ParentViewModel = expectedDuplicatedAssetSet3
            };
            expectedDuplicatedAssetSet3.Add(expectedDuplicatedAssetViewModel5);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel6 = new (_application!)
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

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(2));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel1);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset4);
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[1], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[1].Asset, _asset2);

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
    // Visibility end --------------------------------------------------------------------------------------------------

    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasAssetsWithSomeDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "1cc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash3);
            _asset4 = _asset4.WithFolder(exemptedFolder).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5], [_asset1, _asset4]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasAssetsWithNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash3 = "1cc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";
            const string hash4 = "2cc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(exemptedFolder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(exemptedFolder).WithHash(hash3);
            _asset4 = _asset4.WithFolder(exemptedFolder).WithHash(hash4);

            _asset2 = _asset2.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetAndOneSetsContainsOneAssetWithSameNameAndHash_ReturnsOtherDuplicatedAsset()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset6 = _asset6.WithFolder(exemptedFolder).WithHash(hash1);

            _asset5 = _asset5.WithFolder(folder).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder).WithHash(hash2);
            _asset3 = _asset3.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset3], [_asset5, _asset6]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset6,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Has.Count.EqualTo(1));
            AssertDuplicatedAsset(notExemptedDuplicatedAssets[0], expectedDuplicatedAssetViewModel3);
            AssertAssetPropertyValidity(notExemptedDuplicatedAssets[0].Asset, _asset5);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderHasOneAssetAndOneSetsContainsOneAssetWithSameNameButDifferentHash_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder exemptedFolder = _assetRepository!.AddFolder(exemptedFolderPath);
            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset6 = _asset6.WithFolder(exemptedFolder).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet
            };
            expectedDuplicatedAssetSet.Add(expectedDuplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsTheSameAsTheAssetsDirectoryAndTwoSetsOfDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder1).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsTheSameAsTheOtherDirectoryAndTwoSetsOfDuplicates_ReturnsEmptyList()
    {
        string otherDirectory = Path.Combine(_dataDirectory!, "Folder1");

        string exemptedFolderPath = Path.Combine(otherDirectory);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

            Folder folder1 = _assetRepository!.AddFolder(_dataDirectory!);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder1).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder1).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder1).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder2).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder2).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsTheSameAsTheAssetsDirectoryAndOneSetOfDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!);

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            Folder folder = _assetRepository!.AddFolder(_dataDirectory!);

            const string hash1 = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9";
            const string hash2 = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20";

            _asset1 = _asset1.WithFolder(folder).WithHash(hash1);
            _asset3 = _asset3.WithFolder(folder).WithHash(hash1);
            _asset4 = _asset4.WithFolder(folder).WithHash(hash1);

            _asset2 = _asset2.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5.WithFolder(folder).WithHash(hash2);

            List<List<Asset>> assetsSets = [[_asset1, _asset3, _asset4], [_asset2, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderDoesNotContainAssetsAndDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

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

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderDoesNotContainAssetsAndNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "TestFolder");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderIsEmptyAndDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "Folder2");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

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

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderIsEmptyAndNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "Folder2");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Directory.CreateDirectory(exemptedFolderPath);

            Assert.That(Directory.Exists(exemptedFolderPath), Is.True);

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
            Directory.Delete(exemptedFolderPath, true);
        }
    }

    [Test]
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathDoesNotExistAndDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "Toto");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

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

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathDoesNotExistAndNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = Path.Combine(_dataDirectory!, "Toto");

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            Assert.That(Directory.Exists(exemptedFolderPath), Is.False);

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsEmptyAndDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = string.Empty;

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

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

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsEmptyAndNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = string.Empty;

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsNullAndDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = null!;

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

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

            DuplicatedSetViewModel expectedDuplicatedAssetSet1 = [];
            DuplicatedSetViewModel expectedDuplicatedAssetSet2 = [];

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel1 = new (_application!)
            {
                Asset = _asset1,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel1);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel2 = new (_application!)
            {
                Asset = _asset3,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel2);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel3 = new (_application!)
            {
                Asset = _asset4,
                ParentViewModel = expectedDuplicatedAssetSet1
            };
            expectedDuplicatedAssetSet1.Add(expectedDuplicatedAssetViewModel3);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel4 = new (_application!)
            {
                Asset = _asset2,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel4);

            DuplicatedAssetViewModel expectedDuplicatedAssetViewModel5 = new (_application!)
            {
                Asset = _asset5,
                ParentViewModel = expectedDuplicatedAssetSet2
            };
            expectedDuplicatedAssetSet2.Add(expectedDuplicatedAssetViewModel5);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [expectedDuplicatedAssetSet1, expectedDuplicatedAssetSet2];

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
    public void GetNotExemptedDuplicatedAssets_ExemptedFolderPathIsNullAndNoDuplicates_ReturnsEmptyList()
    {
        string exemptedFolderPath = null!;

        ConfigureFindDuplicatedAssetsViewModel(100, _dataDirectory!, exemptedFolderPath, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            List<DuplicatedAssetViewModel> notExemptedDuplicatedAssets =
                _findDuplicatedAssetsViewModel!.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            Assert.That(notExemptedDuplicatedAssets, Is.Empty);

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
