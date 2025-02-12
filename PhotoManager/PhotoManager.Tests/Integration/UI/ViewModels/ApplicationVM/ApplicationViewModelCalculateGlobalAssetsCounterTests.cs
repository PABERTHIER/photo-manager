using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelCalculateGlobalAssetsCounterTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private readonly DateTime _expectedFileModificationDateTime = new (2024, 06, 07, 08, 54, 37);
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;
    private AssetRepository? _assetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

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
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Initialised later
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
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
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
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
        _asset3 = new()
        {
            FolderId = Guid.Empty, // Initialised later
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
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
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
            FolderId = Guid.Empty, // Initialised later
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
                Creation = DateTime.Now,
                Modification = _expectedFileModificationDateTime
            },
            ThumbnailCreationDateTime = DateTime.Now,
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
        PhotoManager.Application.Application application = new (_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new (application);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CalculateGlobalAssetsCounter_CataloguedAssets_SetsGlobalAssetsCounterWording(bool analyseVideos)
    {
        string outputVideoFirstFrameDirectory = Path.Combine(_dataDirectory!, "OutputVideoFirstFrame");

        ConfigureApplicationViewModel(100, _dataDirectory!, 200, 150, false, false, false, analyseVideos);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            await _applicationViewModel!.CatalogAssets(_ => {});

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            int globalAssetsCounter = analyseVideos ? 52 : 51;

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!, $"Total number of assets: {globalAssetsCounter}");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, _dataDirectory!, $"Total number of assets: {globalAssetsCounter}");

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);

            if (analyseVideos)
            {
                Directory.Delete(outputVideoFirstFrameDirectory, true);
            }
        }
    }

    [Test]
    public void CalculateGlobalAssetsCounter_AssetsExist_SetsGlobalAssetsCounterWording()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 0");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 0");

            _assetRepository.AddAsset(_asset1!, []);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 1");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 1");

            _assetRepository.AddAsset(_asset2!, []);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 2");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 2");

            _assetRepository.AddAsset(_asset3!, []);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 3");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 3");

            _assetRepository.DeleteAsset(assetsDirectory, _asset3.FileName);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 2");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 2");

            _assetRepository.AddAsset(_asset4!, []);

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 3");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(6));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 3");

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
    public void CalculateGlobalAssetsCounter_NoAsset_SetsGlobalAssetsCounterWording()
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

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 0");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 0");

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

    [Test]
    public void CalculateGlobalAssetsCounter_ConcurrentAccess_SetsGlobalAssetsCounterWordingSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates\\NewFolder2");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Folder folder = _assetRepository!.AddFolder(assetsDirectory);

            _asset1 = _asset1!.WithFolder(folder);
            _asset2 = _asset2!.WithFolder(folder);
            _asset3 = _asset3!.WithFolder(folder);
            _asset4 = _asset4!.WithFolder(folder);

            _assetRepository.AddAsset(_asset1!, []);
            _assetRepository.AddAsset(_asset2!, []);
            _assetRepository.AddAsset(_asset3!, []);
            _assetRepository.AddAsset(_asset4!, []);

            // Simulate concurrent access
            Parallel.Invoke(
                () => _applicationViewModel!.CalculateGlobalAssetsCounter(),
                () => _applicationViewModel!.CalculateGlobalAssetsCounter(),
                () => _applicationViewModel!.CalculateGlobalAssetsCounter()
            );

            CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 4");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 4");

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
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
        Assert.That(_applicationViewModel!.CurrentFolder, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.Null);
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.Null);
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.Null);
        Assert.That(_applicationViewModel!.AppTitle, Is.EqualTo($"  - {expectedRootDirectory} - image 1 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.Null);
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.LastSelectedFolder, Is.Null); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedGlobalAssetsCounterWording)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.Product, Is.Null);
        Assert.That(applicationViewModelInstance.Version, Is.Null);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolder, Is.EqualTo(expectedLastDirectoryInspected));
        Assert.That(applicationViewModelInstance.ObservableAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(expectedGlobalAssetsCounterWording));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.Null);
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.Null);
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo($"  - {expectedLastDirectoryInspected} - image 1 of 0 - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.Null);
        Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        Assert.That(applicationViewModelInstance.LastSelectedFolder, Is.Null); // TODO: Should it be the root folder (add it in the ctor) ?
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.False);
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        string expectedGlobalAssetsCounterWording)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2], Is.EqualTo(applicationViewModelInstances[0]));
            // No need to go deeper, same instance because ref updated each time
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 1],
                Is.EqualTo(applicationViewModelInstances[applicationViewModelInstancesCount - 2]));
        }

        if (applicationViewModelInstancesCount > 0)
        {
            CheckAfterChanges(
                applicationViewModelInstances[0],
                expectedLastDirectoryInspected,
                expectedGlobalAssetsCounterWording);
        }
    }
}
