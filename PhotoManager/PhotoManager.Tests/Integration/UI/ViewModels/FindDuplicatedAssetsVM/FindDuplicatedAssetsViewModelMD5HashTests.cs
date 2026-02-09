using PhotoManager.UI.Models;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using MD5Hashes = PhotoManager.Tests.Integration.Constants.MD5Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.UI.ViewModels.FindDuplicatedAssetsVM;

[TestFixture]
public class FindDuplicatedAssetsViewModelMD5HashTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FindDuplicatedAssetsViewModel? _findDuplicatedAssetsViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;
    private Asset? _asset6;
    private Asset? _asset7;
    private Asset? _asset8;
    private Asset? _asset9;
    private Asset? _asset10;
    private Asset? _asset11;
    private Asset? _asset12;
    private Asset? _asset13;
    private Asset? _asset14;

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
        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_1_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_2_DUPLICATED_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_2_DUPLICATED_JPG, Height = PixelHeightAsset.IMAGE_2_DUPLICATED_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_DUPLICATED_JPG, Height = ThumbnailHeightAsset.IMAGE_2_DUPLICATED_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_2_DUPLICATED_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_2_DUPLICATED_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_2_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_2_JPG, Height = PixelHeightAsset.IMAGE_2_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_2_JPG, Height = ThumbnailHeightAsset.IMAGE_2_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_2_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_2_JPG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset4 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_9_PNG,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset5 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_11_HEIC,
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset6 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_JPG, Height = PixelHeightAsset.IMAGE_1_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_JPG, Height = ThumbnailHeightAsset.IMAGE_1_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_1_JPG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset7 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG, Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_1_DUPLICATE_JPG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset8 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_9_PNG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset9 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG, Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_9_DUPLICATE_PNG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset10 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_11_HEIC, Height = ThumbnailHeightAsset.IMAGE_11_HEIC }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_11_HEIC,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset11 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_JPG, Height = PixelHeightAsset._1336_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_JPG, Height = ThumbnailHeightAsset._1336_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes._1336_JPG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset12 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_ORIGINAL_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_ORIGINAL_JPG, Height = PixelHeightAsset._1336_ORIGINAL_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_ORIGINAL_JPG, Height = ThumbnailHeightAsset._1336_ORIGINAL_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_ORIGINAL_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes._1336_ORIGINAL_JPG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset13 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames._1336_4_K_ORIGINAL_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset._1336_4_K_ORIGINAL_JPG, Height = PixelHeightAsset._1336_4_K_ORIGINAL_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset._1336_4_K_ORIGINAL_JPG, Height = ThumbnailHeightAsset._1336_4_K_ORIGINAL_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize._1336_4_K_ORIGINAL_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes._1336_4_K_ORIGINAL_JPG,
            ImageData = null,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset14 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1336_ORIGINAL_JPG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_1336_ORIGINAL_JPG, Height = PixelHeightAsset.IMAGE_1336_ORIGINAL_JPG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_1336_ORIGINAL_JPG, Height = ThumbnailHeightAsset.IMAGE_1336_ORIGINAL_JPG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1336_ORIGINAL_JPG,
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = MD5Hashes.IMAGE_1336_ORIGINAL_JPG,
            ImageData = null,
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
        _application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new(_application);
        _findDuplicatedAssetsViewModel = new(_application);
    }

    [Test]
    public async Task SetDuplicates_CataloguedAssetsAndMD5HashTypeAndAllDuplicatesSets_SetsDuplicates()
    {
        string rootDirectory = Path.Combine(_dataDirectory!);
        string duplicatesDirectory = Path.Combine(rootDirectory, Directories.DUPLICATES);
        string directoryNewFolder1 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_1);
        string directoryNewFolder2 = Path.Combine(duplicatesDirectory, Directories.NEW_FOLDER_2);
        string directorySample1 = Path.Combine(duplicatesDirectory, Directories.NOT_DUPLICATE, Directories.SAMPLE_1);
        string directoryPart = Path.Combine(duplicatesDirectory, Directories.PART);
        string directoryResolution = Path.Combine(duplicatesDirectory, Directories.RESOLUTION);
        string directoryThumbnail = Path.Combine(duplicatesDirectory, Directories.THUMBNAIL);

        ConfigureFindDuplicatedAssetsViewModel(100, rootDirectory, 200, 150, false, true, false, true);

        string directoryOutputVideoFirstFrame = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            List<string> notifyApplicationVmPropertyChangedEvents,
            List<MessageBoxInformationSentEventArgs> messagesInformationSent,
            List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            List<List<Asset>> duplicatedAssetsSets = _application!.GetDuplicatedAssets();

            Assert.That(duplicatedAssetsSets, Has.Count.EqualTo(5));
            Assert.That(duplicatedAssetsSets[0], Has.Count.EqualTo(3));
            Assert.That(duplicatedAssetsSets[1], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[2], Has.Count.EqualTo(3));
            Assert.That(duplicatedAssetsSets[3], Has.Count.EqualTo(2));
            Assert.That(duplicatedAssetsSets[4], Has.Count.EqualTo(4));

            // Image 1
            Assert.That(duplicatedAssetsSets[0][0].FileName, Is.EqualTo(_asset1!.FileName));
            Assert.That(duplicatedAssetsSets[0][1].FileName, Is.EqualTo(_asset6!.FileName));
            Assert.That(duplicatedAssetsSets[0][2].FileName, Is.EqualTo(_asset7!.FileName));

            // Image 2
            Assert.That(duplicatedAssetsSets[1][0].FileName, Is.EqualTo(_asset2!.FileName));
            Assert.That(duplicatedAssetsSets[1][1].FileName, Is.EqualTo(_asset3!.FileName));

            // Image 9
            Assert.That(duplicatedAssetsSets[2][0].FileName, Is.EqualTo(_asset4!.FileName));
            Assert.That(duplicatedAssetsSets[2][1].FileName, Is.EqualTo(_asset8!.FileName));
            Assert.That(duplicatedAssetsSets[2][2].FileName, Is.EqualTo(_asset9!.FileName));

            // Image 11
            Assert.That(duplicatedAssetsSets[3][0].FileName, Is.EqualTo(_asset5!.FileName));
            Assert.That(duplicatedAssetsSets[3][1].FileName, Is.EqualTo(_asset10!.FileName));

            // Image 1336
            Assert.That(duplicatedAssetsSets[4][0].FileName, Is.EqualTo(_asset11!.FileName));
            Assert.That(duplicatedAssetsSets[4][1].FileName, Is.EqualTo(_asset12!.FileName));
            Assert.That(duplicatedAssetsSets[4][2].FileName, Is.EqualTo(_asset13!.FileName));
            Assert.That(duplicatedAssetsSets[4][3].FileName, Is.EqualTo(_asset14!.FileName));

            Folder? folder1 = _assetRepository!.GetFolderByPath(rootDirectory);
            Folder? folder2 = _assetRepository!.GetFolderByPath(directoryNewFolder1);
            Folder? folder3 = _assetRepository!.GetFolderByPath(directoryNewFolder2);
            Folder? folder4 = _assetRepository!.GetFolderByPath(directorySample1);
            Folder? folder5 = _assetRepository!.GetFolderByPath(directoryPart);
            Folder? folder6 = _assetRepository!.GetFolderByPath(directoryResolution);
            Folder? folder7 = _assetRepository!.GetFolderByPath(directoryThumbnail);

            Assert.That(folder1, Is.Not.Null);
            Assert.That(folder2, Is.Not.Null);
            Assert.That(folder3, Is.Not.Null);
            Assert.That(folder4, Is.Not.Null);
            Assert.That(folder5, Is.Not.Null);
            Assert.That(folder6, Is.Not.Null);
            Assert.That(folder7, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder1!);
            _asset2 = _asset2.WithFolder(folder1!);
            _asset3 = _asset3.WithFolder(folder1!);
            _asset4 = _asset4.WithFolder(folder1!);
            _asset5 = _asset5.WithFolder(folder1!);

            _asset6 = _asset6.WithFolder(folder2!);

            _asset7 = _asset7.WithFolder(folder3!);
            _asset8 = _asset8.WithFolder(folder3!);
            _asset9 = _asset9.WithFolder(folder3!);
            _asset10 = _asset10.WithFolder(folder3!);

            _asset11 = _asset11.WithFolder(folder4!);
            _asset12 = _asset12.WithFolder(folder5!);
            _asset13 = _asset13.WithFolder(folder6!);
            _asset14 = _asset14.WithFolder(folder7!);

            DuplicatedSetViewModel duplicatedAssetSet1 = [];
            DuplicatedSetViewModel duplicatedAssetSet2 = [];
            DuplicatedSetViewModel duplicatedAssetSet3 = [];
            DuplicatedSetViewModel duplicatedAssetSet4 = [];
            DuplicatedSetViewModel duplicatedAssetSet5 = [];

            // Image 1
            DuplicatedAssetViewModel duplicatedAssetViewModel1 = new()
            {
                Asset = _asset1,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel1);

            DuplicatedAssetViewModel duplicatedAssetViewModel2 = new()
            {
                Asset = _asset6,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel2);

            DuplicatedAssetViewModel duplicatedAssetViewModel3 = new()
            {
                Asset = _asset7,
                ParentViewModel = duplicatedAssetSet1
            };
            duplicatedAssetSet1.Add(duplicatedAssetViewModel3);

            // Image 2
            DuplicatedAssetViewModel duplicatedAssetViewModel4 = new()
            {
                Asset = _asset2,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel4);

            DuplicatedAssetViewModel duplicatedAssetViewModel5 = new()
            {
                Asset = _asset3,
                ParentViewModel = duplicatedAssetSet2
            };
            duplicatedAssetSet2.Add(duplicatedAssetViewModel5);

            // Image 9
            DuplicatedAssetViewModel duplicatedAssetViewModel6 = new()
            {
                Asset = _asset4,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel6);

            DuplicatedAssetViewModel duplicatedAssetViewModel7 = new()
            {
                Asset = _asset8,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel7);

            DuplicatedAssetViewModel duplicatedAssetViewModel8 = new()
            {
                Asset = _asset9,
                ParentViewModel = duplicatedAssetSet3
            };
            duplicatedAssetSet3.Add(duplicatedAssetViewModel8);

            // Image 11
            DuplicatedAssetViewModel duplicatedAssetViewModel9 = new()
            {
                Asset = _asset5,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel9);

            DuplicatedAssetViewModel duplicatedAssetViewModel10 = new()
            {
                Asset = _asset10,
                ParentViewModel = duplicatedAssetSet4
            };
            duplicatedAssetSet4.Add(duplicatedAssetViewModel10);

            // Image 1336
            DuplicatedAssetViewModel duplicatedAssetViewModel11 = new()
            {
                Asset = _asset11,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel11);

            DuplicatedAssetViewModel duplicatedAssetViewModel12 = new()
            {
                Asset = _asset12,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel12);

            DuplicatedAssetViewModel duplicatedAssetViewModel13 = new()
            {
                Asset = _asset13,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel13);

            DuplicatedAssetViewModel duplicatedAssetViewModel14 = new()
            {
                Asset = _asset14,
                ParentViewModel = duplicatedAssetSet5
            };
            duplicatedAssetSet5.Add(duplicatedAssetViewModel14);

            List<DuplicatedSetViewModel> expectedDuplicatedAssetsSets =
            [
                duplicatedAssetSet1,
                duplicatedAssetSet2,
                duplicatedAssetSet3,
                duplicatedAssetSet4,
                duplicatedAssetSet5
            ];

            _findDuplicatedAssetsViewModel!.SetDuplicates(duplicatedAssetsSets);

            CheckAfterChanges(
                _findDuplicatedAssetsViewModel!,
                expectedDuplicatedAssetsSets,
                0,
                0,
                duplicatedAssetSet1,
                duplicatedAssetViewModel1);

            Assert.That(notifyApplicationVmPropertyChangedEvents, Has.Count.EqualTo(149));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyApplicationVmPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[15], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[17], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[21], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[23], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[26], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[27], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[29], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[32], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[33], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[34], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[35], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[36], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[37], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[38], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[39], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[40], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[41], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[42], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[43], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[44], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[45], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[46], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[47], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[48], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[49], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[50], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[51], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[52], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[53], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[54], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[55], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[56], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[57], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[58], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[59], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[60], Is.EqualTo("AppTitle"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[61], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[62], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[63], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[64], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[65], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[66], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[67], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[68], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[69], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[70], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[71], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[72], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[73], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[74], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[75], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[76], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[77], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[78], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[79], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[80], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[81], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[82], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[83], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[84], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[85], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[86], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[87], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[88], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[89], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[90], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[91], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[92], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[93], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[94], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[95], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[96], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[97], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[98], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[99], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[100], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[101], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[102], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[103], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[104], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[105], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[106], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[107], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[108], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[109], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[110], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[111], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[112], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[113], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[114], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[115], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[116], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[117], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[118], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[119], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[120], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[121], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[122], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[123], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[124], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[125], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[126], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[127], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[128], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[129], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[130], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[131], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[132], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[133], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[134], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[135], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[136], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[137], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[138], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[139], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[140], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[141], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[142], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[143], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[144], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[145], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[146], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[147], Is.EqualTo("StatusMessage"));
            Assert.That(notifyApplicationVmPropertyChangedEvents[148], Is.EqualTo("StatusMessage"));

            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents, Has.Count.EqualTo(5));
            // SetDuplicates
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[0], Is.EqualTo("DuplicatedAssetSets"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[1], Is.EqualTo("DuplicatedAssetSetsPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[2], Is.EqualTo("CurrentDuplicatedAssetSet"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[3], Is.EqualTo("DuplicatedAssetPosition"));
            Assert.That(notifyFindDuplicatedAssetsVmPropertyChangedEvents[4], Is.EqualTo("CurrentDuplicatedAsset"));

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
            Directory.Delete(directoryOutputVideoFirstFrame, true);
        }
    }

    private
        (
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents,
        List<string> notifyApplicationVmPropertyChangedEvents,
        List<MessageBoxInformationSentEventArgs> messagesInformationSent,
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances
        )
        NotifyPropertyChangedEvents()
    {
        List<string> notifyFindDuplicatedAssetsVmPropertyChangedEvents = [];
        List<string> notifyApplicationVmPropertyChangedEvents = [];
        List<FindDuplicatedAssetsViewModel> findDuplicatedAssetsViewModelInstances = [];

        _findDuplicatedAssetsViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyFindDuplicatedAssetsVmPropertyChangedEvents.Add(e.PropertyName!);
            findDuplicatedAssetsViewModelInstances.Add((FindDuplicatedAssetsViewModel)sender!);
        };

        _applicationViewModel!.PropertyChanged += delegate (object? _, PropertyChangedEventArgs e)
        {
            notifyApplicationVmPropertyChangedEvents.Add(e.PropertyName!);
        };

        List<MessageBoxInformationSentEventArgs> messagesInformationSent = [];

        _findDuplicatedAssetsViewModel!.MessageBoxInformationSent += delegate (object _, MessageBoxInformationSentEventArgs e)
        {
            messagesInformationSent.Add(e);
        };

        return
        (
            notifyFindDuplicatedAssetsVmPropertyChangedEvents,
            notifyApplicationVmPropertyChangedEvents,
            messagesInformationSent,
            findDuplicatedAssetsViewModelInstances
        );
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
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(
            asset,
            expectedAsset,
            expectedAsset.FullPath,
            expectedAsset.Folder.Path,
            expectedAsset.Folder);
        // Unlike below (Application, CatalogAssetsService), it is set here for assets in the current directory
        Assert.That(asset.ImageData, expectedAsset.ImageData == null ? Is.Null : Is.Not.Null);
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
