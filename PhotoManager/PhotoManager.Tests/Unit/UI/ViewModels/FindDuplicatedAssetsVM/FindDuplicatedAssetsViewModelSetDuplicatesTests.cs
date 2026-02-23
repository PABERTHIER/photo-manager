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
public class FindDuplicatedAssetsViewModelSetDuplicatesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory()).Returns(_databasePath!);

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
        _findDuplicatedAssetsViewModel = new(application);
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHash_SetsDuplicates()
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

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHashAndCurrentAssetIsNotVisible_SetsDuplicates()
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

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            // Change CurrentDuplicatedAsset Visibility
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0].Visible = Visibility.Collapsed;

            duplicatedAssetSet1 = [];
            duplicatedAssetSet2 = [];

            duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                Visible = Visibility.Collapsed,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            duplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

            duplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

            duplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveSameHashAndSomeAssetsAreNotVisible_SetsDuplicates()
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

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

            // Change some DuplicatedAssetViewModel Visibility
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1].Visible = Visibility.Collapsed;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1].Visible = Visibility.Hidden;

            duplicatedAssetSet1 = [];
            duplicatedAssetSet2 = [];

            duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                Visible = Visibility.Collapsed,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            duplicatedAssetViewModel3 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel3);

            duplicatedAssetViewModel4 = new()
            {
                Asset = _asset4,
                Visible = Visibility.Hidden,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

            duplicatedAssetViewModel5 = new()
            {
                Asset = _asset5,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

            expectedDuplicatedAssetsSets = [duplicatedAssetSet1, duplicatedAssetSet2];

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndDuplicatesHaveNotSameHash_SetsDuplicatesWithoutCheck()
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
            _asset3 = _asset3!.WithFolder(folder2).WithHash(hash2);

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash1);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndCurrentAssetHaveImageDataNotLoaded_SetsDuplicatesAndRefresh()
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

            string folderPath = _dataDirectory!;
            Folder folder = _assetRepository!.AddFolder(folderPath);

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

            _asset2 = _asset2!.WithFolder(folder).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

            string filePath1 = Path.Combine(folderPath, _asset1.FileName);
            string filePath2 = Path.Combine(folderPath, _asset2.FileName);
            string filePath3 = Path.Combine(folderPath, _asset3.FileName);
            string filePath4 = Path.Combine(folderPath, _asset4.FileName);
            string filePath5 = Path.Combine(folderPath, _asset5.FileName);

            byte[] assetData1 = File.ReadAllBytes(filePath1);
            byte[] assetData2 = File.ReadAllBytes(filePath2);
            byte[] assetData3 = File.ReadAllBytes(filePath3);
            byte[] assetData4 = File.ReadAllBytes(filePath4);
            byte[] assetData5 = File.ReadAllBytes(filePath5);

            _assetRepository!.AddAsset(_asset1, assetData1);
            _assetRepository!.AddAsset(_asset2, assetData2);
            _assetRepository!.AddAsset(_asset3, assetData3);
            _assetRepository!.AddAsset(_asset4, assetData4);
            _assetRepository!.AddAsset(_asset5, assetData5);

            // Mock like the ImageData of _asset1 has become null
            _asset1.ImageData = null;

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            // SetDuplicates as load the ImageData of _asset1
            _asset1.ImageData = new();

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndCurrentAssetHaveNullImageData_SetsDuplicatesAndRefresh()
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

            const string hash1 = Hashes.IMAGE_1_JPG;
            const string hash2 = Hashes.IMAGE_9_DUPLICATE_PNG;

            _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
            _asset2 = _asset2!.WithFolder(folder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

            _asset4 = _asset4!.WithFolder(folder).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder).WithHash(hash2);

            byte[] assetData = [1, 2, 3];

            _assetRepository!.AddAsset(_asset2, assetData);
            _assetRepository!.AddAsset(_asset3, assetData);
            _assetRepository!.AddAsset(_asset4, assetData);
            _assetRepository!.AddAsset(_asset5, assetData);

            // Mock like the ImageData of _asset1 has become null
            _asset1.ImageData = null;

            DuplicatedSetViewModel duplicatedAssetSet1 = [];
            DuplicatedSetViewModel duplicatedAssetSet2 = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel3);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel2);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            // Refresh called by SetDuplicates
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CurrentDuplicatedAsset"));

            Assert.That(messagesInformationSent, Is.Empty);

            CheckInstance(
                findDuplicatedAssetsViewModelInstances,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel2);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsAndSomeNotCurrentAssetsHaveNullImageData_SetsDuplicatesAndDoesNotRefresh()
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

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

            _asset2.ImageData = null;
            _asset5.ImageData = null;

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsContainsSomeEmptyDuplicatedAssetsSetAndOneNotEmpty_SetsDuplicates()
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
            const string hash1 = Hashes.IMAGE_1_JPG;

            _asset1 = _asset1!.WithFolder(folder).WithHash(hash1);
            _asset3 = _asset3!.WithFolder(folder).WithHash(hash1);

            DuplicatedSetViewModel duplicatedAssetSet = [];

            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet
            };
            duplicatedAssetSet.Add(duplicatedAssetViewModel2);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [duplicatedAssetSet, [], []];
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [], []];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet,
                duplicatedAssetViewModel1);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                duplicatedAssetSet,
                duplicatedAssetViewModel1);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsOnlyContainsEmptyDuplicatedAssetsSet_DoesNothing()
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

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets = [[], []];
            List<List<Asset>> assetsSets = [[], []];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
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
                [],
                null);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void SetDuplicates_AssetsSetsIsEmpty_DoesNothing()
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

            List<List<Asset>> assetsSets = [];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

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
    public void SetDuplicates_AssetsSetsIsNull_ThrowsArgumentNullException()
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

            List<List<Asset>> assetsSets = null!;

            ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
                _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets));

            Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'assetsSets')"));
            Assert.That(exception?.ParamName, Is.EqualTo(nameof(assetsSets)));

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                [],
                0,
                0,
                [],
                null);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);
            Assert.That(messagesInformationSent, Is.Empty);
            Assert.That(findDuplicatedAssetsViewModelInstances, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // This test is only about checking if all props in FindDuplicatedAssetsViewModel are well set
    [Test]
    public void DuplicatedAssets_SetDuplicatesAndAssetsSetsAndDuplicatesHaveSameHash_SetsProperties()
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

            _asset2 = _asset2!.WithFolder(folder2).WithHash(hash2);
            _asset4 = _asset4!.WithFolder(folder1).WithHash(hash2);
            _asset5 = _asset5!.WithFolder(folder2).WithHash(hash2);

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
            List<List<Asset>> assetsSets = [[_asset1, _asset3], [_asset2, _asset4, _asset5]];

            _findDuplicatedAssetsViewModel!.SetDuplicates(assetsSets);

            // Check DuplicatedAssetSets
            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets, Has.Count.EqualTo(2));

            // Check first DuplicatedAssetSet
            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0],
                Has.Count.EqualTo(2));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
                Has.Count.EqualTo(2));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][0],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition
                ][
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition
                ],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[0]));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[0][1],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1]));

            _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition = 1;
            _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition = 2;

            // Check second DuplicatedAssetSet
            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1],
                Has.Count.EqualTo(3));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
                Has.Count.EqualTo(3));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAsset));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][2],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2]));

            Assert.That(
                _findDuplicatedAssetsViewModel!.DuplicatedAssetSets[
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetSetsPosition
                ][
                    _findDuplicatedAssetsViewModel!.DuplicatedAssetPosition
                ],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[2]));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][0],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[0]));

            Assert.That(_findDuplicatedAssetsViewModel!.DuplicatedAssetSets[1][1],
                Is.EqualTo(_findDuplicatedAssetsViewModel!.CurrentDuplicatedAssetSet[1]));

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                1,
                2,
                duplicatedAssetSet2,
                duplicatedAssetViewModel5);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
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
                2,
                duplicatedAssetSet2,
                duplicatedAssetViewModel5);
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

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent +=
            delegate (object _, MessageBoxInformationSentEventArgs e)
            {
                messagesInformationSent.Add(e);
            };

        return (notifyPropertyChangedEvents, messagesInformationSent, findDuplicatedAssetsViewModelInstances);
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

        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetSetsPosition,
            Is.EqualTo(expectedDuplicatedAssetSetsPosition));
        Assert.That(findDuplicatedAssetsViewModelInstance.DuplicatedAssetPosition,
            Is.EqualTo(expectedDuplicatedAssetPosition));

        AssertDuplicatedAssetsSet(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAssetSet,
            expectedCurrentDuplicatedAssetSet);
        AssertDuplicatedAsset(findDuplicatedAssetsViewModelInstance.CurrentDuplicatedAsset,
            expectedCurrentDuplicatedAsset);
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

    private static void AssertDuplicatedAssetsSet(DuplicatedSetViewModel duplicatedAssetSet,
        DuplicatedSetViewModel expectedDuplicatedAssetSet)
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

    private static void AssertDuplicatedAsset(DuplicatedAssetViewModel? duplicatedAsset,
        DuplicatedAssetViewModel? expectedDuplicatedAsset)
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
                    Assert.That(duplicatedAsset.ParentViewModel[i].Visible,
                        Is.EqualTo(expectedDuplicatedAsset.ParentViewModel[i].Visible));

                    AssertAssetPropertyValidity(duplicatedAsset.ParentViewModel[i].Asset,
                        expectedDuplicatedAsset.ParentViewModel[i].Asset);
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
