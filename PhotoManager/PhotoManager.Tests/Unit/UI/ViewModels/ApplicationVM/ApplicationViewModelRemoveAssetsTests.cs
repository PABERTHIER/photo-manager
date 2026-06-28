using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;
using PixelHeightAsset = PhotoManager.Tests.Unit.Constants.PixelHeightAsset;
using PixelWidthAsset = PhotoManager.Tests.Unit.Constants.PixelWidthAsset;
using ThumbnailHeightAsset = PhotoManager.Tests.Unit.Constants.ThumbnailHeightAsset;
using ThumbnailWidthAsset = PhotoManager.Tests.Unit.Constants.ThumbnailWidthAsset;

namespace PhotoManager.Tests.Unit.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelRemoveAssetsTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    private Asset? _asset1;
    private Asset? _asset2;
    private Asset? _asset3;
    private Asset? _asset4;
    private Asset? _asset5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        Guid folderId = Guid.NewGuid();

        _asset1 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId, Path = _assetsDirectory })
            .WithFileName(FileNames.IMAGE_1_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_1_JPG, PixelHeightAsset.IMAGE_1_JPG,
                ThumbnailWidthAsset.IMAGE_1_JPG, ThumbnailHeightAsset.IMAGE_1_JPG)
            .WithFileProperties(2020, new(2010, 1, 1, 20, 20, 20, 20, 20), new(2011, 1, 1, 20, 20, 20, 20, 20))
            .WithThumbnailCreationDateTime(new(2010, 1, 1, 20, 20, 20, 20, 20))
            .WithHash(string.Empty)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset2 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId, Path = _assetsDirectory })
            .WithFileName(FileNames.IMAGE_2_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_2_JPG, PixelHeightAsset.IMAGE_2_JPG,
                ThumbnailWidthAsset.IMAGE_2_JPG, ThumbnailHeightAsset.IMAGE_2_JPG)
            .WithFileProperties(2048, new(2020, 6, 1), new(2020, 7, 1))
            .WithThumbnailCreationDateTime(new(2020, 6, 1))
            .WithHash(string.Empty)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset3 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId, Path = _assetsDirectory })
            .WithFileName(FileNames.IMAGE_3_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_3_JPG, PixelHeightAsset.IMAGE_3_JPG,
                ThumbnailWidthAsset.IMAGE_3_JPG, ThumbnailHeightAsset.IMAGE_3_JPG)
            .WithFileProperties(2000, new(2010, 1, 1), new(2011, 1, 1))
            .WithThumbnailCreationDateTime(new(2010, 1, 1))
            .WithHash(string.Empty)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset4 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId, Path = _assetsDirectory })
            .WithFileName(FileNames.IMAGE_4_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_4_JPG, PixelHeightAsset.IMAGE_4_JPG,
                ThumbnailWidthAsset.IMAGE_4_JPG, ThumbnailHeightAsset.IMAGE_4_JPG)
            .WithFileProperties(2030, new(2010, 8, 1), new(2011, 9, 1))
            .WithThumbnailCreationDateTime(new(2010, 8, 1))
            .WithHash(string.Empty)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
        _asset5 = AssetBuilder.Create()
            .WithFolder(new() { Id = folderId, Path = _assetsDirectory })
            .WithFileName(FileNames.IMAGE_5_JPG)
            .WithRotation(ImageRotation.Rotate0)
            .WithPixels(PixelWidthAsset.IMAGE_5_JPG, PixelHeightAsset.IMAGE_5_JPG,
                ThumbnailWidthAsset.IMAGE_5_JPG, ThumbnailHeightAsset.IMAGE_5_JPG)
            .WithFileProperties(2048, new(2020, 6, 1), new(2020, 7, 1))
            .WithThumbnailCreationDateTime(new(2020, 6, 1))
            .WithHash(string.Empty)
            .WithImageData(SkiaImageData.Empty())
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
    }

    [SetUp]
    public void SetUp()
    {
        _asset1!.ImageData = SkiaImageData.Empty();
        _asset2!.ImageData = SkiaImageData.Empty();
        _asset3!.ImageData = SkiaImageData.Empty();
        _asset4!.ImageData = SkiaImageData.Empty();
        _asset5!.ImageData = SkiaImageData.Empty();
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
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
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
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);
        _applicationViewModel = new(application);
    }

    [Test]
    public void
        RemoveAssets_ObservableAssetsIsNotEmptyAndAssetsIsNotEmptyAndRemoveTheCurrentAssetInFirstPosition_RemovesAsset()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const int expectedViewerPosition = 0;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 1 of 2 - sorted by file name ascending";

        Asset[] assets = [_asset2!, _asset4!, _asset5!];
        Asset[] expectedAssets = [_asset4!, _asset5!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        _applicationViewModel!.RemoveAssets([_asset2!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            false,
            true);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(10));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            false,
            true);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        RemoveAssets_ObservableAssetsIsNotEmptyAndAssetsIsNotEmptyAndRemoveTheCurrentAssetInMiddlePosition_RemovesAsset()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const int expectedViewerPosition = 1;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 2 of 2 - sorted by file name ascending";

        Asset[] assets = [_asset2!, _asset4!, _asset5!];
        Asset[] expectedAssets = [_asset2!, _asset5!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.GoToNextAsset();

        _applicationViewModel!.RemoveAssets([_asset4!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(15));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // GoToNextAsset 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            true,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void
        RemoveAssets_ObservableAssetsIsNotEmptyAndAssetsIsNotEmptyAndRemoveTheCurrentAssetInLastPosition_RemovesAsset()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        const int expectedViewerPosition = 1;
        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 2 of 2 - sorted by file name ascending";

        Asset[] assets = [_asset2!, _asset4!, _asset5!];
        Asset[] expectedAssets = [_asset2!, _asset4!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);
        _applicationViewModel!.GoToNextAsset();
        _applicationViewModel!.GoToNextAsset();

        _applicationViewModel!.RemoveAssets([_asset5!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            true,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(24));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // GoToNextAsset 1
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));
        // GoToNextAsset 2
        Assert.That(notifyPropertyChangedEvents[10], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[11], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[12], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[13], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[14], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[15], Is.EqualTo("ViewerPosition"));
        Assert.That(notifyPropertyChangedEvents[16], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[17], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[18], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[19], Is.EqualTo("AppTitle"));
        Assert.That(notifyPropertyChangedEvents[20], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[21], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[22], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[23], Is.EqualTo("CurrentAsset"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            expectedViewerPosition,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[expectedViewerPosition],
            expectedAssets[expectedViewerPosition].Folder,
            true,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsHasAssetsAndAssetsHasTheSameAssets_RemovesAllAssets()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 0 of 0 - sorted by file name ascending";

        Asset[] assets = [_asset1!, _asset2!, _asset3!, _asset4!, _asset5!];
        Asset[] expectedAssets = [];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        _applicationViewModel!.RemoveAssets(assets);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            null,
            null!,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(10));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            null,
            null!,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsHasOneAssetAndAssetsHasTheSameAsset_RemovesAsset()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 0 of 0 - sorted by file name ascending";

        Asset[] assets = [_asset1!];
        Asset[] expectedAssets = [];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        _applicationViewModel!.RemoveAssets([assets[0]]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            null,
            null!,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(10));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            null,
            null!,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsHasOneAssetAndAssetsHasNotTheSameAsset_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 1 of 1 - sorted by file name ascending";

        Asset[] assets = [_asset1!];
        Asset[] expectedAssets = [_asset1!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        _applicationViewModel!.RemoveAssets([_asset2!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsHasTwoAssetsAndAssetsHasOneSameAndOneDifferentAsset_RemovesAsset()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 1 of 1 - sorted by file name ascending";

        Asset[] assets = [_asset1!, _asset2!];
        Asset[] expectedAssets = [_asset2!];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assets);

        _applicationViewModel!.RemoveAssets([_asset4!, _asset1!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(10));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));
        // RemoveAssets
        Assert.That(notifyPropertyChangedEvents[5], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[6], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[7], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[8], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[9], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            expectedAssets,
            expectedAssets[0],
            expectedAssets[0].Folder,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsIsEmptyAndAssetsIsNotEmpty_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 0 of 0 - sorted by file name ascending";

        _applicationViewModel!.RemoveAssets([_asset1!, _asset3!]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsIsEmptyAndAssetsIsEmpty_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 0 of 0 - sorted by file name ascending";

        _applicationViewModel!.RemoveAssets([]);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsIsEmptyAndAssetsIsNull_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 0 of 0 - sorted by file name ascending";

        _applicationViewModel!.RemoveAssets(null!);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            [],
            null,
            null!,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsIsNotEmptyAndAssetsIsEmpty_DoesNothing()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 1 of 1 - sorted by file name ascending";

        Asset[] assetsToSet = [_asset1!];
        Asset[] assetsToRemove = [];

        _applicationViewModel!.SetAssets(_assetsDirectory!, assetsToSet);

        _applicationViewModel!.RemoveAssets(assetsToRemove);

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            assetsToSet,
            assetsToSet[0],
            assetsToSet[0].Folder,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            assetsToSet,
            assetsToSet[0],
            assetsToSet[0].Folder,
            false,
            false);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void RemoveAssets_ObservableAssetsIsNotEmptyAssetsIsNull_ThrowsNullReferenceException()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string expectedAppTitle =
            $"PhotoManager {Constants.VERSION} - {_assetsDirectory!} - image 1 of 1 - sorted by file name ascending";

        Asset[] assetsToSet = [_asset1!];
        Asset[] assetsToRemove = null!;

        _applicationViewModel!.SetAssets(_assetsDirectory!, assetsToSet);

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _applicationViewModel!.RemoveAssets(assetsToRemove));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        CheckAfterChanges(
            _applicationViewModel!,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            assetsToSet,
            assetsToSet[0],
            assetsToSet[0].Folder,
            false,
            false);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(5));
        // SetAssets
        Assert.That(notifyPropertyChangedEvents[0], Is.EqualTo("ObservableAssets"));
        Assert.That(notifyPropertyChangedEvents[1], Is.EqualTo("CanGoToPreviousAsset"));
        Assert.That(notifyPropertyChangedEvents[2], Is.EqualTo("CanGoToNextAsset"));
        Assert.That(notifyPropertyChangedEvents[3], Is.EqualTo("CurrentAsset"));
        Assert.That(notifyPropertyChangedEvents[4], Is.EqualTo("AppTitle"));

        CheckInstance(
            applicationViewModelInstances,
            _assetsDirectory!,
            0,
            expectedAppTitle,
            assetsToSet,
            assetsToSet[0],
            assetsToSet[0].Folder,
            false,
            false);

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
        int expectedViewerPosition,
        string expectedAppTitle,
        Asset[] expectedAssets,
        Asset? expectedCurrentAsset,
        Folder expectedFolder,
        bool expectedCanGoToPreviousAsset,
        bool expectedCanGoToNextAsset)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.IsCataloging, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.IsThumbnailsVisible, Is.True);
        Assert.That(applicationViewModelInstance.IsViewerVisible, Is.False);
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.EqualTo(expectedViewerPosition));
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        AssertObservableAssets(expectedLastDirectoryInspected, expectedAssets,
            applicationViewModelInstance.ObservableAssets);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo(expectedAppTitle));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(string.Empty));

        if (expectedCurrentAsset != null)
        {
            AssertAssetPropertyValidity(applicationViewModelInstance.CurrentAsset!, expectedCurrentAsset,
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

    private static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
    {
        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime, Is.EqualTo(expectedAsset.ThumbnailCreationDateTime));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.ImageData, Is.Not.Null); // Unlike below (Application, CatalogAssetsService), it is set here
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(assetPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(folderPath));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(expectedAsset.FileProperties.Creation.Date));
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    private static void AssertObservableAssets(string currentDirectory, Asset[] expectedAssets,
        ObservableCollection<Asset> observableAssets)
    {
        Assert.That(observableAssets, Has.Count.EqualTo(expectedAssets.Length));

        for (int i = 0; i < observableAssets.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            Asset currentObservableAsset = observableAssets[i];

            AssertAssetPropertyValidity(currentObservableAsset, currentExpectedAsset, currentExpectedAsset.FullPath,
                currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);

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
        int expectedViewerPosition,
        string expectedAppTitle,
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
                expectedViewerPosition,
                expectedAppTitle,
                expectedAssets,
                expectedCurrentAsset,
                expectedFolder,
                expectedCanGoToPreviousAsset,
                expectedCanGoToNextAsset);
        }
    }
}
