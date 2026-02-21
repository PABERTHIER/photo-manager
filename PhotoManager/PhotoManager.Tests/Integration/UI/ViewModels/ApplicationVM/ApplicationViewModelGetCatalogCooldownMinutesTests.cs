using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.ViewModels.ApplicationVM;

[TestFixture]
public class ApplicationViewModelGetCatalogCooldownMinutesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private ApplicationViewModel? _applicationViewModel;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplicationViewModel(string assetsDirectory, ushort catalogCooldownMinutes)
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES, catalogCooldownMinutes.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IPathProviderService> pathProviderServiceMock = new();
        pathProviderServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        ImageProcessingService imageProcessingService = new();
        FileOperationsService fileOperationsService = new(userConfigurationService);
        ImageMetadataService imageMetadataService = new(fileOperationsService);
        AssetRepository assetRepository = new(database, pathProviderServiceMock.Object, imageProcessingService,
            imageMetadataService, userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(assetRepository, fileOperationsService, imageProcessingService,
            imageMetadataService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(assetRepository, fileOperationsService, imageMetadataService,
            assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(assetRepository, fileOperationsService, assetCreationService);
        SyncAssetsService syncAssetsService =
            new(assetRepository, fileOperationsService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService =
            new(assetRepository, fileOperationsService, userConfigurationService);
        PhotoManager.Application.Application application = new(assetRepository, syncAssetsService,
            catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService,
            fileOperationsService, imageProcessingService);
        _applicationViewModel = new(application);
    }

    [Test]
    [TestCase((ushort)0)]
    [TestCase((ushort)2)]
    [TestCase((ushort)15)]
    public void GetCatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue(ushort expectedCatalogCooldownMinutes)
    {
        ConfigureApplicationViewModel(_dataDirectory!, expectedCatalogCooldownMinutes);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        try
        {
            CheckBeforeChanges(_dataDirectory!);

            ushort catalogCooldownMinutes = _applicationViewModel!.GetCatalogCooldownMinutes();

            Assert.That(catalogCooldownMinutes, Is.EqualTo(expectedCatalogCooldownMinutes));

            CheckAfterChanges(_applicationViewModel!, _dataDirectory!);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            CheckInstance(applicationViewModelInstances, _dataDirectory!);

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
        Assert.That(_applicationViewModel!.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_applicationViewModel!.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_applicationViewModel!.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_applicationViewModel!.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_applicationViewModel!.ViewerPosition, Is.Zero);
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

    private static void CheckAfterChanges(ApplicationViewModel applicationViewModelInstance, string expectedLastDirectoryInspected)
    {
        Assert.That(applicationViewModelInstance.SortAscending, Is.True);
        Assert.That(applicationViewModelInstance.IsRefreshingFolders, Is.False);
        Assert.That(applicationViewModelInstance.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(applicationViewModelInstance.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(applicationViewModelInstance.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(applicationViewModelInstance.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(applicationViewModelInstance.ViewerPosition, Is.Zero);
        Assert.That(applicationViewModelInstance.SelectedAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        Assert.That(applicationViewModelInstance.ObservableAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.False);
        Assert.That(applicationViewModelInstance.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(applicationViewModelInstance.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(applicationViewModelInstance.AboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    private static void CheckInstance(List<ApplicationViewModel> applicationViewModelInstances, string expectedLastDirectoryInspected)
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
            CheckAfterChanges(applicationViewModelInstances[0], expectedLastDirectoryInspected);
        }
    }
}
