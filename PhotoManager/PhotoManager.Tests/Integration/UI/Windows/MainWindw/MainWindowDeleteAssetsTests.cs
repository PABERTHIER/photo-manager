using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.Windows.MainWindw;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Window
// The goal is to test what does MainWindow
[TestFixture]
public class MainWindowDeleteAssetsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private FolderNavigationViewModel? _folderNavigationViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    private Asset _asset1Temp;
    private Asset _asset2Temp;

    private Folder? _sourceFolder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = "Image 1.jpg",
            Pixel = new()
            {
                Asset = new() { Width = 1280, Height = 720 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new()
            {
                Size = 29857,
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "1fafae17c3c5c38d1205449eebdb9f5976814a5e54ec5797270c8ec467fe6d6d1190255cbaac11d9057c4b2697d90bc7116a46ed90c5ffb71e32e569c3b47fb9",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset2Temp = new()
        {
            FolderId = Guid.Empty, // Initialised later
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
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = Rotation.Rotate0,
            Hash = "bcc994c14aa314dbc2dfbf48ffd34fa628dadcd86cdb8efda113b94a9035f15956cf039f5858b74cd7f404e98f7e84d9821b39aaa6cbbdc73228fa74ad2a5c20",
            ImageData = new(),
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _sourceFolder = null;
        _folderNavigationViewModel = null;
    }

    private void ConfigureApplicationViewModel(
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

        _sourceFolder = new() { Id = Guid.NewGuid(), Path = _applicationViewModel!.CurrentFolderPath };
    }

    // DeleteDuplicatedAssets
    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndMultipleAssets_DeletesAssets()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            DeleteDuplicatedAssets([observableAssets[0], observableAssets[1]]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            // RemoveAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndOneAssetAndDeleteTwice_DeletesAssets()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            DeleteDuplicatedAssets([observableAssets[0]]);
            DeleteDuplicatedAssets([observableAssets[1]]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            // RemoveAssets 1
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            // RemoveAssets 2
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndOneAssetAndCurrentAsset_DeletesAsset()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset2Temp];

            DeleteDuplicatedAssets([_applicationViewModel.CurrentAsset!]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            // RemoveAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndOneAssetAndNotCurrentAsset_DeletesAsset()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp];

            DeleteDuplicatedAssets([observableAssets[1]]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(13));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            // RemoveAssets
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndEmptyArray_DoesNothing()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            DeleteDuplicatedAssets([]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            // RemoveAssets

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndUnknownAssetsAndFileExists_DeletesFiles()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            DeleteDuplicatedAssets([_asset1Temp, _asset2Temp]);

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_CataloguedAssetsAndOneAssetAndFileDoesNotExistAnymore_ThrowsFileNotFoundException()
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            string expectedAppTitle = $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            File.Delete(imagePath2ToCopy);

            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => DeleteDuplicatedAssets([observableAssets[1]]));

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{imagePath2ToCopy}'."));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    public async Task DeleteDuplicatedAssets_NoCataloguedAssets_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            string expectedAppTitle = $"PhotoManager v1.0.0 - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            DeleteDuplicatedAssets([]);

            assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                AppMode.Thumbnails,
                Visibility.Visible,
                Visibility.Hidden,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
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

    // DeleteSelectedAssets
    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndMultipleSelectedAsset_DeletesAssets(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[0], observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndOneSelectedAssetAndDeleteTwice_DeletesAssets(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            Asset[] expectedSelectedAssets = [_applicationViewModel.CurrentAsset!];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            expectedSelectedAssets = [_applicationViewModel.CurrentAsset!];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(17));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets 1
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
                // RemoveAssets 1
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
                // SelectedAssets 2
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("SelectedAssets"));
                // RemoveAssets 2
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("AppTitle"));
            }
            else
            {
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
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets 1
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
                // RemoveAssets 1
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
                // SelectedAssets 2
                Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("SelectedAssets"));
                // RemoveAssets 2
                Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndOneSelectedAssetAndCurrentAsset_DeletesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            Asset[] expectedSelectedAssets = [_applicationViewModel.CurrentAsset!];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 1 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} - {asset2TempFileName} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset2Temp];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset2Temp,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndOneSelectedAssetAndNotCurrentAsset_DeletesAsset(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 1 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} - {asset1TempFileName} - image 1 of 1 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(1));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(14));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(18));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
                // RemoveAssets
                Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndNoSelectedAssets_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} - {asset1TempFileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(11));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndUnknownSelectedAssetsAndFileExists_DeletesFiles(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            Asset[] expectedSelectedAssets = [_asset1Temp, _asset2Temp];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} - {asset1TempFileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(appMode == AppMode.Thumbnails ? "ShowImage for ThumbnailsUserControl" : "ShowImage for ViewerUserControl"));

            Assert.That(File.Exists(imagePath1ToCopy), Is.False);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.False);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.False);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_CataloguedAssetsAndOneSelectedAssetAndFileDoesNotExistAnymore_ThrowsFileNotFoundException(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string destinationDirectory = Path.Combine(_dataDirectory!, "DestinationToCopy");

        ConfigureApplicationViewModel(100, destinationDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);

            const string asset1TempFileName = "Image 1.jpg";
            const string asset2TempFileName = "Image 9.png";

            string imagePath1 = Path.Combine(_dataDirectory!, asset1TempFileName);
            string imagePath1ToCopy = Path.Combine(destinationDirectory, asset1TempFileName);

            string imagePath2 = Path.Combine(_dataDirectory!, asset2TempFileName);
            string imagePath2ToCopy = Path.Combine(destinationDirectory, asset2TempFileName);

            File.Copy(imagePath1, imagePath1ToCopy);
            File.Copy(imagePath2, imagePath2ToCopy);

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.True);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(destinationDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1Temp = _asset1Temp.WithFolder(folder!);
            _asset2Temp = _asset2Temp.WithFolder(folder!);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Is.Not.Empty);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            List<Asset> observableAssets = [.._applicationViewModel!.ObservableAssets];

            Asset[] expectedSelectedAssets = [observableAssets[1]];

            _applicationViewModel!.SelectedAssets = expectedSelectedAssets;

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {destinationDirectory} - image 1 of 2 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {destinationDirectory} - {asset1TempFileName} - image 1 of 2 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1Temp, _asset2Temp];

            File.Delete(imagePath2ToCopy);

            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            FileNotFoundException? exception = Assert.Throws<FileNotFoundException>(() => DeleteSelectedAssets());

            Assert.That(exception?.Message, Is.EqualTo($"File does not exist: '{imagePath2ToCopy}'."));

            Assert.That(File.Exists(imagePath1ToCopy), Is.True);
            Assert.That(File.Exists(imagePath2ToCopy), Is.False);

            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset1TempFileName), Is.True);
            Assert.That(_assetRepository!.ContainsThumbnail(folder.Path, asset2TempFileName), Is.True);

            assetsInRepository = _assetRepository!.GetAssetsByPath(destinationDirectory);
            Assert.That(assetsInRepository, Has.Length.EqualTo(2));
            Assert.That(assetsInRepository[0].FileName, Is.EqualTo(asset1TempFileName));
            Assert.That(assetsInRepository[1].FileName, Is.EqualTo(asset2TempFileName));

            CheckAfterChanges(
                _applicationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(12));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("SelectedAssets"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(16));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
                // SelectedAssets
                Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("SelectedAssets"));
            }

            CheckInstance(
                applicationViewModelInstances,
                destinationDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                _asset1Temp,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(destinationDirectory, true);
        }
    }

    [Test]
    [TestCase(AppMode.Thumbnails, Visibility.Visible, Visibility.Hidden)]
    [TestCase(AppMode.Viewer, Visibility.Hidden, Visibility.Visible)]
    public async Task DeleteSelectedAssets_NoCataloguedAssets_DoesNothing(
        AppMode appMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible)
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "TempEmptyFolder");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, true);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(assetsDirectory);

            MainWindowsInit();

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Asset[] assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            if (appMode == AppMode.Viewer)
            {
                _applicationViewModel!.ChangeAppMode();
            }

            string expectedAppTitle =
                appMode == AppMode.Thumbnails
                    ? $"PhotoManager v1.0.0 - {assetsDirectory} - image 0 of 0 - sorted by file name ascending"
                    : $"PhotoManager v1.0.0 - {assetsDirectory} -  - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];

            string result = DeleteSelectedAssets();

            Assert.That(result, Is.EqualTo(string.Empty));

            assetsInRepository = _assetRepository!.GetAssetsByPath(assetsDirectory);
            Assert.That(assetsInRepository, Is.Empty);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
                false,
                _sourceFolder!);

            if (appMode == AppMode.Thumbnails)
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            }
            else
            {
                Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
                // CatalogAssets + NotifyCatalogChange
                Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
                Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
                // ChangeAppMode
                Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("AppMode"));
                Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ThumbnailsVisible"));
                Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ViewerVisible"));
                Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("AppTitle"));
            }

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                appMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                [],
                expectedAppTitle,
                expectedAssets,
                null,
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
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.EqualTo(0));
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.AppTitle,
            Is.EqualTo($"PhotoManager v1.0.0 - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo("v1.0.0"));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(expectedAppMode));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(expectedThumbnailsVisible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(expectedViewerVisible));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        AssertSelectedAssets(expectedSelectedAssets, applicationViewModelInstance.SelectedAssets);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo("The catalog process has ended."));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo("v1.0.0"));
    }

    private static void CheckFolderNavigationViewModel(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset,
        Folder expectedSourceFolder)
    {
        CheckAfterChanges(
            folderNavigationViewModelInstance.ApplicationViewModel,
            expectedLastDirectoryInspected,
            expectedAppMode,
            expectedThumbnailsVisible,
            expectedViewerVisible,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedAssets,
            expectedCurrentAsset,
            expectedCanGoToNextAsset);

        Assert.That(folderNavigationViewModelInstance.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
        Assert.That(folderNavigationViewModelInstance.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);
        Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.False);
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.False);
        Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.Null);
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        AppMode expectedAppMode,
        Visibility expectedThumbnailsVisible,
        Visibility expectedViewerVisible,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
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
                expectedAppMode,
                expectedThumbnailsVisible,
                expectedViewerVisible,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedCanGoToNextAsset);
        }
    }

    private static void AssertSelectedAssets(Asset[] expectedAssets, Asset[] selectedAssets)
    {
        Assert.That(selectedAssets, Has.Length.EqualTo(expectedAssets.Length));

        for (int i = 0; i < selectedAssets.Length; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentSelectedAsset = selectedAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentSelectedAsset, currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            Assert.That(currentSelectedAsset.ImageData, Is.Not.Null);
        }
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

    private void MainWindowsInit()
    {
        _folderNavigationViewModel = new (
            _applicationViewModel!,
            _application!,
            _sourceFolder!,
            []);

        CancellationTokenSource cancellationTokenSource = new();

        Assert.That(cancellationTokenSource.IsCancellationRequested, Is.False);
        Assert.That(cancellationTokenSource.Token.CanBeCanceled, Is.True);
        Assert.That(cancellationTokenSource.Token.IsCancellationRequested, Is.False);
    }

    private void DeleteDuplicatedAssets(Asset[] assets)
    {
        if (assets.Length > 0)
        {
            DeleteAssets(assets);
        }
    }

    private string DeleteSelectedAssets()
    {
        Asset[] selectedAssets = _applicationViewModel!.SelectedAssets;

        if (selectedAssets.Length > 0)
        {
            DeleteAssets(selectedAssets);
            return ShowImage();
        }

        return string.Empty;
    }

    private void DeleteAssets(Asset[] assets)
    {
        _application!.DeleteAssets(assets); // TODO: Need to rework how the deletion is handled
        _applicationViewModel!.RemoveAssets(assets);
    }

    private string ShowImage()
    {
        return _applicationViewModel!.AppMode == AppMode.Viewer ? "ShowImage for ViewerUserControl" : "ShowImage for ThumbnailsUserControl";
    }
}
