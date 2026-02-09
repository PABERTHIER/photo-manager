using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.UI.Controls;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" the Control
// The goal is to test what does FolderNavigationControl
[TestFixture]
public class FolderNavigationControlTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private FolderNavigationViewModel? _folderNavigationViewModel;
    private ApplicationViewModel? _applicationViewModel;
    private AssetRepository? _assetRepository;

    private event EventHandler? FolderSelected;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    [TearDown]
    public void TearDown()
    {
        _folderNavigationViewModel = null;
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

        UserConfigurationService userConfigurationService = new(configurationRootMock.Object);

        Mock<IStorageService> storageServiceMock = new();
        storageServiceMock.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath!);
        storageServiceMock.Setup(x => x.LoadBitmapThumbnailImage(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new BitmapImage());

        Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _assetRepository = new(database, storageServiceMock.Object, userConfigurationService);
        StorageService storageService = new(userConfigurationService);
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService);
        AssetCreationService assetCreationService = new(_assetRepository, storageService, assetHashCalculatorService, userConfigurationService);
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(_assetRepository, storageService, assetCreationService, userConfigurationService, assetsComparator);
        MoveAssetsService moveAssetsService = new(_assetRepository, storageService, assetCreationService);
        SyncAssetsService syncAssetsService = new(_assetRepository, storageService, assetsComparator, moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_assetRepository, storageService, userConfigurationService);
        PhotoManager.Application.Application application = new(_assetRepository, syncAssetsService, catalogAssetsService, moveAssetsService, findDuplicatedAssetsService, userConfigurationService, storageService);
        _applicationViewModel = new(application);
    }

    [Test]
    public void FoldersTreeViewSelectedItemChanged_SelectedPathIsNotSameAsSource_UpdatesSelectedPath()
    {
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2, Directories.NON_EXISTENT_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        List<EventArgs> notifyFolderSelectedEvents = NotifyFolderSelected();

        try
        {
            Folder folder1 = _assetRepository!.AddFolder(assetsDirectory);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            _folderNavigationViewModel = new(_applicationViewModel!, folder1, []);

            CheckBeforeChanges(assetsDirectory, null, folder1, []);

            // First SelectedItemChanged
            string newSelectedPath = SelectedItemChanged(folder2);

            Assert.That(newSelectedPath, Is.EqualTo(otherDirectory));

            CheckAfterChanges(_folderNavigationViewModel, assetsDirectory, null, folder1, null, []);

            // Second SelectedItemChanged
            newSelectedPath = SelectedItemChanged(folder1);

            Assert.That(newSelectedPath, Is.EqualTo(assetsDirectory));

            CheckAfterChanges(_folderNavigationViewModel, assetsDirectory, null, folder1, null, []);

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
        string assetsDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2);
        string otherDirectory = Path.Combine(_dataDirectory!, Directories.DUPLICATES, Directories.NEW_FOLDER_2, Directories.NON_EXISTENT_FOLDER);

        ConfigureApplicationViewModel(100, assetsDirectory, 200, 150, false, false, false, false);

        (
            List<string> notifyPropertyChangedEvents,
            List<ApplicationViewModel> applicationViewModelInstances,
            List<Folder> folderAddedEvents, List<Folder> folderRemovedEvents
        ) = NotifyPropertyChangedEvents();

        List<EventArgs> notifyFolderSelectedEvents = NotifyFolderSelected();

        try
        {
            Folder folder1 = _assetRepository!.AddFolder(assetsDirectory);
            Folder folder2 = _assetRepository!.AddFolder(otherDirectory);

            _folderNavigationViewModel = new(_applicationViewModel!, folder1, []);

            CheckBeforeChanges(assetsDirectory, null, folder1, []);

            // First SelectedItemChanged
            string newSelectedPath = SelectedItemChanged(folder2);

            Assert.That(newSelectedPath, Is.EqualTo(otherDirectory));

            CheckAfterChanges(_folderNavigationViewModel, assetsDirectory, null, folder1, null, []);

            // Second SelectedItemChanged
            newSelectedPath = SelectedItemChanged(folder1);

            Assert.That(newSelectedPath, Is.EqualTo(assetsDirectory));

            CheckAfterChanges(_folderNavigationViewModel, assetsDirectory, null, folder1, null, []);

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

    private List<EventArgs> NotifyFolderSelected()
    {
        List<EventArgs> folderSelectedEvents = [];

        FolderSelected += delegate
        {
            folderSelectedEvents.Add(EventArgs.Empty);
        };

        return folderSelectedEvents;
    }

    private void CheckBeforeChanges(
        string expectedRootDirectory,
        Folder? expectedMoveAssetsLastSelectedFolder,
        Folder? expectedSourceFolder,
        ObservableCollection<string> expectedRecentTargetPaths)
    {
        // From ApplicationViewModel
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SortAscending, Is.True);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.IsRefreshingFolders, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ViewerPosition, Is.EqualTo(0));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.SelectedAssets, Is.Empty);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CurrentFolderPath, Is.EqualTo(expectedRootDirectory));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ObservableAssets, Is.Empty);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedRootDirectory} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CurrentAsset, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CanGoToPreviousAsset, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.CanGoToNextAsset, Is.False);
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(_folderNavigationViewModel!.ApplicationViewModel.AboutInformation.Version, Is.EqualTo(Constants.VERSION));

        // From FolderNavigationViewModel
        if (expectedSourceFolder != null)
        {
            Assert.That(_folderNavigationViewModel!.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
            Assert.That(_folderNavigationViewModel!.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        }
        else
        {
            Assert.That(_folderNavigationViewModel!.SourceFolder, Is.Null);
        }

        Assert.That(_folderNavigationViewModel!.SelectedFolder, Is.Null);

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder!.Id, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(_folderNavigationViewModel!.LastSelectedFolder, Is.Null);
        }

        Assert.That(_folderNavigationViewModel!.CanConfirm, Is.False);
        Assert.That(_folderNavigationViewModel!.HasConfirmed, Is.False);

        for (int i = 0; i < expectedRecentTargetPaths.Count; i++)
        {
            Assert.That(_folderNavigationViewModel!.RecentTargetPaths[i], Is.EqualTo(expectedRecentTargetPaths[i]));
        }

        Assert.That(_folderNavigationViewModel!.TargetPath, Is.Null);
    }

    private static void CheckAfterChanges(
        FolderNavigationViewModel folderNavigationViewModelInstance,
        string expectedLastDirectoryInspected,
        Folder? expectedMoveAssetsLastSelectedFolder,
        Folder? expectedSourceFolder,
        Folder? expectedSelectedFolder,
        ObservableCollection<string> expectedRecentTargetPaths)
    {
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SortAscending, Is.True);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.IsRefreshingFolders, Is.False);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AppMode, Is.EqualTo(AppMode.Thumbnails));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SortCriteria, Is.EqualTo(SortCriteria.FileName));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ThumbnailsVisible, Is.EqualTo(Visibility.Visible));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ViewerVisible, Is.EqualTo(Visibility.Hidden));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ViewerPosition, Is.EqualTo(0));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.SelectedAssets, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CurrentFolderPath, Is.EqualTo(expectedLastDirectoryInspected));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ObservableAssets, Is.Empty);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.GlobalAssetsCounterWording, Is.EqualTo(string.Empty));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.ExecutionTimeWording, Is.EqualTo(string.Empty));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.TotalFilesCountWording, Is.EqualTo(string.Empty));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AppTitle,
            Is.EqualTo($"PhotoManager {Constants.VERSION} - {expectedLastDirectoryInspected} - image 0 of 0 - sorted by file name ascending"));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.StatusMessage, Is.EqualTo(string.Empty));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CurrentAsset, Is.Null);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.MoveAssetsLastSelectedFolder, Is.Null);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CanGoToPreviousAsset, Is.False);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.CanGoToNextAsset, Is.False);
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(folderNavigationViewModelInstance.ApplicationViewModel.AboutInformation.Version, Is.EqualTo(Constants.VERSION));

        // From FolderNavigationViewModel
        if (expectedSourceFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.SourceFolder.Id, Is.EqualTo(expectedSourceFolder.Id));
            Assert.That(folderNavigationViewModelInstance.SourceFolder.Path, Is.EqualTo(expectedSourceFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.SourceFolder, Is.Null);
        }


        if (expectedSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.SelectedFolder!.Id, Is.EqualTo(expectedSelectedFolder.Id));
            Assert.That(folderNavigationViewModelInstance.SelectedFolder.Path, Is.EqualTo(expectedSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.SelectedFolder, Is.Null);
        }

        if (expectedMoveAssetsLastSelectedFolder != null)
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Id, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Id));
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder!.Path, Is.EqualTo(expectedMoveAssetsLastSelectedFolder.Path));
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.LastSelectedFolder, Is.Null);
        }

        Assert.That(folderNavigationViewModelInstance.CanConfirm, Is.False);
        Assert.That(folderNavigationViewModelInstance.HasConfirmed, Is.False);

        if (expectedRecentTargetPaths.Count == 0)
        {
            Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Is.Empty);
        }
        else
        {
            Assert.That(folderNavigationViewModelInstance.RecentTargetPaths, Has.Count.EqualTo(expectedRecentTargetPaths.Count));

            for (int i = 0; i < expectedRecentTargetPaths.Count; i++)
            {
                Assert.That(folderNavigationViewModelInstance.RecentTargetPaths[i], Is.EqualTo(expectedRecentTargetPaths[i]));
            }
        }

        Assert.That(folderNavigationViewModelInstance.TargetPath, Is.Null);
    }

    private string SelectedItemChanged(Folder selectedFolder)
    {
        string newSelectedPath = selectedFolder.Path;
        FolderSelected?.Invoke(this, EventArgs.Empty);

        return newSelectedPath;
    }
}
