using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelCalculateGlobalAssetsCounterTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [SetUp]
    public void SetUp()
    {
        _asset1 = new()
        {
            FolderId = Guid.Empty, // Initialised later
            Folder = new() { Id = Guid.Empty, Path = "" }, // Initialised later
            FileName = FileNames.IMAGE_1_DUPLICATE_JPG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = PixelHeightAsset.IMAGE_1_DUPLICATE_JPG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_1_DUPLICATE_JPG,
                    Height = ThumbnailHeightAsset.IMAGE_1_DUPLICATE_JPG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_1_DUPLICATE_JPG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
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
            FileName = FileNames.IMAGE_9_PNG,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_9_PNG, Height = PixelHeightAsset.IMAGE_9_PNG },
                Thumbnail = new() { Width = ThumbnailWidthAsset.IMAGE_9_PNG, Height = ThumbnailHeightAsset.IMAGE_9_PNG }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_9_PNG,
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
            FileName = FileNames.IMAGE_9_DUPLICATE_PNG,
            Pixel = new()
            {
                Asset = new()
                {
                    Width = PixelWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = PixelHeightAsset.IMAGE_9_DUPLICATE_PNG
                },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_9_DUPLICATE_PNG,
                    Height = ThumbnailHeightAsset.IMAGE_9_DUPLICATE_PNG
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_9_DUPLICATE_PNG,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
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
            FileName = FileNames.IMAGE_11_HEIC,
            Pixel = new()
            {
                Asset = new() { Width = PixelWidthAsset.IMAGE_11_HEIC, Height = PixelHeightAsset.IMAGE_11_HEIC },
                Thumbnail = new()
                {
                    Width = ThumbnailWidthAsset.IMAGE_11_HEIC,
                    Height = ThumbnailHeightAsset.IMAGE_11_HEIC
                }
            },
            FileProperties = new()
            {
                Size = FileSize.IMAGE_11_HEIC,
                Creation = DateTime.Now,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = DateTime.Now,
            ImageRotation = ImageRotation.Rotate0,
            Hash = Hashes.IMAGE_11_HEIC,
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
        _testableAssetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplicationViewModel(int catalogBatchSize, string assetsDirectory, int thumbnailMaxWidth,
        int thumbnailMaxHeight, bool usingDHash, bool usingMD5Hash, bool usingPHash, bool analyseVideos)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_DHASH, usingDHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_MD5_HASH, usingMD5Hash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.USING_PHASH, usingPHash.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ANALYSE_VIDEOS, analyseVideos.ToString());

        _userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, _userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ImageMagickThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            _userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, _userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            _userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, _userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);
        _applicationViewModel = new(application);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task CalculateGlobalAssetsCounter_CataloguedAssets_SetsGlobalAssetsCounterWording(bool analyseVideos)
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, analyseVideos);

        string outputVideoFirstFrameDirectory = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_assetsDirectory!);

            await _applicationViewModel!.CatalogAssets(_ => { });

            _applicationViewModel!.CalculateGlobalAssetsCounter();

            int globalAssetsCounter = analyseVideos ? 52 : 51;

            CheckAfterChanges(_applicationViewModel!, _assetsDirectory!,
                $"Total number of assets: {globalAssetsCounter}");

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));

            CheckInstance(applicationViewModelInstances, _assetsDirectory!,
                $"Total number of assets: {globalAssetsCounter}");

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {

            if (analyseVideos)
            {
                Directory.Delete(outputVideoFirstFrameDirectory, true);
            }
        }
    }

    [Test]
    public void CalculateGlobalAssetsCounter_AssetsExist_SetsGlobalAssetsCounterWording()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset1 = _asset1!.WithFolder(folder);
        _asset2 = _asset2!.WithFolder(folder);
        _asset3 = _asset3!.WithFolder(folder);
        _asset4 = _asset4!.WithFolder(folder);

        _applicationViewModel!.CalculateGlobalAssetsCounter();

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 0");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(1));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));

        CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 0");

        _testableAssetRepository.AddAsset(_asset1!, []);

        _applicationViewModel!.CalculateGlobalAssetsCounter();

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 1");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(2));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));

        CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 1");

        _testableAssetRepository.AddAsset(_asset2!, []);

        _applicationViewModel!.CalculateGlobalAssetsCounter();

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 2");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(3));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));

        CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 2");

        _testableAssetRepository.AddAsset(_asset3!, []);

        _applicationViewModel!.CalculateGlobalAssetsCounter();

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 3");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(4));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("GlobalAssetsCounterWording"));

        CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 3");

        _testableAssetRepository.DeleteAsset(assetsDirectory, _asset3.FileName);

        _applicationViewModel!.CalculateGlobalAssetsCounter();

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, "Total number of assets: 2");

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("GlobalAssetsCounterWording"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("GlobalAssetsCounterWording"));

        CheckInstance(applicationViewModelInstances, assetsDirectory, "Total number of assets: 2");

        _testableAssetRepository.AddAsset(_asset4!, []);

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

    [Test]
    public void CalculateGlobalAssetsCounter_NoAsset_SetsGlobalAssetsCounterWording()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

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
            Directory.Delete(assetsDirectory, true);
        }
    }

    [Test]
    public void CalculateGlobalAssetsCounter_ConcurrentAccess_SetsGlobalAssetsCounterWordingSafely()
    {
        string assetsDirectory =
            Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        Folder folder = _testableAssetRepository!.AddFolder(assetsDirectory);

        _asset1 = _asset1!.WithFolder(folder);
        _asset2 = _asset2!.WithFolder(folder);
        _asset3 = _asset3!.WithFolder(folder);
        _asset4 = _asset4!.WithFolder(folder);

        _testableAssetRepository.AddAsset(_asset1!, []);
        _testableAssetRepository.AddAsset(_asset2!, []);
        _testableAssetRepository.AddAsset(_asset3!, []);
        _testableAssetRepository.AddAsset(_asset4!, []);

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

    private
        (List<string> notifyPropertyChangedEvents,
        List<ApplicationViewModel> applicationViewModelInstances,
        List<Folder> folderAddedEvents,
        List<Folder> folderRemovedEvents)
        NotifyPropertyChangedEvents()
    {
        List<string> notifyPropertyChangedEvents = [];
        List<ApplicationViewModel> applicationViewModelInstances = [];
        object eventsLock = new();

        _applicationViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            lock (eventsLock)
            {
                notifyPropertyChangedEvents.Add(e.PropertyName!);
                applicationViewModelInstances.Add((ApplicationViewModel)sender!);
            }
        };

        List<Folder> folderAddedEvents = [];

        _applicationViewModel.FolderAdded += delegate (object _, FolderAddedEventArgs e)
        {
            lock (eventsLock)
            {
                folderAddedEvents.Add(e.Folder);
            }
        };

        List<Folder> folderRemovedEvents = [];

        _applicationViewModel.FolderRemoved += delegate (object _, FolderRemovedEventArgs e)
        {
            lock (eventsLock)
            {
                folderRemovedEvents.Add(e.Folder);
            }
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private void CheckBeforeChanges(string expectedRootDirectory)
    {
        Assert.That(_applicationViewModel!.SortAscending, Is.True);
        Assert.That(_applicationViewModel!.IsRefreshingFolders, Is.False);
        Assert.That(_applicationViewModel!.IsCataloging, Is.False);
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.IsThumbnailsVisible, Is.True);
        Assert.That(_applicationViewModel!.IsViewerVisible, Is.False);
        Assert.That(_applicationViewModel!.ViewerPosition, Is.Zero);
        Assert.That(_applicationViewModel!.SelectedAssets, Is.Empty);
        Assert.That(_applicationViewModel!.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_applicationViewModel!.ObservableAssets, Is.Empty);
        Assert.That(_applicationViewModel!.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.AppTitle,
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_applicationViewModel!.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_applicationViewModel!.CurrentAsset, Is.Null);
        Assert.That(_applicationViewModel!.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_applicationViewModel!.CanGoToPreviousAsset, Is.False);
        Assert.That(_applicationViewModel!.CanGoToNextAsset, Is.False);
        Assert.That(_applicationViewModel!.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_applicationViewModel!.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_applicationViewModel!.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckAfterChanges(
        ApplicationViewModel applicationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedGlobalAssetsCounterWording)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.IsCataloging, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.IsThumbnailsVisible, Is.True);
        Assert.That(applicationViewModelInstance.IsViewerVisible, Is.False);
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.Zero);
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        Assert.That(applicationViewModelInstance.ObservableAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording,
            Is.EqualTo(expectedGlobalAssetsCounterWording));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.False);
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        string expectedGlobalAssetsCounterWording)
    {
        int applicationViewModelInstancesCount = applicationViewModelInstances.Count;

        if (applicationViewModelInstancesCount > 1)
        {
            Assert.That(applicationViewModelInstances[applicationViewModelInstancesCount - 2],
                Is.EqualTo(applicationViewModelInstances[0]));
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
