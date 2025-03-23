using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Windows;

namespace PhotoManager.Tests.Integration.UI.Controls;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Control
// The goal is to test what does FolderNavigationControl
[TestFixture]
public class FolderNavigationControlTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private ApplicationViewModel? _applicationViewModel;
    private PhotoManager.Application.Application? _application;
    private AssetRepository? _assetRepository;

    public event EventHandler? FolderSelected;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);
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
    public void FoldersTreeViewSelectedItemChanged_SelectedPathIsNotSameAsSource_UpdatesSelectedPath()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string otherDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2", "Toto");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        List<EventArgs> notifyFolderSelectedEvents = NotifyFolderSelected();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Folder folder1 = _assetRepository!.AddFolder(assetsDirectory);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            // First SelectedItemChanged
            string newSelectedPath = SelectedItemChanged(folder2);

            Assert.That(newSelectedPath, Is.EqualTo(otherDirectory));

            CheckAfterChanges(_applicationViewModel!, assetsDirectory);

            // Second SelectedItemChanged
            newSelectedPath = SelectedItemChanged(folder1);

            Assert.That(newSelectedPath, Is.EqualTo(assetsDirectory));

            CheckAfterChanges(_applicationViewModel!, assetsDirectory);

            Assert.That(applicationViewModelInstances, Is.Empty);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(notifyFolderSelectedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyFolderSelectedEvents[0], Is.EqualTo(EventArgs.Empty));
            Assert.That(notifyFolderSelectedEvents[1], Is.EqualTo(EventArgs.Empty));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }

    [Test]
    public void FoldersTreeViewSelectedItemChanged_SelectedPathIsSameAsSource_UpdatesSelectedPath()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2");
        string otherDirectory = Path.Combine(_dataDirectory!, "Duplicates", "NewFolder2", "Toto");

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        List<EventArgs> notifyFolderSelectedEvents = NotifyFolderSelected();

        try
        {
            CheckBeforeChanges(assetsDirectory);

            Folder folder1 = _assetRepository!.AddFolder(assetsDirectory);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            // First SelectedItemChanged
            string newSelectedPath = SelectedItemChanged(folder2);

            Assert.That(newSelectedPath, Is.EqualTo(otherDirectory));

            CheckAfterChanges(_applicationViewModel!, assetsDirectory);

            // Second SelectedItemChanged
            newSelectedPath = SelectedItemChanged(folder1);

            Assert.That(newSelectedPath, Is.EqualTo(assetsDirectory));

            CheckAfterChanges(_applicationViewModel!, assetsDirectory);

            Assert.That(applicationViewModelInstances, Is.Empty);

            Assert.That(notifyPropertyChangedEvents, Is.Empty);

            // Because the root folder is already added
            Assert.That(folderAddedEvents, Is.Empty);
            Assert.That(folderRemovedEvents, Is.Empty);

            Assert.That(notifyFolderSelectedEvents, Has.Count.EqualTo(2));
            Assert.That(notifyFolderSelectedEvents[0], Is.EqualTo(EventArgs.Empty));
            Assert.That(notifyFolderSelectedEvents[1], Is.EqualTo(EventArgs.Empty));
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

    private List<EventArgs> NotifyFolderSelected()
    {
        List<EventArgs> folderSelectedEvents = [];

        FolderSelected += delegate
        {
            folderSelectedEvents.Add(EventArgs.Empty);
        };

        return folderSelectedEvents;
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

    private static void CheckAfterChanges(ApplicationViewModel applicationViewModelInstance, string expectedLastDirectoryInspected)
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
        Assert.That(applicationViewModelInstance.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        Assert.That(applicationViewModelInstance.ObservableAssets, Is.Empty);
        Assert.That(applicationViewModelInstance.GlobalAssetsCounterWording, Is.Null);
        Assert.That(applicationViewModelInstance.ExecutionTimeWording, Is.Null);
        Assert.That(applicationViewModelInstance.TotalFilesCountWording, Is.Null);
        Assert.That(applicationViewModelInstance.AppTitle, Is.EqualTo($"  - {expectedLastDirectoryInspected} - image 1 of 0 - sorted by file name ascending"));
        Assert.That(applicationViewModelInstance.StatusMessage, Is.Null);
        Assert.That(applicationViewModelInstance.CurrentAsset, Is.Null);
        Assert.That(applicationViewModelInstance.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(applicationViewModelInstance.CanGoToPreviousAsset, Is.False);
        Assert.That(applicationViewModelInstance.CanGoToNextAsset, Is.False);
    }

    private string SelectedItemChanged(Folder selectedFolder)
    {
        string newSelectedPath = selectedFolder.Path;
        FolderSelected?.Invoke(this, EventArgs.Empty);

        return newSelectedPath;
    }
}
