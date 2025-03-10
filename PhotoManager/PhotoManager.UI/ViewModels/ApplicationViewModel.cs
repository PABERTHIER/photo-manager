using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Domain.Comparers;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.ViewModels;

public class ApplicationViewModel : BaseViewModel
{
    private AppMode _appMode;
    private string _appTitle;
    private string _currentFolder;
    private int _viewerPosition;
    private SortableObservableCollection<Asset> _observableAssets;
    private Asset[] _selectedAssets;
    private string _statusMessage;
    private SortCriteria _sortCriteria;
    private SortCriteria _previousSortCriteria;
    private string _globalAssetsCounterWording;
    private string _executionTimeWording;
    private string _totalFilesCountWording;

    public ApplicationViewModel(IApplication application) : base(application)
    {
        _observableAssets = [];
        _selectedAssets = [];

        // TODO: Rename to CurrentFolderPath
        CurrentFolder = Application.GetInitialFolderPath();
    }

    public event FolderAddedEventHandler? FolderAdded;
    public event FolderRemovedEventHandler? FolderRemoved;

    public ObservableCollection<Asset> ObservableAssets => _observableAssets;

    public AppMode AppMode
    {
        get { return _appMode; }
        private set
        {
            _appMode = value;
            NotifyPropertyChanged(nameof(AppMode), nameof(ThumbnailsVisible), nameof(ViewerVisible));
            UpdateAppTitle();
        }
    }

    public SortCriteria SortCriteria
    {
        get { return _sortCriteria; }
        private set
        {
            _sortCriteria = value;
            NotifyPropertyChanged(nameof(SortCriteria));
        }
    }

    public bool SortAscending { get; private set; } = true;

    public Visibility ThumbnailsVisible
    {
        get { return AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden; }
    }

    public Visibility ViewerVisible
    {
        get { return AppMode == AppMode.Viewer ? Visibility.Visible : Visibility.Hidden; }
    }

    public int ViewerPosition
    {
        get { return _viewerPosition; }
        set
        {
            _viewerPosition = value;
            NotifyPropertyChanged(
                nameof(ViewerPosition),
                nameof(CanGoToPreviousAsset),
                nameof(CanGoToNextAsset),
                nameof(CurrentAsset));
            UpdateAppTitle();
        }
    }

    public Asset[] SelectedAssets
    {
        get { return _selectedAssets; }
        set
        {
            _selectedAssets = value;
            NotifyPropertyChanged(nameof(SelectedAssets));
        }
    }

    // TODO: Rename to CurrentFolderPath
    public string CurrentFolder
    {
        get { return _currentFolder; }
        // TODO: Set the setter into private -> rework SetAssets to pass the path as parameter into it, to set the value of CurrentFolder
        set
        {
            _currentFolder = value;
            NotifyPropertyChanged(nameof(CurrentFolder));
            UpdateAppTitle();
        }
    }

    public bool IsRefreshingFolders { get; set; }

    public string AppTitle
    {
        get { return _appTitle; }
        set
        {
            _appTitle = value;
            NotifyPropertyChanged(nameof(AppTitle));
        }
    }

    public string StatusMessage
    {
        get { return _statusMessage; }
        set
        {
            _statusMessage = value;
            NotifyPropertyChanged(nameof(StatusMessage));
        }
    }

    public Asset? CurrentAsset
    {
        get { return ObservableAssets?.Count > 0 && ViewerPosition >= 0 ? ObservableAssets?[ViewerPosition] : null; }
    }

    public Folder? MoveAssetsLastSelectedFolder { get; set; }

    public bool CanGoToPreviousAsset => ViewerPosition > 0;

    public bool CanGoToNextAsset => ViewerPosition < (ObservableAssets.Count - 1);

    public string GlobalAssetsCounterWording
    {
        get => _globalAssetsCounterWording;
        private set
        {
            _globalAssetsCounterWording = value;
            NotifyPropertyChanged(nameof(GlobalAssetsCounterWording));
        }
    }

    public string ExecutionTimeWording
    {
        get => _executionTimeWording;
        private set
        {
            _executionTimeWording = value;
            NotifyPropertyChanged(nameof(ExecutionTimeWording));
        }
    }

    public string TotalFilesCountWording
    {
        get => _totalFilesCountWording;
        private set
        {
            _totalFilesCountWording = value;
            NotifyPropertyChanged(nameof(TotalFilesCountWording));
        }
    }

    public string? Product { get; set; }

    public string? Version { get; set; }

    // TODO: When everything tested, rework this to have one method without param (like a toggle)
    public void ChangeAppMode()
    {
        AppMode = AppMode switch
        {
            AppMode.Viewer => AppMode.Thumbnails,
            AppMode.Thumbnails => AppMode.Viewer,
            _ => AppMode
        };
    }

    public void ChangeAppMode(AppMode newAppMode)
    {
        AppMode = newAppMode;
    }

    public void SetAssets(Asset[] assets)
    {
        List<Asset> filteredAssets = [];

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i].ImageData != null)
            {
                filteredAssets.Add(assets[i]);
            }
        }

        _observableAssets = [..filteredAssets];

        if (_observableAssets.Count == 0)
        {
            OnObservableAssetsUpdated();

            return;
        }

        SortAssets();
    }

    public void RemoveAssets(Asset[] assets)
    {
        int initialObservableAssetsCount = ObservableAssets.Count;

        if (initialObservableAssetsCount > 0 && assets.Length > 0)
        {
            for (int i = 0; i < assets.Length; i++)
            {
                ObservableAssets.Remove(assets[i]);

                if (ViewerPosition == ObservableAssets.Count)
                {
                    GoToPreviousAsset();
                }
            }

            if (initialObservableAssetsCount != ObservableAssets.Count)
            {
                OnObservableAssetsUpdated();
            }
        }
    }

    public void GoToAsset(Asset asset)
    {
        GoToAsset(asset, AppMode);
    }

    public void GoToAsset(Asset asset, AppMode newAppMode)
    {
        Asset? observableAsset = null;
        int newViewerPosition = -1;

        for (int i = 0; i < ObservableAssets.Count; i++)
        {
            if (ObservableAssets[i].FileName.AsSpan() == asset.FileName.AsSpan())
            {
                observableAsset = ObservableAssets[i];
                newViewerPosition = i;
                break;
            }
        }

        if (observableAsset != null && Application.FileExists(observableAsset.FullPath))
        {
            if (AppMode != newAppMode)
            {
                ChangeAppMode(newAppMode);
            }

            if (newViewerPosition > -1)
            {
                ViewerPosition = newViewerPosition;
            }
        }
    }

    public void GoToPreviousAsset()
    {
        if (CanGoToPreviousAsset)
        {
            ViewerPosition--;
        }
    }

    public void GoToNextAsset()
    {
        if (CanGoToNextAsset)
        {
            ViewerPosition++;
        }
    }

    public void SortAssetsByCriteria(SortCriteria newSortCriteria)
    {
        _previousSortCriteria = SortCriteria;
        SortCriteria = newSortCriteria;
        SortAscending = (SortCriteria != _previousSortCriteria) || !SortAscending; // To change ascending order when clicking on the same criteria
        SortAssets();
    }

    public void NotifyCatalogChange(CatalogChangeCallbackEventArgs e)
    {
        StatusMessage = e.Message;

        switch (e.Reason)
        {
            case CatalogChangeReason.AssetCreated:
                if (e.Asset?.Folder.Path == CurrentFolder)
                {
                    Application.LoadThumbnail(e.Asset);
                    AddAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetUpdated:
                if (e.Asset?.Folder.Path == CurrentFolder)
                {
                    Application.LoadThumbnail(e.Asset);
                    UpdateAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetDeleted:
                if (e.Asset?.Folder.Path == CurrentFolder)
                {
                    RemoveAssets([e.Asset]);
                }

                break;

            case CatalogChangeReason.FolderCreated:
                if (e.Folder != null)
                {
                    AddFolder(e.Folder);
                }

                break;

            case CatalogChangeReason.FolderDeleted:
                if (e.Folder != null)
                {
                    RemoveFolder(e.Folder);
                }

                break;

            default:
                break;
        }
    }

    public async Task CatalogAssets(CatalogChangeCallback callback, CancellationToken? token = null) => await Application.CatalogAssetsAsync(callback, token);

    public ushort GetCatalogCooldownMinutes() => Application.GetCatalogCooldownMinutes();

    public bool GetSyncAssetsEveryXMinutes() => Application.GetSyncAssetsEveryXMinutes();

    public string GetExemptedFolderPath() => Application.GetExemptedFolderPath();

    public Folder[] GetRootCatalogFolders() => Application.GetRootCatalogFolders();

    public Folder[] GetSubFolders(Folder parentFolder) => Application.GetSubFolders(parentFolder);

    public BitmapImage LoadBitmapImageFromPath()
    {
        if (CurrentAsset == null)
        {
            throw new NullReferenceException("CurrentAsset is null");
        }

        return Application.LoadBitmapImageFromPath(CurrentAsset.FullPath, CurrentAsset.ImageRotation);
    }

    public BitmapImage LoadBitmapHeicImageFromPath()
    {
        if (CurrentAsset == null)
        {
            throw new NullReferenceException("CurrentAsset is null");
        }

        return Application.LoadBitmapHeicImageFromPath(CurrentAsset.FullPath, CurrentAsset.ImageRotation);
    }

    public void CalculateGlobalAssetsCounter()
    {
        int globalAssetsCounter = Application.GetAssetsCounter();
        GlobalAssetsCounterWording = $"Total number of assets: {globalAssetsCounter}";
    }

    public void SetExecutionTime(TimeSpan executionTime)
    {
        ExecutionTimeWording = $"Execution time: {executionTime}";
    }

    public void CalculateTotalFilesCount()
    {
        int totalFilesCount = Application.GetTotalFilesCount();
        TotalFilesCountWording = $"{totalFilesCount} files found";
    }

    private void SortAssets()
    {
        if (_observableAssets.Count == 0)
        {
            UpdateAppTitle();

            return;
        }

        IComparer<Asset> comparer = SortCriteria switch
        {
            SortCriteria.FileName => new StringAssetComparer(SortAscending, asset => asset.FileName),
            SortCriteria.FileSize => new LongAssetComparer(SortAscending, asset => asset.FileProperties.Size),
            SortCriteria.FileCreationDateTime => new DateTimeAssetComparer(SortAscending, asset => asset.FileProperties.Creation),
            SortCriteria.FileModificationDateTime => new DateTimeAssetComparer(SortAscending, asset => asset.FileProperties.Modification),
            SortCriteria.ThumbnailCreationDateTime => new DateTimeAssetComparer(SortAscending, asset => asset.ThumbnailCreationDateTime),
            _ => throw new ArgumentOutOfRangeException(nameof(SortCriteria), "Unknown sort criteria")
        };

        _observableAssets.Sort(comparer);

        OnObservableAssetsUpdated();
    }

    // TODO: Need to rework the content of the title + case where init getting -> image 1 of 0 -> not good
    // TODO: Called to many times, need to rework this
    private void UpdateAppTitle()
    {
        string title = null;
        string sortCriteria = GetSortCriteriaDescription();

        if (AppMode == AppMode.Thumbnails)
        {
            title = string.Format(
                Thread.CurrentThread.CurrentCulture,
                "{0} {1} - {2} - image {3} of {4} - sorted by {5}",
                Product,
                Version,
                CurrentFolder,
                ViewerPosition + 1,
                ObservableAssets?.Count,
                sortCriteria);
        }
        else if (AppMode == AppMode.Viewer)
        {
            title = string.Format(
                Thread.CurrentThread.CurrentCulture,
                "{0} {1} - {2} - {3} - image {4} of {5} - sorted by {6}",
                Product,
                Version,
                CurrentFolder,
                CurrentAsset?.FileName,
                ViewerPosition + 1,
                ObservableAssets?.Count,
                sortCriteria);
        }

        AppTitle = title;
    }

    private string GetSortCriteriaDescription()
    {
        string result = "";

        switch (SortCriteria)
        {
            case SortCriteria.FileName:
                result = "file name";
                break;

            case SortCriteria.FileSize:
                result = "file size";
                break;

            case SortCriteria.FileCreationDateTime:
                result = "file creation";
                break;

            case SortCriteria.FileModificationDateTime:
                result = "file modification";
                break;

            case SortCriteria.ThumbnailCreationDateTime:
                result = "thumbnail creation";
                break;
        }

        result += SortAscending ? " ascending" : " descending";

        return result;
    }

    private void AddAsset(Asset asset)
    {
        ObservableAssets.Add(asset);
        OnObservableAssetsUpdated();
    }

    private void UpdateAsset(Asset asset)
    {
        Asset? updatedAsset = ObservableAssets.FirstOrDefault(
            a => string.Compare(a.FileName, asset.FileName, StringComparison.OrdinalIgnoreCase) == 0);

        if (updatedAsset != null)
        {
            RemoveAssets([updatedAsset]);
            AddAsset(asset);
        }
    }

    private void OnObservableAssetsUpdated()
    {
        NotifyPropertyChanged(nameof(ObservableAssets));
        UpdateAppTitle();
    }

    private void AddFolder(Folder folder) => FolderAdded?.Invoke(this, new FolderAddedEventArgs { Folder = folder });

    private void RemoveFolder(Folder folder) => FolderRemoved?.Invoke(this, new FolderRemovedEventArgs { Folder = folder });
}
