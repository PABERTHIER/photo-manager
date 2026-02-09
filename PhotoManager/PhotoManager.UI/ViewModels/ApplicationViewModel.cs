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
    private readonly IApplication _application;

    private AppMode _appMode;
    private string _appTitle;
    private string _currentFolderPath;
    private int _viewerPosition;
    private SortableObservableCollection<Asset> _observableAssets;
    private Asset[] _selectedAssets;
    private string _statusMessage;
    private SortCriteria _sortCriteria;
    private SortCriteria _previousSortCriteria;
    private string _globalAssetsCounterWording;
    private string _executionTimeWording;
    private string _totalFilesCountWording;

    private static readonly Dictionary<SortCriteria, string> CriteriaDescriptions =
        new()
        {
            [SortCriteria.FileName] = "file name",
            [SortCriteria.FileSize] = "file size",
            [SortCriteria.FileCreationDateTime] = "file creation",
            [SortCriteria.FileModificationDateTime] = "file modification",
            [SortCriteria.ThumbnailCreationDateTime] = "thumbnail creation"
        };

    // TODO: Private set for all props + Update UI to set mode OneWay
    public ApplicationViewModel(IApplication application)
    {
        _application = application;

        _appTitle = string.Empty;
        _statusMessage = string.Empty;
        _observableAssets = [];
        _selectedAssets = [];
        _currentFolderPath = _application.GetInitialFolderPath();

        _globalAssetsCounterWording = string.Empty;
        _executionTimeWording = string.Empty;
        _totalFilesCountWording = string.Empty;

        AboutInformation = _application.GetAboutInformation(GetType().Assembly);
        UpdateAppTitle();
    }

    public event FolderAddedEventHandler? FolderAdded;
    public event FolderRemovedEventHandler? FolderRemoved;

    public ObservableCollection<Asset> ObservableAssets => _observableAssets;

    public AppMode AppMode
    {
        get => _appMode;
        private set
        {
            _appMode = value;
            NotifyPropertyChanged(nameof(AppMode), nameof(ThumbnailsVisible), nameof(ViewerVisible));
            UpdateAppTitle();
        }
    }

    public SortCriteria SortCriteria
    {
        get => _sortCriteria;
        private set
        {
            _sortCriteria = value;
            NotifyPropertyChanged(nameof(SortCriteria));
        }
    }

    public bool SortAscending { get; private set; } = true;

    public Visibility ThumbnailsVisible => AppMode == AppMode.Thumbnails ? Visibility.Visible : Visibility.Hidden;

    public Visibility ViewerVisible => AppMode == AppMode.Viewer ? Visibility.Visible : Visibility.Hidden;

    // TODO: Rework ViewerPosition to make the setter private (xaml binding issues)
    public int ViewerPosition
    {
        get => _viewerPosition;
        set
        {
            // TODO: ViewerPosition is reset to -1 when moving and deleting assets from MainWindow
            // For this case, the best would be keeping the old ViewerPosition so that we can go to the nearest asset
            _viewerPosition = value < 0 ? 0 : value;

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
        get => _selectedAssets;
        set
        {
            _selectedAssets = value;
            NotifyPropertyChanged(nameof(SelectedAssets));
        }
    }

    public string CurrentFolderPath
    {
        get => _currentFolderPath;
        private set
        {
            _currentFolderPath = value;
            NotifyPropertyChanged(nameof(CurrentFolderPath));
            UpdateAppTitle();
        }
    }

    public bool IsRefreshingFolders { get; set; }

    // TODO: Private set + Update UI to set mode OneWay
    public string AppTitle
    {
        get => _appTitle;
        set
        {
            _appTitle = value;
            NotifyPropertyChanged(nameof(AppTitle));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            NotifyPropertyChanged(nameof(StatusMessage));
        }
    }

    public Asset? CurrentAsset => _observableAssets.Count > 0 ? _observableAssets[ViewerPosition] : null;

    public Folder? MoveAssetsLastSelectedFolder { get; set; }

    public bool CanGoToPreviousAsset => ViewerPosition > 0;

    public bool CanGoToNextAsset => ViewerPosition < (_observableAssets.Count - 1);

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

    public AboutInformation AboutInformation { get; }

    public void ChangeAppMode()
    {
        AppMode = AppMode == AppMode.Thumbnails ? AppMode.Viewer : AppMode.Thumbnails;
    }

    public void SetAssets(string newCurrentFolderPath, Asset[] assets)
    {
        if (CurrentFolderPath.AsSpan() != newCurrentFolderPath.AsSpan())
        {
            CurrentFolderPath = newCurrentFolderPath;
        }

        List<Asset> filteredAssets = [];

        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i].ImageData != null)
            {
                filteredAssets.Add(assets[i]);
            }
        }

        _observableAssets = [.. filteredAssets];

        if (_observableAssets.Count == 0)
        {
            OnObservableAssetsUpdated();

            return;
        }

        SortAssets();
    }

    public void RemoveAssets(Asset[] assets)
    {
        int initialObservableAssetsCount = _observableAssets.Count;

        if (initialObservableAssetsCount > 0 && assets.Length > 0)
        {
            for (int i = 0; i < assets.Length; i++)
            {
                _observableAssets.Remove(assets[i]);

                if (ViewerPosition == _observableAssets.Count)
                {
                    GoToPreviousAsset();
                }
            }

            if (initialObservableAssetsCount != _observableAssets.Count)
            {
                OnObservableAssetsUpdated();
            }
        }
    }

    // TODO: Dead code: Delete this method and tests
    public void GoToAsset(Asset asset)
    {
        GoToAsset(asset, AppMode);
    }

    // TODO: Dead code: Delete this method and tests
    public void GoToAsset(Asset asset, AppMode newAppMode)
    {
        Asset? observableAsset = null;
        int newViewerPosition = -1;

        for (int i = 0; i < _observableAssets.Count; i++)
        {
            if (_observableAssets[i].FileName.AsSpan() == asset.FileName.AsSpan())
            {
                observableAsset = _observableAssets[i];
                newViewerPosition = i;
                break;
            }
        }

        if (observableAsset != null && _application.FileExists(observableAsset.FullPath))
        {
            if (AppMode != newAppMode)
            {
                ChangeAppMode();
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
                if (e.Asset?.Folder.Path == CurrentFolderPath)
                {
                    _application.LoadThumbnail(e.Asset);
                    AddAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetUpdated:
                if (e.Asset?.Folder.Path == CurrentFolderPath)
                {
                    _application.LoadThumbnail(e.Asset);
                    UpdateAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetDeleted:
                if (e.Asset?.Folder.Path == CurrentFolderPath)
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

            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }
    }

    public async Task CatalogAssets(CatalogChangeCallback callback, CancellationToken? token = null) => await _application.CatalogAssetsAsync(callback, token);

    public ushort GetCatalogCooldownMinutes() => _application.GetCatalogCooldownMinutes();

    public bool GetSyncAssetsEveryXMinutes() => _application.GetSyncAssetsEveryXMinutes();

    public string GetExemptedFolderPath() => _application.GetExemptedFolderPath();

    public Folder[] GetRootCatalogFolders() => _application.GetRootCatalogFolders();

    public Folder[] GetSubFolders(Folder parentFolder) => _application.GetSubFolders(parentFolder);

    public BitmapImage LoadBitmapImageFromPath()
    {
        if (CurrentAsset == null)
        {
            throw new NullReferenceException("CurrentAsset is null");
        }

        return _application.LoadBitmapImageFromPath(CurrentAsset.FullPath, CurrentAsset.ImageRotation);
    }

    public BitmapImage LoadBitmapHeicImageFromPath()
    {
        if (CurrentAsset == null)
        {
            throw new NullReferenceException("CurrentAsset is null");
        }

        return _application.LoadBitmapHeicImageFromPath(CurrentAsset.FullPath, CurrentAsset.ImageRotation);
    }

    public void CalculateGlobalAssetsCounter()
    {
        int globalAssetsCounter = _application.GetAssetsCounter();
        GlobalAssetsCounterWording = $"Total number of assets: {globalAssetsCounter}";
    }

    public void SetExecutionTime(TimeSpan executionTime)
    {
        ExecutionTimeWording = $"Execution time: {executionTime}";
    }

    public void CalculateTotalFilesCount()
    {
        int totalFilesCount = _application.GetTotalFilesCount();
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

    // TODO: Called to many times, need to rework this (reduce event "AppTitle")
    private void UpdateAppTitle()
    {
        int observableAssetsCount = _observableAssets.Count;
        int viewerPosition = observableAssetsCount > 0 ? ViewerPosition + 1 : 0;
        string sortCriteriaDescription = GetSortCriteriaDescription();
        string baseTitle = $"{AboutInformation.Product} {AboutInformation.Version} - {CurrentFolderPath}";

        AppTitle = AppMode == AppMode.Thumbnails
            ? $"{baseTitle} - image {viewerPosition} of {observableAssetsCount} - sorted by {sortCriteriaDescription}"
            : $"{baseTitle} - {CurrentAsset?.FileName} - image {viewerPosition} of {observableAssetsCount} - sorted by {sortCriteriaDescription}";
    }

    private string GetSortCriteriaDescription() =>
        $"{CriteriaDescriptions[SortCriteria]} {(SortAscending ? "ascending" : "descending")}";

    private void AddAsset(Asset asset)
    {
        _observableAssets.Add(asset);
        OnObservableAssetsUpdated();
    }

    private void UpdateAsset(Asset asset)
    {
        Asset? updatedAsset = _observableAssets.FirstOrDefault(
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
