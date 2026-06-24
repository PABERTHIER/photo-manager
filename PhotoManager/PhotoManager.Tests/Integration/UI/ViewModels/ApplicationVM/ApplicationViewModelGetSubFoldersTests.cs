using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelGetSubFoldersTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private ApplicationViewModel? _applicationViewModel;
    private TestableAssetRepository? _testableAssetRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
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
        PhotoManager.Application.Application application = new(_testableAssetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService, assetConversionService);
        _applicationViewModel = new(application);
    }

    [Test]
    public async Task GetSubFolders_CataloguedAssetsAndParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        string assetsDirectory = Path.Combine(_assetsDirectory!, Directories.DUPLICATES);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(assetsDirectory);

        string parentFolderPath1 = Path.Combine(assetsDirectory, Directories.NEW_FOLDER_1);
        string parentFolderPath2 = Path.Combine(assetsDirectory, Directories.NEW_FOLDER_2);
        string parentFolderPath3 = Path.Combine(assetsDirectory, Directories.NOT_DUPLICATE);
        string parentFolderPath4 = Path.Combine(assetsDirectory, Directories.PART);
        string parentFolderPath5 = Path.Combine(assetsDirectory, Directories.RESOLUTION);
        string parentFolderPath6 = Path.Combine(assetsDirectory, Directories.THUMBNAIL);

        string childFolderPath1 = Path.Combine(parentFolderPath3, Directories.SAMPLE_1);
        string childFolderPath2 = Path.Combine(parentFolderPath3, Directories.SAMPLE_2);
        string childFolderPath3 = Path.Combine(parentFolderPath3, Directories.SAMPLE_3);

        await _applicationViewModel!.CatalogAssets(_applicationViewModel.NotifyCatalogChange);

        Folder? parentFolder1 = _testableAssetRepository!.GetFolderByPath(parentFolderPath1);
        Folder? parentFolder2 = _testableAssetRepository!.GetFolderByPath(parentFolderPath2);
        Folder? parentFolder3 = _testableAssetRepository!.GetFolderByPath(parentFolderPath3);
        Folder? parentFolder4 = _testableAssetRepository!.GetFolderByPath(parentFolderPath4);
        Folder? parentFolder5 = _testableAssetRepository!.GetFolderByPath(parentFolderPath5);
        Folder? parentFolder6 = _testableAssetRepository!.GetFolderByPath(parentFolderPath6);

        Assert.That(parentFolder1, Is.Not.Null);
        Assert.That(parentFolder2, Is.Not.Null);
        Assert.That(parentFolder3, Is.Not.Null);
        Assert.That(parentFolder4, Is.Not.Null);
        Assert.That(parentFolder5, Is.Not.Null);
        Assert.That(parentFolder6, Is.Not.Null);

        Folder[] parentFolders1 = _applicationViewModel!.GetSubFolders(parentFolder1);
        Folder[] parentFolders2 = _applicationViewModel!.GetSubFolders(parentFolder2);
        Folder[] parentFolders3 = _applicationViewModel!.GetSubFolders(parentFolder3);
        Folder[] parentFolders4 = _applicationViewModel!.GetSubFolders(parentFolder4);
        Folder[] parentFolders5 = _applicationViewModel!.GetSubFolders(parentFolder5);
        Folder[] parentFolders6 = _applicationViewModel!.GetSubFolders(parentFolder6);

        Assert.That(parentFolders1, Is.Empty);
        Assert.That(parentFolders2, Is.Empty);

        Assert.That(parentFolders3, Is.Not.Empty);
        Assert.That(parentFolders3, Has.Length.EqualTo(3));
        Assert.That(parentFolders3.Any(x => x.Path == childFolderPath1));
        Assert.That(parentFolders3.Any(x => x.Path == childFolderPath2));
        Assert.That(parentFolders3.Any(x => x.Path == childFolderPath3));

        Assert.That(parentFolders4, Is.Empty);
        Assert.That(parentFolders5, Is.Empty);
        Assert.That(parentFolders6, Is.Empty);

        Folder? childFolder1 = _testableAssetRepository!.GetFolderByPath(childFolderPath1);
        Folder? childFolder2 = _testableAssetRepository!.GetFolderByPath(childFolderPath2);
        Folder? childFolder3 = _testableAssetRepository!.GetFolderByPath(childFolderPath3);

        Assert.That(childFolder1, Is.Not.Null);
        Assert.That(childFolder2, Is.Not.Null);
        Assert.That(childFolder3, Is.Not.Null);

        Folder[] childFolders1 = _applicationViewModel!.GetSubFolders(childFolder1);
        Folder[] childFolders2 = _applicationViewModel!.GetSubFolders(childFolder2);
        Folder[] childFolders3 = _applicationViewModel!.GetSubFolders(childFolder3);

        Assert.That(childFolders1, Is.Empty);
        Assert.That(childFolders2, Is.Empty);
        Assert.That(childFolders3, Is.Empty);

        const string expectedStatusMessage = "The catalog process has ended.";

        CheckAfterChanges(_applicationViewModel!, assetsDirectory, expectedStatusMessage);

        Assert.That(notifyPropertyChangedEvents, Has.Count.EqualTo(54));

        for (int i = 0; i < 54; i++)
        {
            Assert.That(notifyPropertyChangedEvents[i], Is.EqualTo("StatusMessage"));
        }

        CheckInstance(applicationViewModelInstances, assetsDirectory, expectedStatusMessage);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Has.Count.EqualTo(9));
        Assert.That(folderAddedEvents[0], Is.EqualTo(parentFolder1));
        Assert.That(folderAddedEvents[1], Is.EqualTo(parentFolder2));
        Assert.That(folderAddedEvents[2], Is.EqualTo(parentFolder3));
        Assert.That(folderAddedEvents[3], Is.EqualTo(childFolder1));
        Assert.That(folderAddedEvents[4], Is.EqualTo(childFolder2));
        Assert.That(folderAddedEvents[5], Is.EqualTo(childFolder3));
        Assert.That(folderAddedEvents[6], Is.EqualTo(parentFolder4));
        Assert.That(folderAddedEvents[7], Is.EqualTo(parentFolder5));
        Assert.That(folderAddedEvents[8], Is.EqualTo(parentFolder6));

        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void GetSubFolders_ParentHasSubFolders_ReturnsMatchingSubFolders()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
        string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

        string childFolderPath1 = Path.Combine(parentFolderPath1, Directories.TEST_SUB_FOLDER_1);
        string childFolderPath2 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_2);
        string childFolderPath3 = Path.Combine(parentFolderPath2, Directories.TEST_SUB_FOLDER_3);

        Folder parentFolder1 = _testableAssetRepository!.AddFolder(parentFolderPath1);
        Folder parentFolder2 = _testableAssetRepository!.AddFolder(parentFolderPath2);

        Folder childFolder1 = _testableAssetRepository!.AddFolder(childFolderPath1);
        Folder childFolder2 = _testableAssetRepository!.AddFolder(childFolderPath2);
        Folder childFolder3 = _testableAssetRepository!.AddFolder(childFolderPath3);

        Folder[] parentFolders1 = _applicationViewModel!.GetSubFolders(parentFolder1);
        Folder[] parentFolders2 = _applicationViewModel!.GetSubFolders(parentFolder2);

        Folder[] childFolders1 = _applicationViewModel!.GetSubFolders(childFolder1);
        Folder[] childFolders2 = _applicationViewModel!.GetSubFolders(childFolder2);
        Folder[] childFolders3 = _applicationViewModel!.GetSubFolders(childFolder3);

        Assert.That(parentFolders1, Is.Not.Empty);
        Assert.That(parentFolders1, Has.Length.EqualTo(1));
        Assert.That(parentFolders1[0].Path, Is.EqualTo(childFolderPath1));

        Assert.That(parentFolders2, Is.Not.Empty);
        Assert.That(parentFolders2, Has.Length.EqualTo(2));
        Assert.That(parentFolders2.Any(x => x.Path == childFolderPath2));
        Assert.That(parentFolders2.Any(x => x.Path == childFolderPath3));

        Assert.That(childFolders1, Is.Empty);
        Assert.That(childFolders2, Is.Empty);
        Assert.That(childFolders3, Is.Empty);

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, string.Empty);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, string.Empty);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void GetSubFolders_ParentHasNoSubFolders_ReturnsEmptyArray()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
        string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

        Folder parentFolder1 = _testableAssetRepository!.AddFolder(parentFolderPath1);
        Folder parentFolder2 = _testableAssetRepository!.AddFolder(parentFolderPath2);

        Folder[] parentFolders1 = _applicationViewModel!.GetSubFolders(parentFolder1);
        Folder[] parentFolders2 = _applicationViewModel!.GetSubFolders(parentFolder2);

        Assert.That(parentFolders1, Is.Empty);
        Assert.That(parentFolders2, Is.Empty);

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, string.Empty);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, string.Empty);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void GetSubFolders_NoFoldersRegistered_ReturnsEmptyArray()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        string parentFolderPath1 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_1);
        string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

        Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderPath1 };
        Folder parentFolder2 = new() { Id = Guid.NewGuid(), Path = parentFolderPath2 };

        Folder[] parentFolders1 = _applicationViewModel!.GetSubFolders(parentFolder1);
        Folder[] parentFolders2 = _applicationViewModel!.GetSubFolders(parentFolder2);

        Assert.That(parentFolders1, Is.Empty);
        Assert.That(parentFolders2, Is.Empty);

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, string.Empty);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, string.Empty);

        // Because the root folder is already added
        Assert.That(folderAddedEvents, Is.Empty);
        Assert.That(folderRemovedEvents, Is.Empty);
    }

    [Test]
    public void GetSubFolders_ParentFolderIsNull_ThrowsNullReferenceException()
    {
        ConfigureApplicationViewModel(100, _assetsDirectory!, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        CheckBeforeChanges(_assetsDirectory!);

        Folder? parentFolder1 = null;

        string parentFolderPath2 = Path.Combine(_assetsDirectory!, Directories.TEST_FOLDER_2);

        // At least one folder to trigger the Where on folders
        _testableAssetRepository!.AddFolder(parentFolderPath2);

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _applicationViewModel!.GetSubFolders(parentFolder1!));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        CheckAfterChanges(_applicationViewModel!, _assetsDirectory!, string.Empty);

        Assert.That(notifyPropertyChangedEvents, Is.Empty);

        CheckInstance(applicationViewModelInstances, _assetsDirectory!, string.Empty);

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
        string? expectedStatusMessage)
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
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo(
                $"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(expectedStatusMessage));
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
        string? expectedStatusMessage)
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
                expectedStatusMessage);
        }
    }
}
