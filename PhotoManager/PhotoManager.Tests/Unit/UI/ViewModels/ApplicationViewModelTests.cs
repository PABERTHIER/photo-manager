namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class ApplicationViewModelTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private Database? _database;
    private Mock<IStorageService>? _storageServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
        _storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
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

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        PhotoManager.Application.Application application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_CreatedToNonEmptyListCurrentFolder_AddNewAssetToList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = "NewImage.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            const string statusMessage = "Creating thumbnail for NewImage.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(6));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[5].FileName, Is.EqualTo("NewImage.jpg"));
            Assert.That(_applicationViewModel.StatusMessage, Is.EqualTo(statusMessage));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name + content of Notify not full
    public void NotifyCatalogChange_CreatedToNonEmptyListNotCurrentFolder_DoesNotAddNewAssetToList(bool analyseVideos)
    {
        string otherDirectory = Path.Combine(_dataDirectory!, "Duplicates");

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(otherDirectory);

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = "NewImage.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            const string statusMessage = "Creating thumbnail for NewImage.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(5));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(_applicationViewModel.StatusMessage, Is.EqualTo(statusMessage));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [Ignore("Not accurate anymore, now adding one asset at a time")]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_CreatedToEmptyListCurrentFolderWithCataloguedAssets_AddAllCataloguedAssetsToList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets = [];

            List<Asset> cataloguedAssets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            const string statusMessage = "Creating thumbnail for Image 5.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = cataloguedAssets[4],
                CataloguedAssetsByPath = cataloguedAssets,
                Reason = CatalogChangeReason.AssetCreated,
                Message = statusMessage
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(5));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(_applicationViewModel.StatusMessage, Is.EqualTo(statusMessage));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_CreatedToEmptyListCurrentFolderWithoutCataloguedAssets_AddNewAssetToList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets = [];
            Asset newAsset = new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = "NewImage.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(1));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_CreatedToNonEmptyListDifferentFolder_KeepExistingList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            Folder newFolder = new() { Id = Guid.NewGuid(), Path = @"D:\NewFolder" };
            Asset newAsset = new()
            {
                FolderId = newFolder.Id,
                Folder = newFolder,
                FileName = "NewImage.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new(),
            };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(5));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_DefaultFolder_IgnoreNewAsset(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            Asset newAsset = new()
            {
                FolderId = Guid.NewGuid(),
                Folder = new() { Id = Guid.Empty, Path = "" },
                FileName = "NewImage.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new()
            };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(null!);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = null,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(5));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase("", true, "", false)]
    [TestCase("", true, "", true)]
    [TestCase("NewImage.jpg", true, "", false)]
    [TestCase("NewImage.jpg", true, "", true)]
    [TestCase("", false, "", false)]
    [TestCase("", false, "", true)]
    [TestCase("", true, "NewFolder", false)]
    [TestCase("", true, "NewFolder", true)]
    [TestCase("NewImage.jpg", false, "", false)]
    [TestCase("NewImage.jpg", false, "", true)]
    [TestCase("NewImage.jpg", true, "C:\\NewFolder", false)]
    [TestCase("NewImage.jpg", true, "C:\\NewFolder", true)]
    // TODO: Update the name
    public void NotifyCatalogChange_InvalidParameters_IgnoreAsset(string fileName, bool isFolderIdEmpty, string folderPath, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);
            Guid folderId = isFolderIdEmpty ? Guid.Empty : Guid.NewGuid();

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            Asset newAsset = new()
            {
                FolderId = folderId,
                Folder = new() { Id = folderId, Path = folderPath },
                FileName = fileName,
                Pixel = new()
                {
                    Asset = new() { Width = 1280, Height = 720 },
                    Thumbnail = new() { Width = 200, Height = 112 }
                },
                Hash = string.Empty,
                ImageData = new BitmapImage()
            };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(null!);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = null,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(5));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 3.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[4].FileName, Is.EqualTo("Image 5.jpg"));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void NotifyCatalogChange_DeletedInFromCurrentFolder_RemoveFromAssetList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 1.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 2.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 3.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 4.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                },
                new()
                {
                    FolderId = folder.Id,
                    Folder = folder,
                    FileName = "Image 5.jpg",
                    Pixel = new()
                    {
                        Asset = new() { Width = 1280, Height = 720 },
                        Thumbnail = new() { Width = 200, Height = 112 }
                    },
                    Hash = string.Empty,
                    ImageData = new()
                }
            ];

            const string statusMessage = "Removing thumbnail for Image 3.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = assets[2],
                Reason = CatalogChangeReason.AssetDeleted,
                Message = statusMessage
            });

            Assert.That(_applicationViewModel.ObservableAssets, Has.Count.EqualTo(4));
            Assert.That(_applicationViewModel.ObservableAssets[0].FileName, Is.EqualTo("Image 1.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[1].FileName, Is.EqualTo("Image 2.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[2].FileName, Is.EqualTo("Image 4.jpg"));
            Assert.That(_applicationViewModel.ObservableAssets[3].FileName, Is.EqualTo("Image 5.jpg"));
            Assert.That(_applicationViewModel.StatusMessage, Is.EqualTo(statusMessage));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
