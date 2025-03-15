using PhotoManager.UI;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelChangeAppModeTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;

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
        DateTime actualDate = DateTime.Now;

        _asset1 = new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1_duplicate.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
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
            FileName = "Image 9.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image 9_duplicate.png",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 126277,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
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
            FileName = "Image_11.heic",
            Pixel = new()
            {
                Asset = new() { Width = 3024, Height = 4032 },
                Thumbnail = new() { Width = 112, Height = 150 }
            },
            FileProperties = new()
            {
                Size = 1411940,
                Creation = actualDate,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = actualDate,
            ImageRotation = Rotation.Rotate0,
            Hash = "f52bd860f5ad7f81a92919e5fb5769d3e86778b2ade74832fbd3029435c85e59cb64b3c2ce425445a49917953e6e913c72b81e48976041a4439cb65e92baf18d",
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
        _assetRepository = new (database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new (userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new (userConfigurationService);
        AssetCreationService assetCreationService = new (_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new (_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new (_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new (_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new (_assetRepository, storageService, userConfigurationService);
        _application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (_application);
    }

    [Test]
    public async Task ChangeAppMode_CataloguedAssets_ChangesAppMode()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);
            _asset4 = _asset4.WithFolder(folder!);

            AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(App).Assembly);

            Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
            Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
            Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
            Assert.That(aboutInformation.Version, Does.StartWith("v"));
            Assert.That(aboutInformation.Version, Is.EqualTo("v1.0.0"));

            _applicationViewModel!.Product = aboutInformation.Product;
            _applicationViewModel!.Version = aboutInformation.Version;

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - {_asset1.FileName} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            _applicationViewModel!.ChangeAppMode();

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - {_asset1.FileName} - image 1 of 4 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode(AppMode.Viewer);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(25));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode(AppMode.Thumbnails);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(29));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            // ChangeAppMode 3
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - {_asset1.FileName} - image 1 of 4 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode();

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(33));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            // ChangeAppMode 3
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("AppTitle"));
            // ChangeAppMode 4
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[0],
                folder,
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
    public async Task ChangeAppMode_NoCataloguedAssets_ChangesAppMode()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            AboutInformation aboutInformation = _application!.GetAboutInformation(typeof(App).Assembly);

            Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
            Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
            Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
            Assert.That(aboutInformation.Version, Does.StartWith("v"));
            Assert.That(aboutInformation.Version, Is.EqualTo("v1.0.0"));

            _applicationViewModel!.Product = aboutInformation.Product;
            _applicationViewModel!.Version = aboutInformation.Version;

            const string expectedStatusMessage = "The catalog process has ended.";
            string expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} -  - image 1 of 0 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode();

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} -  - image 1 of 0 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode(AppMode.Viewer);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - image 1 of 0 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode(AppMode.Thumbnails);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            // ChangeAppMode 3
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));

            expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} -  - image 1 of 0 - sorted by file name ascending";

            _applicationViewModel!.ChangeAppMode();

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // ChangeAppMode 1
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            // ChangeAppMode 2
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            // ChangeAppMode 3
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            // ChangeAppMode 4
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppMode"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("ThumbnailsVisible"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ViewerVisible"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                "PhotoManager",
                "v1.0.0",
                AppMode.Viewer,
                Visibility.Hidden,
                Visibility.Visible,
                expectedAppTitle,
                expectedStatusMessage,
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
            Directory.Delete(assetsDirectory, true);
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
        Assert.That(_applicationViewModel!.Product, Is.Null);
        Assert.That(_applicationViewModel!.Version, Is.Null);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.EqualTo(0));
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.Null);
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.Null);
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.Null);
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo($"  - {expectedRootDirectory} - image 1 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.Null);
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        string? expectedProduct,
        string? expectedVersion,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.Product, Is.EqualTo(expectedProduct));
        Assert.That(applicationViewModelInstance.Version, Is.EqualTo(expectedVersion));
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(expectedAppMode));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(expectedThumbnailsVisible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(expectedViewerVisible));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.Null);
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.Null);
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.Null);
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset, expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets, ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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
        string? expectedProduct,
        string? expectedVersion,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        string expectedAppTitle,
        string expectedStatusMessage,
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
                expectedProduct,
                expectedVersion,
                expectedAppMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToNextAsset);
        }
    }
}
