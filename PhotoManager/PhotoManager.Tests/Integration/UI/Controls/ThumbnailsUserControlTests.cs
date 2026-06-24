using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
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

namespace PhotoManager.Tests.Integration.UI.Controls;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class ThumbnailsUserControlTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;

    private event EventHandler? ThumbnailSelected;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AvaloniaTestSetup.EnsureInitialized();
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
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
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
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
            FolderId = Guid.Empty,
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
                Creation = actualDate,
                Modification = ModificationDate.Default
            },
            ThumbnailCreationDateTime = actualDate,
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

        UserConfigurationService userConfigurationService = configurationRootMock.CreateUserConfigurationService();

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqlitePersistenceContext sqlitePersistenceContext =
            PersistenceContextTestHelper.CreateInitializedContext(pathProviderServiceMock.ResolveDatabaseDirectory());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ImageMagickThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator, catalogFolderPipeline,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
        _applicationViewModel = new(_application);
    }

    [Test]
    public async Task GoToFolder_CataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsTrue_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
        Assert.That(folder, Is.Not.Null);

        _asset1 = _asset1!.WithFolder(folder!);
        _asset2 = _asset2!.WithFolder(folder!);
        _asset3 = _asset3!.WithFolder(folder!);
        _asset4 = _asset4!.WithFolder(folder!);

        const string expectedStatusMessage = "The catalog process has ended.";
        const int expectedViewerPosition = 0;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
        Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

        _applicationViewModel.SetIsRefreshingFolders(true);

        await GoToFolder(assetsDirectory);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            true,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(29));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));

        CheckInstance(
            applicationViewModelInstances,
            assetsDirectory,
            true,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public async Task
        GoToFolder_CataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsFalse_GoesToFolderAndResetsViewerPosition()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
        Assert.That(folder, Is.Not.Null);

        _asset1 = _asset1!.WithFolder(folder!);
        _asset2 = _asset2!.WithFolder(folder!);
        _asset3 = _asset3!.WithFolder(folder!);
        _asset4 = _asset4!.WithFolder(folder!);

        const string expectedStatusMessage = "The catalog process has ended.";
        const int expectedViewerPosition = 0;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
        Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

        await GoToFolder(assetsDirectory);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            false,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(33));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("CurrentAsset"));

        CheckInstance(
            applicationViewModelInstances,
            assetsDirectory,
            false,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public async Task
        GoToFolder_CataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsTrueAndChangePosition_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
        Assert.That(folder, Is.Not.Null);

        _asset1 = _asset1!.WithFolder(folder!);
        _asset2 = _asset2!.WithFolder(folder!);
        _asset3 = _asset3!.WithFolder(folder!);
        _asset4 = _asset4!.WithFolder(folder!);

        const string expectedStatusMessage = "The catalog process has ended.";
        const int expectedViewerPosition = 2;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 3 of 4 - sorted by file name ascending";
        Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

        _applicationViewModel!.SetViewerPosition(expectedViewerPosition);

        _applicationViewModel.SetIsRefreshingFolders(true);

        await GoToFolder(assetsDirectory);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            true,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            true,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(34));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SetViewerPosition
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            assetsDirectory,
            true,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            true,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public async Task
        GoToFolder_CataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsFalseAndChangePosition_GoesToFolderAndResetsViewerPosition()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
        Assert.That(folder, Is.Not.Null);

        _asset1 = _asset1!.WithFolder(folder!);
        _asset2 = _asset2!.WithFolder(folder!);
        _asset3 = _asset3!.WithFolder(folder!);
        _asset4 = _asset4!.WithFolder(folder!);

        const string expectedStatusMessage = "The catalog process has ended.";
        const int expectedViewerPosition = 0;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
        Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

        _applicationViewModel!.SetViewerPosition(2);

        await GoToFolder(assetsDirectory);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            false,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(43));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SetViewerPosition
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("AppTitle"));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("CurrentAsset"));
        // ViewerPosition
        Assert.That(notifyPropertyChangedEvents[38], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[39], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[40], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[41], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[42], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            assetsDirectory,
            false,
            expectedViewerPosition,
            [],
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public async Task GoToFolder_CataloguedAssetsAndOtherNotEmptyDirectoryAndIsRefreshingFoldersIsTrue_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string emptyDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2,
            Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(emptyDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? emptyFolder = _testableAssetRepository!.GetFolderByPath(emptyDirectory);
            Assert.That(emptyFolder, Is.Not.Null);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {emptyDirectory} - image 0 of 0 - sorted by file name ascending";

            // Mock like we already were in an empty directory
            await GoToFolder(emptyDirectory);

            _applicationViewModel.SetIsRefreshingFolders(true);

            await GoToFolder(assetsDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                emptyDirectory,
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(38));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            // GoToFolder 1 (mock)
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                emptyDirectory,
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(emptyFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(emptyDirectory, true);
        }
    }

    [Test]
    public async Task
        GoToFolder_CataloguedAssetsAndOtherNotEmptyDirectoryAndNotRefreshing_GoesToFolderAndResetsViewerPosition()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string emptyDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2,
            Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(emptyDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            Folder? emptyFolder = _testableAssetRepository!.GetFolderByPath(emptyDirectory);
            Assert.That(emptyFolder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            // Mock like we already were in an empty directory
            await GoToFolder(emptyDirectory);

            await GoToFolder(assetsDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(45));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("StatusMessage"));
            // GoToFolder 1 (mock)
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[36], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[37], Is.EqualTo("AppTitle"));
            // GoToFolder 2
            Assert.That(notifyPropertyChangedEvents[38], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[39], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[40], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[41], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[42], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[43], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[44], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                folder,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Has.Count.EqualTo(1));
            Assert.That(folderAddedEvents[0], Is.EqualTo(emptyFolder));

            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(emptyDirectory, true);
        }
    }

    [Test]
    public async Task GoToFolder_CataloguedAssetsAndOtherEmptyDirectoryAndIsRefreshingFoldersIsTrue_DoesNothing()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string emptyDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(emptyDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
            Assert.That(folder, Is.Not.Null);

            _asset1 = _asset1!.WithFolder(folder!);
            _asset2 = _asset2!.WithFolder(folder!);
            _asset3 = _asset3!.WithFolder(folder!);
            _asset4 = _asset4!.WithFolder(folder!);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
            Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

            _applicationViewModel.SetIsRefreshingFolders(true);

            await GoToFolder(emptyDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                folder,
                false,
                true);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(29));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedAssets[expectedViewerPosition],
                folder,
                false,
                true);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(emptyDirectory, true);
        }
    }

    [Test]
    public async Task GoToFolder_CataloguedAssetsAndOtherEmptyDirectoryAndIsRefreshingFoldersIsFalse_GoesToFolder()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string emptyDirectory = Path.Combine(_assetsDirectory!, Directories.TEMP_EMPTY_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Directory.CreateDirectory(emptyDirectory);

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {emptyDirectory} - image 0 of 0 - sorted by file name ascending";

            await GoToFolder(emptyDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                emptyDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(36));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("CurrentFolderPath"));
            Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("AppTitle"));
            Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[32], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[33], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[34], Is.EqualTo("CurrentAsset"));
            Assert.That(notifyPropertyChangedEvents[35], Is.EqualTo("AppTitle"));

            CheckInstance(
                applicationViewModelInstances,
                emptyDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(emptyDirectory, true);
        }
    }

    [Test]
    public async Task GoToFolder_NoCataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsTrue_DoesNothing()
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

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            _applicationViewModel.SetIsRefreshingFolders(true);

            await GoToFolder(assetsDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

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
                true,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

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
    public async Task GoToFolder_NoCataloguedAssetsAndRootDirectoryAndIsRefreshingFoldersIsFalse_GoesToFolder()
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

            await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

            const string expectedStatusMessage = "The catalog process has ended.";
            const int expectedViewerPosition = 0;
            string expectedAppTitle =
                $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 0 of 0 - sorted by file name ascending";

            await GoToFolder(assetsDirectory);

            CheckAfterChanges(
                _applicationViewModel!,
                assetsDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

            Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(9));
            // CatalogAssets + NotifyCatalogChange
            Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("StatusMessage"));
            Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("StatusMessage"));
            // SetAssets
            Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
            Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
            Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
            Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));

            CheckInstance(
                applicationViewModelInstances,
                assetsDirectory,
                false,
                expectedViewerPosition,
                [],
                expectedAppTitle,
                expectedStatusMessage,
                [],
                null,
                null!,
                false,
                false);

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
    public void ContentControlMouseDoubleClick_Event_SendsEvent()
    {
        List<string> thumbnailSelectedEvents = NotifyThumbnailSelected();

        ThumbnailSelected?.Invoke(this, EventArgs.Empty);

        Assert.That(thumbnailSelectedEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ThumbnailsListViewSelectionChanged_CataloguedAssets_SetsSelectedAssets()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? folder = _testableAssetRepository!.GetFolderByPath(assetsDirectory);
        Assert.That(folder, Is.Not.Null);

        _asset1 = _asset1!.WithFolder(folder!);
        _asset2 = _asset2!.WithFolder(folder!);
        _asset3 = _asset3!.WithFolder(folder!);
        _asset4 = _asset4!.WithFolder(folder!);

        const string expectedStatusMessage = "The catalog process has ended.";
        const int expectedViewerPosition = 0;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {assetsDirectory} - image 1 of 4 - sorted by file name ascending";
        Asset[] expectedAssets = [_asset1, _asset2, _asset3, _asset4];

        List<Asset> observableAssets = [.. _applicationViewModel!.ObservableAssets];

        Asset[] expectedSelectedAssets = [observableAssets[0]];

        // First set SelectedAssets
        _applicationViewModel!.SetSelectedAssets(expectedSelectedAssets);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            false,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(30));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SelectedAssets 1
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SelectedAssets"));

        // Second set SelectedAssets
        expectedSelectedAssets = [];

        _applicationViewModel!.SetSelectedAssets(expectedSelectedAssets);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            false,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(31));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SelectedAssets 1
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SelectedAssets"));
        // SelectedAssets 2
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("SelectedAssets"));

        // Third set SelectedAssets
        expectedSelectedAssets =
            [observableAssets[3], observableAssets[0], observableAssets[1], observableAssets[2]];

        _applicationViewModel!.SetSelectedAssets(expectedSelectedAssets);

        CheckAfterChanges(
            _applicationViewModel!,
            assetsDirectory,
            false,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(32));
        // CatalogAssets + NotifyCatalogChange
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[24], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[25], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[26], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[27], Is.EqualTo("StatusMessage"));
        Assert.That(notifyPropertyChangedEvents[28], Is.EqualTo("StatusMessage"));
        // SelectedAssets 1
        Assert.That(notifyPropertyChangedEvents[29], Is.EqualTo("SelectedAssets"));
        // SelectedAssets 2
        Assert.That(notifyPropertyChangedEvents[30], Is.EqualTo("SelectedAssets"));
        // SelectedAssets 3
        Assert.That(notifyPropertyChangedEvents[31], Is.EqualTo("SelectedAssets"));

        CheckInstance(
            applicationViewModelInstances,
            assetsDirectory,
            false,
            expectedViewerPosition,
            expectedSelectedAssets,
            expectedAppTitle,
            expectedStatusMessage,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            folder,
            false,
            true);

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

        _applicationViewModel!.PropertyChanged += delegate (object? sender, PropertyChangedEventArgs e)
        {
            notifyPropertyChangedEvents.Add(e.PropertyName!);
            applicationViewModelInstances.Add((ApplicationViewModel)sender!);
        };

        List<Folder> folderAddedEvents = [];

        _applicationViewModel.FolderAdded += delegate (object _, FolderAddedEventArgs e)
        {
            folderAddedEvents.Add(e.Folder);
        };

        List<Folder> folderRemovedEvents = [];

        _applicationViewModel.FolderRemoved += delegate (object _, FolderRemovedEventArgs e)
        {
            folderRemovedEvents.Add(e.Folder);
        };

        return (notifyPropertyChangedEvents, applicationViewModelInstances, folderAddedEvents, folderRemovedEvents);
    }

    private List<string> NotifyThumbnailSelected()
    {
        List<string> thumbnailSelectedEvents = [];

        ThumbnailSelected += delegate
        {
            thumbnailSelectedEvents.Add(string.Empty);
        };

        return thumbnailSelectedEvents;
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
        bool expectedIsRefreshingFolders,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.EqualTo(expectedIsRefreshingFolders));
        Assert.That(applicationViewModelInstance.IsCataloging, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.IsThumbnailsVisible, Is.True);
        Assert.That(applicationViewModelInstance.IsViewerVisible, Is.False);
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(expectedViewerPosition));
        AssertSelectedAssets(expectedSelectedAssets, applicationViewModelInstance.SelectedAssets);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets,
            applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));

        if (expectedCurrentAsset != null)
        {
            AssertCurrentAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset,
                expectedCurrentAsset.FullPath, expectedLastDirectoryInspected, expectedFolder);
        }
        else
        {
            Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        }

        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.EqualTo(expectedCanGoToPreviousAsset));
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.EqualTo(expectedCanGoToNextAsset));
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void AssertCurrentAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
    {
        CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset,
                currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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

    private static void AssertSelectedAssets(Asset[] expectedAssets, Asset[] selectedAssets)
    {
        Assert.That(selectedAssets, Has.Length.EqualTo(expectedAssets.Length));

        for (int i = 0; i < selectedAssets.Length; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentSelectedAsset = selectedAssets[i];

            CatalogAssetsAsyncAsserts.AssertAssetPropertyValidity(currentSelectedAsset, currentExpectedAsset,
                currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

            Assert.That(currentSelectedAsset.ImageData, Is.Not.Null);
        }
    }

    private static void CheckInstance(
        List<ApplicationViewModel> applicationViewModelInstances,
        string expectedLastDirectoryInspected,
        bool expectedIsRefreshingFolders,
        int expectedViewerPosition,
        Asset[] expectedSelectedAssets,
        string expectedAppTitle,
        string expectedStatusMessage,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
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
                expectedIsRefreshingFolders,
                expectedViewerPosition,
                expectedSelectedAssets,
                expectedAppTitle,
                expectedStatusMessage,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToPreviousAsset,
                expectedCanGoToNextAsset);
        }
    }

    private async Task GoToFolder(string selectedImagePath)
    {
        if (!_applicationViewModel!.IsRefreshingFolders)
        {
            Asset[] assets = await GetAssets(selectedImagePath);

            _applicationViewModel.SetAssets(selectedImagePath, assets);

            if (_applicationViewModel.ObservableAssets.Count > 0)
            {
                _applicationViewModel.SetViewerPosition(0);
            }
        }
    }

    private Task<Asset[]> GetAssets(string directory)
    {
        return Task.Run(() => _application!.GetAssetsByPath(directory));
    }
}
