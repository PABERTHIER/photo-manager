using System.Windows;

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
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_databasePath);
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
        configurationRootMock.MockGetValue(UserConfigurationKeys.FFMPEG_PATH, Constants.FFMPEG_PATH);

        _userConfigurationService = new (configurationRootMock.Object);
        _testableAssetRepository = new (_database!, _storageServiceMock!.Object, _userConfigurationService);
        StorageService storageService = new (_userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (_userConfigurationService);
        AssetCreationService assetCreationService = new (_testableAssetRepository, storageService, assetHashCalculatorService, _userConfigurationService);
        AssetsComparator assetsComparator = new (storageService);
        CatalogAssetsService catalogAssetsService = new (_testableAssetRepository, storageService, assetCreationService, _userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_testableAssetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_testableAssetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_testableAssetRepository, storageService, _userConfigurationService);
        PhotoManager.Application.Application application = new (_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    // TODO: Add the remaining cases to test all methods one by one + cases
    // TODO: Rename, reorder and rework all tests (adding what is missing...)

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name
    public void ChangeAppMode_NoParameter_ChangeAppMode(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Assert.AreEqual(AppModeEnum.Thumbnails, _applicationViewModel!.AppMode);
            _applicationViewModel!.ChangeAppMode();
            Assert.AreEqual(AppModeEnum.Viewer, _applicationViewModel!.AppMode);
            _applicationViewModel!.ChangeAppMode();
            Assert.AreEqual(AppModeEnum.Thumbnails, _applicationViewModel!.AppMode);
            _applicationViewModel!.ChangeAppMode(AppModeEnum.Viewer);
            Assert.AreEqual(AppModeEnum.Viewer, _applicationViewModel!.AppMode);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(AppModeEnum.Thumbnails, Visibility.Visible, false)]
    [TestCase(AppModeEnum.Thumbnails, Visibility.Visible, true)]
    [TestCase(AppModeEnum.Viewer, Visibility.Hidden, false)]
    [TestCase(AppModeEnum.Viewer, Visibility.Hidden, true)]
    // TODO: Update the name
    public void ThumbnailsVisible_ChangeAppMode_RefreshThumbnailsVisible(AppModeEnum appMode, Visibility visibilityExpected, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            _applicationViewModel!.ChangeAppMode(appMode);

            Assert.AreEqual(visibilityExpected, _applicationViewModel!.ThumbnailsVisible);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(AppModeEnum.Viewer, Visibility.Visible, false)]
    [TestCase(AppModeEnum.Viewer, Visibility.Visible, true)]
    [TestCase(AppModeEnum.Thumbnails, Visibility.Hidden, false)]
    [TestCase(AppModeEnum.Thumbnails, Visibility.Hidden, true)]
    // TODO: Update the name
    public void ViewerVisible_ChangeAppMode_RefreshViewerVisible(AppModeEnum appMode, Visibility visibilityExpected, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            _applicationViewModel!.ChangeAppMode(appMode);

            Assert.AreEqual(visibilityExpected, _applicationViewModel!.ViewerVisible);
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
    public void NotifyCatalogChange_CreatedToNonEmptyListCurrentFolder_AddNewAssetToList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Asset newAsset = new() { FileName = "NewImage.jpg", ImageData = new BitmapImage(), Folder = folder };
            const string statusMessage = "Creating thumbnail for NewImage.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Message = statusMessage,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(6, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
            Assert.AreEqual("NewImage.jpg", _applicationViewModel.ObservableAssets[5].FileName);
            Assert.AreEqual(statusMessage, _applicationViewModel.StatusMessage);
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
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Asset newAsset = new() { FileName = "NewImage.jpg", ImageData = new BitmapImage(), Folder = folder };
            const string statusMessage = "Creating thumbnail for NewImage.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Message = statusMessage,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(5, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
            Assert.AreEqual(statusMessage, _applicationViewModel.StatusMessage);
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
                new Asset {FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder},
                new Asset {FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder},
                new Asset {FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder},
                new Asset {FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder},
                new Asset {FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder}
            ];

            const string statusMessage = "Creating thumbnail for Image 5.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = cataloguedAssets[4],
                CataloguedAssetsByPath = cataloguedAssets,
                Message = statusMessage,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(5, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
            Assert.AreEqual(statusMessage, _applicationViewModel.StatusMessage);
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
            Asset newAsset = new() { FileName = "NewImage.jpg", ImageData = new BitmapImage(), Folder = folder };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(1, _applicationViewModel.ObservableAssets.Count);
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
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Folder newFolder = new() { Path = @"D:\NewFolder" };
            Asset newAsset = new() { FileName = "NewImage.jpg", ImageData = new BitmapImage(), Folder = newFolder };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(5, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
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
    public void NotifyCatalogChange_NullFolder_IgnoreNewAsset(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Asset newAsset = new() { FileName = "NewImage.jpg", ImageData = new BitmapImage(), Folder = null };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(null);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = null,
                Reason = ReasonEnum.AssetCreated
            });

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(5, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
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
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Asset newAsset = new()
            {
                FileName = fileName,
                ImageData = new BitmapImage(),
                Folder = new Folder { FolderId = folderId, Path = folderPath }
            };

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(null);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = null,
                Reason = ReasonEnum.AssetCreated
            });

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = newAsset,
                Reason = ReasonEnum.AssetCreated
            });

            Assert.AreEqual(5, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[4].FileName);
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
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            const string statusMessage = "Removing thumbnail for Image 3.jpg";

            _applicationViewModel!.SetAssets(assets);

            _applicationViewModel!.NotifyCatalogChange(new CatalogChangeCallbackEventArgs
            {
                Asset = assets[2],
                Message = statusMessage,
                Reason = ReasonEnum.AssetDeleted
            });

            Assert.AreEqual(4, _applicationViewModel.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel.ObservableAssets[3].FileName);
            Assert.AreEqual(statusMessage, _applicationViewModel.StatusMessage);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, 0, false)]
    [TestCase(0, 0, true)]
    [TestCase(1, 0, false)]
    [TestCase(1, 0, true)]
    [TestCase(2, 1, false)]
    [TestCase(2, 1, true)]
    [TestCase(3, 2, false)]
    [TestCase(3, 2, true)]
    [TestCase(4, 3, false)]
    [TestCase(4, 3, true)]
    // TODO: Update the name
    public void GoToPreviousAsset_ChangeViewerPosition(int currentPosition, int expectedViewerPosition, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            _applicationViewModel!.GoToPreviousAsset();

            Assert.AreEqual(expectedViewerPosition, _applicationViewModel!.ViewerPosition);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, false, false)]
    [TestCase(0, false, true)]
    [TestCase(1, true, false)]
    [TestCase(1, true, true)]
    [TestCase(2, true, false)]
    [TestCase(2, true, true)]
    [TestCase(3, true, false)]
    [TestCase(3, true, true)]
    [TestCase(4, true, false)]
    [TestCase(4, true, true)]
    // TODO: Update the name
    public void CanGoToPreviousAsset(int currentPosition, bool expectedCanGoToPreviousAsset, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            Assert.AreEqual(expectedCanGoToPreviousAsset, _applicationViewModel!.CanGoToPreviousAsset);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, true, false)]
    [TestCase(0, true, true)]
    [TestCase(1, true, false)]
    [TestCase(1, true, true)]
    [TestCase(2, true, false)]
    [TestCase(2, true, true)]
    [TestCase(3, true, false)]
    [TestCase(3, true, true)]
    [TestCase(4, false, false)]
    [TestCase(4, false, true)]
    // TODO: Update the name
    public void CanGoToNextAsset(int currentPosition, bool expectedCanGoToNextAsset, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            Assert.AreEqual(expectedCanGoToNextAsset, _applicationViewModel!.CanGoToNextAsset);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, 1, false)]
    [TestCase(0, 1, true)]
    [TestCase(1, 2, false)]
    [TestCase(1, 2, true)]
    [TestCase(2, 3, false)]
    [TestCase(2, 3, true)]
    [TestCase(3, 4, false)]
    [TestCase(3, 4, true)]
    [TestCase(4, 4, false)]
    [TestCase(4, 4, true)]
    // TODO: Update the name
    public void GoToNextAsset_ChangeViewerPosition(int currentPosition, int expectedViewerPosition, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            _applicationViewModel!.GoToNextAsset();

            Assert.AreEqual(expectedViewerPosition, _applicationViewModel!.ViewerPosition);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    // TODO: Add another test for case where file in the list but does not exist
    [Test]
    [TestCase(0, 0, false)]
    [TestCase(0, 0, true)]
    [TestCase(1, 0, false)]
    [TestCase(1, 0, true)]
    [TestCase(2, 0, false)]
    [TestCase(2, 0, true)]
    [TestCase(3, 0, false)]
    [TestCase(3, 0, true)]
    [TestCase(4, 0, false)]
    [TestCase(4, 0, true)]
    [TestCase(0, 1, false)]
    [TestCase(0, 1, true)]
    [TestCase(1, 1, false)]
    [TestCase(1, 1, true)]
    [TestCase(2, 1, false)]
    [TestCase(2, 1, true)]
    [TestCase(3, 1, false)]
    [TestCase(3, 1, true)]
    [TestCase(4, 1, false)]
    [TestCase(4, 1, true)]
    [TestCase(0, 2, false)]
    [TestCase(0, 2, true)]
    [TestCase(1, 2, false)]
    [TestCase(1, 2, true)]
    [TestCase(2, 2, false)]
    [TestCase(2, 2, true)]
    [TestCase(3, 2, false)]
    [TestCase(3, 2, true)]
    [TestCase(4, 2, false)]
    [TestCase(4, 2, true)]
    [TestCase(0, 3, false)]
    [TestCase(0, 3, true)]
    [TestCase(1, 3, false)]
    [TestCase(1, 3, true)]
    [TestCase(2, 3, false)]
    [TestCase(2, 3, true)]
    [TestCase(3, 3, false)]
    [TestCase(3, 3, true)]
    [TestCase(4, 3, false)]
    [TestCase(4, 3, true)]
    [TestCase(0, 4, false)]
    [TestCase(0, 4, true)]
    [TestCase(1, 4, false)]
    [TestCase(1, 4, true)]
    [TestCase(2, 4, false)]
    [TestCase(2, 4, true)]
    [TestCase(3, 4, false)]
    [TestCase(3, 4, true)]
    [TestCase(4, 4, false)]
    [TestCase(4, 4, true)]
    // TODO: Update the name
    public void GoToAsset_ChangeViewerPosition(int currentPosition, int goToAssetIndex, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            _applicationViewModel!.GoToAsset(assets[goToAssetIndex]);

            Assert.AreEqual(goToAssetIndex, _applicationViewModel!.ViewerPosition);
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
    public void GoToAsset_NotInList_KeepPosition(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Folder folder = _testableAssetRepository!.AddFolder(_dataDirectory!);

            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), Folder = folder },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), Folder = folder }
            ];

            Asset assetNotInList = new() { FileName = "ImageNotInList.jpg", ImageData = new BitmapImage(), Folder = folder };

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = 4;
            // TODO: Need to add assert about UpdateAppTitle (not updated)
            _applicationViewModel!.GoToAsset(assetNotInList);

            Assert.AreEqual(4, _applicationViewModel!.ViewerPosition);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(0, 0, 0, false)]
    [TestCase(0, 0, 0, true)]
    [TestCase(0, 1, 1, false)]
    [TestCase(0, 1, 1, true)]
    [TestCase(1, 2, 2, false)]
    [TestCase(1, 2, 2, true)]
    [TestCase(2, 3, 3, false)]
    [TestCase(2, 3, 3, true)]
    [TestCase(3, 4, 4, false)]
    [TestCase(3, 4, 4, true)]
    [TestCase(4, 5, 4, false)]
    [TestCase(4, 5, 4, true)]
    // TODO: Update the name
    public void RemoveAsset_AssetRemovedIsCurrent_RemoveFromAssetList(int index, int currentPosition, int expectedPosition, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = currentPosition;
            // TODO: Need to add assert about UpdateAppTitle (been updated)
            _applicationViewModel!.RemoveAssets([assets[index]]);

            Assert.AreEqual(expectedPosition, _applicationViewModel!.ViewerPosition);
            Assert.AreEqual(4, _applicationViewModel!.ObservableAssets.Count);
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
    public void RemoveAsset_SoleElement_ListBeEmptyAndPositionBeforeFirstItem(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = 0;

            _applicationViewModel!.RemoveAssets([assets[0]]);

            Assert.AreEqual(0, _applicationViewModel!.ViewerPosition);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name + Not null for a list !!!!
    public void RemoveAsset_NullAssetFromEmptyAssetList_EmptyAssetList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets = [];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = -1;

            _applicationViewModel!.RemoveAssets(null);

            Assert.AreEqual(-1, _applicationViewModel!.ViewerPosition);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name + Not null for a list !!!!
    public void RemoveAsset_NullAssetFromNullAssetList_NullAssetList(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            _applicationViewModel!.SetAssets(null);
            _applicationViewModel!.ViewerPosition = -1;

            _applicationViewModel!.RemoveAssets(null);

            Assert.AreEqual(-1, _applicationViewModel!.ViewerPosition);
            Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(AppModeEnum.Thumbnails, false)]
    [TestCase(AppModeEnum.Thumbnails, true)]
    [TestCase(AppModeEnum.Viewer, false)]
    [TestCase(AppModeEnum.Viewer, true)]
    // TODO: Update the name
    public void AppTitle_AppMode_ApplyAppTitleFormat(AppModeEnum appMode, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            string expectedTitle = appMode == AppModeEnum.Thumbnails
                ? $"PhotoManager v1.0.0.0 - {_dataDirectory} - image 4 of 5 - sorted by file name ascending"
                : $"PhotoManager v1.0.0.0 - {_dataDirectory} - Image 4.jpg - image 4 of 5 - sorted by file name ascending";

            Asset[] assets =
            [
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.Product = "PhotoManager";
            _applicationViewModel!.Version = "v1.0.0.0";
            _applicationViewModel!.ChangeAppMode(appMode);
            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.ViewerPosition = 3;

            Assert.AreEqual(expectedTitle, _applicationViewModel!.AppTitle);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    // TODO: Update the name + Not null for a list !!!!
    public void SortAssetsByCriteria_NullAssetList_AssetListIsNull(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets = null;

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileName);

            Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
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
    public void SortAssetsByCriteria_EmptyAssetList_AssetListIsEmpty(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets = [];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileName);

            Assert.IsEmpty(_applicationViewModel!.ObservableAssets);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    [TestCase(SortCriteriaEnum.FileCreationDateTime, false)]
    [TestCase(SortCriteriaEnum.FileCreationDateTime, true)]
    [TestCase(SortCriteriaEnum.FileModificationDateTime, false)]
    [TestCase(SortCriteriaEnum.FileModificationDateTime, true)]
    [TestCase(SortCriteriaEnum.FileName, false)]
    [TestCase(SortCriteriaEnum.FileName, true)]
    [TestCase(SortCriteriaEnum.FileSize, false)]
    [TestCase(SortCriteriaEnum.FileSize, true)]
    [TestCase(SortCriteriaEnum.ThumbnailCreationDateTime, false)]
    [TestCase(SortCriteriaEnum.ThumbnailCreationDateTime, true)]
    [TestCase(SortCriteriaEnum.Undefined, false)]
    [TestCase(SortCriteriaEnum.Undefined, true)]
    // TODO: Update the name
    public void SortAssetsByCriteria_SingleItemAssetList_AssetListHasSameSingleItem(SortCriteriaEnum sortCriteria, bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset
                {
                    FileName = "Image 1.jpg",
                    ImageData = new BitmapImage(),
                    FileSize = 2048,
                    FileCreationDateTime = DateTime.Now,
                    FileModificationDateTime = DateTime.Now,
                    ThumbnailCreationDateTime = DateTime.Now
                }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(sortCriteria);

            Assert.AreEqual(1, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
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
    public void SortAssetsByCriteria_ByFileNameMultipleAssetsList_AssetListSortedByFileName(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.Undefined);

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileName);

            Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
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
    public void SortAssetsByCriteria_ByFileNameMultipleAssetsListMultipleTimes_AssetListSortedByFileNameChangingSortingDirection(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage() },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage() }
            ];

            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.Undefined);
            _applicationViewModel!.SetAssets(assets);

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileName);

                Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);

                if (i % 2 == 0) // Ascending
                {
                    Assert.IsTrue(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
                else // Descending
                {
                    Assert.IsFalse(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
            }
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
    public void SortAssetsByCriteria_ByThumbnailCreationDateTimeMultipleAssetsList_AssetListSortedByThumbnailCreationDateTime(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.ThumbnailCreationDateTime);

            Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
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
    public void SortAssetsByCriteria_ByThumbnailCreationDateTimeMultipleAssetsListMultipleTimes_AssetListSortedByThumbnailCreationDateTimeChangingSortingDirection(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), ThumbnailCreationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.ThumbnailCreationDateTime);

                Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);

                if (i % 2 == 0) // Ascending
                {
                    Assert.IsTrue(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
                else // Descending
                {
                    Assert.IsFalse(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
            }
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
    public void SortAssetsByCriteria_ByFileCreationDateTimeMultipleAssetsList_AssetListSortedByFileCreationDateTime(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileCreationDateTime);

            Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
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
    public void SortAssetsByCriteria_ByFileCreationDateTimeMultipleAssetsListMultipleTimes_AssetListSortedByFileCreationDateTimeChangingSortingDirection(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileCreationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileCreationDateTime);

                Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);

                if (i % 2 == 0) // Ascending
                {
                    Assert.IsTrue(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
                else // Descending
                {
                    Assert.IsFalse(_applicationViewModel!.SortAscending);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
            }
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
    public void SortByFileModificationDateTimeMultipleFilesOneTimeTest(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileModificationDateTime);

            Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
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
    public void SortByFileModificationDateTimeMultipleFilesMultipleTimesTest(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2020, 6, 1) },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 2, 1) },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 1, 1) },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileModificationDateTime = new DateTime(2010, 8, 1) }
            ];

            _applicationViewModel!.SetAssets(assets);

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileModificationDateTime);

                Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);

                if (i % 2 == 0) // Ascending
                {
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
                else // Descending
                {
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
            }
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
    public void SortByFileSizeMultipleFilesOneTimeTest(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileSize = 2048 },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileSize = 2048 },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileSize = 2020 },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileSize = 2000 },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileSize = 2030 }
            ];

            _applicationViewModel!.SetAssets(assets);
            _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileSize);

            Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);
            Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
            Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
            Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
            Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
            Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
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
    public void SortByFileSizeMultipleFilesMultipleTimesTest(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        try
        {
            Asset[] assets =
            [
                new Asset { FileName = "Image 5.jpg", ImageData = new BitmapImage(), FileSize = 2048 },
                new Asset { FileName = "Image 2.jpg", ImageData = new BitmapImage(), FileSize = 2048 },
                new Asset { FileName = "Image 1.jpg", ImageData = new BitmapImage(), FileSize = 2020 },
                new Asset { FileName = "Image 3.jpg", ImageData = new BitmapImage(), FileSize = 2000 },
                new Asset { FileName = "Image 4.jpg", ImageData = new BitmapImage(), FileSize = 2030 }
            ];

            _applicationViewModel!.SetAssets(assets);

            for (int i = 0; i < 10; i++)
            {
                _applicationViewModel!.SortAssetsByCriteria(SortCriteriaEnum.FileSize);

                Assert.AreEqual(5, _applicationViewModel!.ObservableAssets.Count);

                if (i % 2 == 0) // Ascending
                {
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
                else // Descending
                {
                    Assert.AreEqual("Image 5.jpg", _applicationViewModel!.ObservableAssets[0].FileName);
                    Assert.AreEqual("Image 2.jpg", _applicationViewModel!.ObservableAssets[1].FileName);
                    Assert.AreEqual("Image 4.jpg", _applicationViewModel!.ObservableAssets[2].FileName);
                    Assert.AreEqual("Image 1.jpg", _applicationViewModel!.ObservableAssets[3].FileName);
                    Assert.AreEqual("Image 3.jpg", _applicationViewModel!.ObservableAssets[4].FileName);
                }
            }
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
