using Avalonia.Media.Imaging;
using PhotoManager.Application;
using PhotoManager.Common.Imaging;
using PhotoManager.Domain;
using PhotoManager.Domain.Comparers;
using PhotoManager.UI.Converters;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class ApplicationViewModel : BaseViewModel
{
    private readonly IApplication _application;

    private string _appTitle;
    private string _currentFolderPath;
    private SortableObservableCollection<Asset> _observableAssets;
    private Asset[] _selectedAssets;
    private string _statusMessage;
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
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged(nameof(AppMode), nameof(IsThumbnailsVisible), nameof(IsViewerVisible));
            UpdateAppTitle();
        }
    }

    public SortCriteria SortCriteria
    {
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged(nameof(SortCriteria));
        }
    }

    public bool SortAscending { get; private set; } = true;

    public bool IsThumbnailsVisible => AppMode == AppMode.Thumbnails;

    public bool IsViewerVisible => AppMode == AppMode.Viewer;

    public int ViewerPosition
    {
        get;
        private set
        {
            int clampedPosition = ClampViewerPosition(value);
            bool wasCoerced = clampedPosition != value;

            if (field == clampedPosition && !wasCoerced)
            {
                return;
            }

            field = clampedPosition;

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
        private set
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

    public bool IsRefreshingFolders { get; private set; }

    public string AppTitle
    {
        get => _appTitle;
        private set
        {
            if (_appTitle == value)
            {
                return;
            }

            _appTitle = value;
            NotifyPropertyChanged(nameof(AppTitle));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            NotifyPropertyChanged(nameof(StatusMessage));
        }
    }

    public Asset? CurrentAsset =>
        ViewerPosition < _observableAssets.Count ? _observableAssets[ViewerPosition] : null;

    public Folder? MoveAssetsLastSelectedFolder { get; private set; }

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

    public void SetViewerPosition(int viewerPosition)
    {
        ViewerPosition = viewerPosition;
    }

    public void SetSelectedAssets(Asset[] selectedAssets)
    {
        SelectedAssets = selectedAssets;
    }

    public void SetStatusMessage(string statusMessage)
    {
        StatusMessage = statusMessage;
    }

    public void SetIsRefreshingFolders(bool isRefreshingFolders)
    {
        IsRefreshingFolders = isRefreshingFolders;
    }

    public void SetMoveAssetsLastSelectedFolder(Folder? folder)
    {
        MoveAssetsLastSelectedFolder = folder;
    }

    public void SetAssets(string newCurrentFolderPath, Asset[] assets)
    {
        if (string.Compare(CurrentFolderPath, newCurrentFolderPath, StringComparison.OrdinalIgnoreCase) != 0)
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

        _observableAssets = CreateSortableObservableCollection(filteredAssets);

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
            HashSet<Asset> assetsToRemove = CreateAssetSet(assets);
            SortableObservableCollection<Asset> remainingAssets = [];
            List<Asset> removedAssets = [];

            for (int i = 0; i < _observableAssets.Count; i++)
            {
                Asset observableAsset = _observableAssets[i];

                if (assetsToRemove.Contains(observableAsset))
                {
                    removedAssets.Add(observableAsset);
                }
                else
                {
                    remainingAssets.Add(observableAsset);
                }
            }

            if (removedAssets.Count > 0)
            {
                for (int i = 0; i < removedAssets.Count; i++)
                {
                    removedAssets[i].ImageData?.Dispose();
                    removedAssets[i].ImageData = null;
                }

                _observableAssets = remainingAssets;
                ViewerPosition = ClampViewerPosition(ViewerPosition);
                OnObservableAssetsUpdated();
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
        // To change ascending order when clicking on the same criteria
        SortAscending = (SortCriteria != _previousSortCriteria) || !SortAscending;
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

    public async Task CatalogAssets(CatalogChangeCallback callback, CancellationToken token = default) =>
        await _application.CatalogAssetsAsync(callback, token);

    public ushort GetCatalogCooldownMinutes() => _application.GetCatalogCooldownMinutes();

    public bool GetSyncAssetsEveryXMinutes() => _application.GetSyncAssetsEveryXMinutes();

    public string GetExemptedFolderPath() => _application.GetExemptedFolderPath();

    public Folder[] GetRootCatalogFolders() => _application.GetRootCatalogFolders();

    public Folder[] GetSubFolders(Folder parentFolder) => _application.GetSubFolders(parentFolder);

    public Bitmap? LoadBitmapImageFromPath()
    {
        if (CurrentAsset == null)
        {
            throw new NullReferenceException("CurrentAsset is null");
        }

        using (IImageData imageData =
               _application.LoadBitmapImageFromPath(CurrentAsset.FullPath, CurrentAsset.ImageRotation))
        {
            return AvaloniaBitmapFactory.Create(imageData, ImageEncodingFormat.Jpeg);
        }
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
            SortCriteria.FileCreationDateTime => new DateTimeAssetComparer(SortAscending,
                asset => asset.FileProperties.Creation),
            SortCriteria.FileModificationDateTime => new DateTimeAssetComparer(SortAscending,
                asset => asset.FileProperties.Modification),
            SortCriteria.ThumbnailCreationDateTime => new DateTimeAssetComparer(SortAscending,
                asset => asset.ThumbnailCreationDateTime),
            _ => throw new ArgumentOutOfRangeException(nameof(SortCriteria), "Unknown sort criteria")
        };

        _observableAssets.Sort(comparer);

        OnObservableAssetsUpdated();
    }

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

    private int ClampViewerPosition(int position)
    {
        if (position < 0 || _observableAssets.Count == 0)
        {
            return 0;
        }

        int maximumPosition = _observableAssets.Count - 1;
        return position > maximumPosition ? maximumPosition : position;
    }

    private void AddAsset(Asset asset)
    {
        _observableAssets.Add(asset);
        OnObservableAssetsUpdated();
    }

    private void UpdateAsset(Asset asset)
    {
        Asset? updatedAsset = _observableAssets.FirstOrDefault(a =>
            string.Compare(a.FileName, asset.FileName, StringComparison.OrdinalIgnoreCase) == 0);

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

    private void AddFolder(Folder folder) => FolderAdded?.Invoke(this, new() { Folder = folder });

    private void RemoveFolder(Folder folder) =>
        FolderRemoved?.Invoke(this, new() { Folder = folder });

    private static SortableObservableCollection<Asset> CreateSortableObservableCollection(List<Asset> values)
    {
        SortableObservableCollection<Asset> result = [];

        for (int i = 0; i < values.Count; i++)
        {
            result.Add(values[i]);
        }

        return result;
    }

    private static HashSet<Asset> CreateAssetSet(Asset[] values)
    {
        HashSet<Asset> result = new(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            result.Add(values[i]);
        }

        return result;
    }
}
