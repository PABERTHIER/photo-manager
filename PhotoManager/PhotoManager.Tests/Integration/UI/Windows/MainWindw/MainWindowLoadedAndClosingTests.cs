using log4net;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using FileNames = PhotoManager.Tests.Integration.Constants.FileNames;
using FileSize = PhotoManager.Tests.Integration.Constants.FileSize;
using Hashes = PhotoManager.Tests.Integration.Constants.Hashes;
using ModificationDate = PhotoManager.Tests.Integration.Constants.ModificationDate;
using PixelWidthAsset = PhotoManager.Tests.Integration.Constants.PixelWidthAsset;
using PixelHeightAsset = PhotoManager.Tests.Integration.Constants.PixelHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Integration.Constants.ThumbnailWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Integration.Constants.ThumbnailHeightAsset;

namespace PhotoManager.Tests.Integration.UI.Windows.MainWindw;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Window
// The goal is to test what does MainWindow
[TestFixture]
public class MainWindowLoadedAndClosingTests
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FolderNavigationViewModel? _folderNavigationViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private AssetRepository? _assetRepository;
    private UserConfigurationService? _userConfigurationService;

    private Asset _asset1;
    private Asset _asset2;
    private Asset _asset3;
    private Asset _asset4;

    private Folder? _sourceFolder;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task _backgroundWorkTask = new (() => {});
    private Task _catalogTask = new (() => {});
    private Stopwatch? _stopwatch;

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
            Hash = Hashes.IMAGE_1_DUPLICATE_JPG,
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
            Hash = Hashes.IMAGE_9_PNG,
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
            Hash = Hashes.IMAGE_9_DUPLICATE_PNG,
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
            Hash = Hashes.IMAGE_11_HEIC,
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

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _backgroundWorkTask = Task.CompletedTask;
        _catalogTask = Task.CompletedTask;
        _stopwatch = null;
    }

    private void ConfigureApplicationViewModel(
        int catalogBatchSize,
        int catalogCooldownMinutes,
        string assetsDirectory,
        int thumbnailMaxWidth,
        int thumbnailMaxHeight,
        bool syncAssetsEveryXMinutes,
        bool usingDHash,
        bool usingMD5Hash,
        bool usingPHash,
        bool analyseVideos)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES, catalogCooldownMinutes.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH, thumbnailMaxWidth.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT, thumbnailMaxHeight.ToString());
        configurationRootMock.MockGetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES, syncAssetsEveryXMinutes.ToString());
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
        _applicationViewModel = new (application);

        _sourceFolder = new() { Id = Guid.NewGuid(), Path = _applicationViewModel!.CurrentFolderPath };
    }

    [Test]
    public async Task WindowLoaded_CataloguedAssetsAndSyncAssetsEveryXMinutesAndIsCancellationRequestedAndWindowClosing_CatalogsAssetsThreeTimesAndClosesWindowSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, 1, assetsDirectory, 200, 150, true, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            MainWindowsInit();

            await WindowLoaded();

            await _cancellationTokenSource!.CancelAsync();

            Assert.That(_stopwatch, Is.Not.Null);
            Assert.That(_stopwatch.IsRunning, Is.False);
            Assert.That(_stopwatch.Elapsed, Is.GreaterThan(TimeSpan.FromMilliseconds(0)));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);
            _asset4 = _asset4.WithFolder(folder!);

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];
            const string expectedGlobalAssetsCounterWording = "Total number of assets: 4";
            string expectedExecutionTimeWording = $"Execution time: {_stopwatch.Elapsed}";
            const string expectedTotalFilesCountWording = "4 files found";
            const string expectedStatusMessage = "The catalog process has ended.";

            Assert.That(_catalogTask.IsCompleted, Is.True);

            await WindowClosing();

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.True);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(35));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("TotalFilesCountWording"));
            // Second CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("TotalFilesCountWording"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("TotalFilesCountWording"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            // Rider (debug/coverage mode) seems to hold references longer, causing file locks (zip and db files)
            // Forcing GC ensures cleanup before deletion
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task WindowLoaded_CataloguedAssetsAndIsCancellationRequestedAndWindowClosing_ClosesWindowSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, 5, assetsDirectory, 200, 150, false, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            MainWindowsInit();

            await _cancellationTokenSource!.CancelAsync();

            await WindowLoaded();

            Assert.That(_stopwatch, Is.Not.Null);
            Assert.That(_stopwatch.IsRunning, Is.False);
            Assert.That(_stopwatch.Elapsed, Is.GreaterThan(TimeSpan.FromMilliseconds(0)));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);
            _asset4 = _asset4.WithFolder(folder!);

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";
            Asset[] expectedAssets = [];
            const string expectedGlobalAssetsCounterWording = "Total number of assets: 0";
            string expectedExecutionTimeWording = $"Execution time: {_stopwatch.Elapsed}";
            const string expectedTotalFilesCountWording = "4 files found";
            const string expectedStatusMessage = "The catalog process has ended.";

            Assert.That(_catalogTask.IsCompleted, Is.True);

            await WindowClosing();

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.True);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(8));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("TotalFilesCountWording"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            // Rider (debug/coverage mode) seems to hold references longer, causing file locks (zip and db files)
            // Forcing GC ensures cleanup before deletion
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task WindowLoaded_CataloguedAssetsAndWindowClosing_CatalogsAssetsAndClosesWindowSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, 5, assetsDirectory, 200, 150, false, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            MainWindowsInit();

            await WindowLoaded();

            Assert.That(_stopwatch, Is.Not.Null);
            Assert.That(_stopwatch.IsRunning, Is.False);
            Assert.That(_stopwatch.Elapsed, Is.GreaterThan(TimeSpan.FromMilliseconds(0)));

            Folder? folder = _assetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1.WithFolder(folder!);
            _asset2 = _asset2.WithFolder(folder!);
            _asset3 = _asset3.WithFolder(folder!);
            _asset4 = _asset4.WithFolder(folder!);

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];
            const string expectedGlobalAssetsCounterWording = "Total number of assets: 4";
            string expectedExecutionTimeWording = $"Execution time: {_stopwatch.Elapsed}";
            const string expectedTotalFilesCountWording = "4 files found";
            const string expectedStatusMessage = "The catalog process has ended.";

            Assert.That(_catalogTask.IsCompleted, Is.True);

            await WindowClosing();

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.True);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(21));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("TotalFilesCountWording"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                _asset1,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            // Rider (debug/coverage mode) seems to hold references longer, causing file locks (zip and db files)
            // Forcing GC ensures cleanup before deletion
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Directory.Delete(_databaseDirectory!, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task WindowLoaded_NoCataloguedAssetsAndWindowClosing_ClosesWindowSafely()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, 5, assetsDirectory, 200, 150, false, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

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

            await WindowLoaded();

            Assert.That(_stopwatch, Is.Not.Null);
            Assert.That(_stopwatch.IsRunning, Is.False);
            Assert.That(_stopwatch.Elapsed, Is.GreaterThan(TimeSpan.FromMilliseconds(0)));

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";
            const string expectedGlobalAssetsCounterWording = "Total number of assets: 0";
            string expectedExecutionTimeWording = $"Execution time: {_stopwatch.Elapsed}";
            const string expectedTotalFilesCountWording = "0 files found";
            const string expectedStatusMessage = "The catalog process has ended.";

            Assert.That(_catalogTask.IsCompleted, Is.True);

            await WindowClosing();

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.True);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("GlobalAssetsCounterWording"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("ExecutionTimeWording"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("TotalFilesCountWording"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                [],
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            // Rider (debug/coverage mode) seems to hold references longer, causing file locks (zip and db files)
            // Forcing GC ensures cleanup before deletion
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task WindowClosing_WindowNotLoadedAndCatalogTaskIsCompleted_CancelsTaskAndCancellationToken()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, 5, assetsDirectory, 200, 150, false, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

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

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            TaskCompletionSource<bool> taskCompletionSource = new();
            _catalogTask = taskCompletionSource.Task;

            Task windowClosingTask = Task.Run(WindowClosing);

            Assert.That(_catalogTask.IsCompleted, Is.False);

            taskCompletionSource.SetResult(true);
            await windowClosingTask;

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.True);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
        }
    }

    [Test]
    public async Task WindowClosing_WindowNotLoadedAndCatalogTaskIsNotCompleted_CancelsTaskAndCancellationToken()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, 5, assetsDirectory, 200, 150, false, false, false, false, true);
        LoggingAssertsService loggingAssertsService = new();

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

            string expectedAppTitle = $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            TaskCompletionSource<bool> taskCompletionSource = new();
            _catalogTask = taskCompletionSource.Task;

            Task windowClosingTask = Task.Run(WindowClosing);

            Assert.That(_catalogTask.IsCompleted, Is.False);

            await windowClosingTask;

            Assert.That(_cancellationTokenSource!.IsCancellationRequested, Is.True);
            Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
            Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.True);

            Assert.That(_catalogTask.IsCompleted, Is.False);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false);

            CheckFolderNavigationViewModel(
                _folderNavigationViewModel!,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false,
                _sourceFolder!);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                expectedAppTitle,
                [],
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Exception[] expectedExceptions = [];
            Type typeOfService = typeof(MainWindowLoadedAndClosingTests);

            loggingAssertsService.AssertLogExceptions(expectedExceptions, typeOfService);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            Directory.Delete(assetsDirectory, true);
            loggingAssertsService.LoggingAssertTearDown();
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
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
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
        string expectedAppTitle,
        Asset[] expectedAssets,
        string? expectedGlobalAssetsCounterWording,
        string? expectedExecutionTimeWording,
        string? expectedTotalFilesCountWording,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(0));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets, applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(expectedGlobalAssetsCounterWording));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(expectedExecutionTimeWording));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(expectedTotalFilesCountWording));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

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
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckFolderNavigationViewModel(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        string expectedAppTitle,
        Asset[] expectedAssets,
        string? expectedGlobalAssetsCounterWording,
        string? expectedExecutionTimeWording,
        string? expectedTotalFilesCountWording,
        string? expectedStatusMessage,
        Asset? expectedCurrentAsset,
        bool expectedCanGoToNextAsset,
        Folder expectedSourceFolder)
    {
        CheckAfterChanges(
            folderNavigationViewModelInstance.ApplicationViewModel,
            expectedLastDirectoryInspected,
            expectedAppTitle,
            expectedAssets,
            expectedGlobalAssetsCounterWording,
            expectedExecutionTimeWording,
            expectedTotalFilesCountWording,
            expectedStatusMessage,
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
        string expectedAppTitle,
        Asset[] expectedAssets,
        string? expectedGlobalAssetsCounterWording,
        string? expectedExecutionTimeWording,
        string? expectedTotalFilesCountWording,
        string? expectedStatusMessage,
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
                expectedAppTitle,
                expectedAssets,
                expectedGlobalAssetsCounterWording,
                expectedExecutionTimeWording,
                expectedTotalFilesCountWording,
                expectedStatusMessage,
                expectedCurrentAsset,
                expectedCanGoToNextAsset);
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
        _folderNavigationViewModel = new (_applicationViewModel!, _sourceFolder!, []);
        _cancellationTokenSource = new();

        Assert.That(_cancellationTokenSource.IsCancellationRequested, Is.False);
        Assert.That(_cancellationTokenSource.Token.CanBeCanceled, Is.True);
        Assert.That(_cancellationTokenSource.Token.IsCancellationRequested, Is.False);
    }

    private Task WindowLoaded()
    {
        _backgroundWorkTask = StartBackgroundWorkAsync();

        return _backgroundWorkTask;
    }

    private async Task StartBackgroundWorkAsync()
    {
        try
        {
            _stopwatch = Stopwatch.StartNew();
            _applicationViewModel!.StatusMessage = $"Cataloging thumbnails for {_applicationViewModel!.CurrentFolderPath}";

            await InitializeOnceAsync().ConfigureAwait(false); // Unlike in WPF context, we do no need to set it to true

            if (_applicationViewModel!.GetSyncAssetsEveryXMinutes())
            {
                ushort minutes = _applicationViewModel!.GetCatalogCooldownMinutes();
                // TimeSpan delay = TimeSpan.FromMinutes(minutes);
                TimeSpan delay = TimeSpan.FromSeconds(minutes * 10); // To reduce the test duration
                int counter = 0; // Adding a counter just for the test

                while (!_cancellationTokenSource!.Token.IsCancellationRequested && counter < 2)
                {
                    try
                    {
                        await Task.Delay(delay, _cancellationTokenSource!.Token).ConfigureAwait(false); // Unlike in WPF context, we do no need to set it to true
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    await InitializeOnceAsync().ConfigureAwait(false); // Unlike in WPF context, we do no need to set it to true
                    counter++;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, new Exception("Unexpected error in background work"));
        }
    }

    private async Task InitializeOnceAsync()
    {
        _catalogTask = _applicationViewModel!.CatalogAssets(
            e => _applicationViewModel!.NotifyCatalogChange(e), // Unlike in WPF context, there is no thread issue here
            _cancellationTokenSource!.Token
        );

        try
        {
            await _catalogTask.ConfigureAwait(false); // Unlike in WPF context, we do no need to set it to true
        }
        catch (OperationCanceledException ex)
        {
            Log.Error(ex);
        }

        _applicationViewModel!.CalculateGlobalAssetsCounter();
        _stopwatch!.Stop();
        _applicationViewModel!.SetExecutionTime(_stopwatch.Elapsed);
        _applicationViewModel!.CalculateTotalFilesCount();
    }

    private Task WindowClosing()
    {
        _cancellationTokenSource?.Cancel();

        _ = _backgroundWorkTask.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Log.Error(task.Exception, new Exception("BackgroundWorkTask faulted during shutdown"));
            }
        }, TaskScheduler.Default);

        _backgroundWorkTask = Task.CompletedTask;

        return _backgroundWorkTask;
    }
}
