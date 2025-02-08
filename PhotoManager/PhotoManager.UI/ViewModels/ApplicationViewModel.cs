using PhotoManager.Application;
using PhotoManager.Domain;
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

// TODO: Move these 4 into separated file
public class FolderAddedEventArgs
{
    public Folder Folder { get; set; }
}

public class FolderRemovedEventArgs
{
    public Folder Folder { get; set; }
}

public delegate void FolderAddedEventHandler(object sender, FolderAddedEventArgs e);
public delegate void FolderRemovedEventHandler(object sender, FolderRemovedEventArgs e);

public class ApplicationViewModel : BaseViewModel
{
    private AppMode appMode;
    private int viewerPosition;
    private string currentFolder;
    private Asset[] cataloguedAssets; // TODO: ByPath
    private ObservableCollection<Asset> observableAssets; // TODO: ByPath
    private string _globalAssetsCounterWording;
    private string _executionTimeWording;
    private string _totalFilesCountWording;
    private Asset[] selectedAssets;
    private string appTitle;
    private string statusMessage;
    private SortCriteria sortCriteria;
    private SortCriteria _previousSortCriteria;

    public bool SortAscending { get; private set; } = true;
    public string? Product { get; set; }
    public string? Version { get; set; }
    public bool IsRefreshingFolders { get; set; }

    public event FolderAddedEventHandler FolderAdded;
    public event FolderRemovedEventHandler FolderRemoved;

    public ApplicationViewModel(IApplication application) : base(application)
    {
        cataloguedAssets = [];
        observableAssets = [];
        selectedAssets = [];

        // TODO: Rename to CurrentFolderPath
        CurrentFolder = Application.GetInitialFolderPath();
    }

    public AppMode AppMode
    {
        get { return appMode; }
        private set
        {
            appMode = value;
            NotifyPropertyChanged(nameof(AppMode), nameof(ThumbnailsVisible), nameof(ViewerVisible));
            UpdateAppTitle();
        }
    }

    public SortCriteria SortCriteria
    {
        get { return sortCriteria; }
        private set
        {
            sortCriteria = value;
            NotifyPropertyChanged(nameof(SortCriteria));
        }
    }

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
        get { return viewerPosition; }
        set
        {
            viewerPosition = value;
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
        get { return selectedAssets; }
        set
        {
            selectedAssets = value;
            NotifyPropertyChanged(nameof(SelectedAssets));
        }
    }

    // TODO: Rename to CurrentFolderPath
    public string CurrentFolder
    {
        get { return currentFolder; }
        // TODO: Set the setter into private -> rework SetAssets to pass the path as parameter into it, to set the value of CurrentFolder
        set
        {
            currentFolder = value;
            NotifyPropertyChanged(nameof(CurrentFolder));
            UpdateAppTitle();
        }
    }

    // TODO: Not the best thing to update ObservableAssets while CataloguedAssets can also be updated, need to improve this
    // TODO: ByPath
    public ObservableCollection<Asset> ObservableAssets
    {
        get { return observableAssets; }
        private set
        {
            observableAssets = value;
            NotifyPropertyChanged(nameof(ObservableAssets));
            UpdateAppTitle();
        }
    }

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

    public string AppTitle
    {
        get { return appTitle; }
        set
        {
            appTitle = value;
            NotifyPropertyChanged(nameof(AppTitle));
        }
    }

    public string StatusMessage
    {
        get { return statusMessage; }
        set
        {
            statusMessage = value;
            NotifyPropertyChanged(nameof(StatusMessage));
        }
    }

    public Asset? CurrentAsset
    {
        get { return ObservableAssets?.Count > 0 && ViewerPosition >= 0 ? ObservableAssets?[ViewerPosition] : null; }
    }

    public Folder LastSelectedFolder { get; set; }

    public bool CanGoToPreviousAsset
    {
        get
        {
            return ViewerPosition > 0;
        }
    }

    public bool CanGoToNextAsset
    {
        get
        {
            return ViewerPosition < (ObservableAssets?.Count - 1);
        }
    }

    public void ChangeAppMode()
    {
        AppMode = AppMode switch
        {
            AppMode.Viewer => AppMode.Thumbnails,
            AppMode.Thumbnails => AppMode.Viewer,
            _ => AppMode
        };
    }

    public void ChangeAppMode(AppMode appMode)
    {
        AppMode = appMode;
    }

    public void SetAssets(Asset[] assets)
    {
        // The assets that have no image data are filtered out.
        // If a folder is being catalogued for the first time and
        // the GetImages method is called, since the thumbnails file is not
        // created yet, the assets catalogued so far are returned without
        // its thumbnails.
        cataloguedAssets = assets?.Where(a => a.ImageData != null).ToArray() ?? [];
        SortAssets();
    }

    public void RemoveAssets(Asset[] assets)
    {
        int observableAssetsCount = ObservableAssets.Count;

        if (observableAssetsCount > 0 && assets != null)
        {
            foreach (var asset in assets)
            {
                int position = ViewerPosition;
                ObservableAssets.Remove(asset);

                if (position == observableAssetsCount)
                {
                    position--;
                }

                ViewerPosition = position;
            }

            NotifyPropertyChanged(nameof(ObservableAssets));
        }
    }

    public void GoToAsset(Asset asset)
    {
        GoToAsset(asset, AppMode);
    }

    public void GoToAsset(Asset asset, AppMode newAppMode)
    {
        Asset targetAsset = ObservableAssets.FirstOrDefault(f => f.FileName == asset.FileName);

        if (targetAsset != null && Application.FileExists(targetAsset.FullPath))
        {
            int position = ObservableAssets.IndexOf(targetAsset);
            ChangeAppMode(newAppMode);
            ViewerPosition = position;
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

    public void SortAssetsByCriteria(SortCriteria sortCriteria)
    {
        _previousSortCriteria = SortCriteria;
        SortCriteria = sortCriteria;
        SortAscending = (SortCriteria != _previousSortCriteria) || !SortAscending; // To change ascending order when clicking on the same criteria
        SortAssets();
    }

    public void NotifyCatalogChange(CatalogChangeCallbackEventArgs e)
    {
        StatusMessage = e?.Message;

        switch (e?.Reason)
        {
            case CatalogChangeReason.AssetCreated:
                if (e?.Asset?.Folder?.Path == CurrentFolder)
                {
                    // // If the files list is empty or belongs to other directory
                    // if ((ObservableAssets.Count == 0 || ObservableAssets[0].Folder.Path != CurrentFolder) && e.CataloguedAssetsByPath != null)
                    // {
                    //     cataloguedAssets = e.CataloguedAssetsByPath.Where(a => a.ImageData != null).ToArray();
                    //     SortAssets();
                    // }
                    // else
                    // {
                    //     AddAsset(e.Asset);
                    // }

                    Application.LoadThumbnail(e.Asset);
                    AddAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetUpdated:
                if (e?.Asset?.Folder?.Path == CurrentFolder)
                {
                    // // If the files list is empty or belongs to other directory
                    // if ((ObservableAssets.Count == 0 || ObservableAssets[0].Folder.Path != CurrentFolder) && e.CataloguedAssetsByPath != null)
                    // {
                    //     cataloguedAssets = e.CataloguedAssetsByPath.Where(a => a.ImageData != null).ToArray();
                    //     SortAssets();
                    // }
                    // else
                    // {
                    //     UpdateAsset(e.Asset);
                    // }

                    Application.LoadThumbnail(e.Asset);
                    UpdateAsset(e.Asset);
                }

                break;

            case CatalogChangeReason.AssetDeleted:
                if (e?.Asset?.Folder?.Path == CurrentFolder)
                {
                    RemoveAssets([e.Asset]);
                }

                break;

            case CatalogChangeReason.FolderCreated:
                AddFolder(e.Folder);
                break;

            case CatalogChangeReason.FolderDeleted:
                RemoveFolder(e.Folder);
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

    // TODO: Reorder based on the Criteria enum
    private void SortAssets()
    {
        bool isCataloguedAssetsEmpty = cataloguedAssets.Length == 0;

        // TODO: Temp fix waiting for rework about cataloguedAssets
        if (isCataloguedAssetsEmpty && ObservableAssets.Count > 0)
        {
            cataloguedAssets = [..ObservableAssets];
        }

        isCataloguedAssetsEmpty = cataloguedAssets.Length == 0;

        if (!isCataloguedAssetsEmpty)
        {
            // AssetComparer comparer = new (SortAscending);

            switch (SortCriteria)
            {
                case SortCriteria.FileName:
                    // StringAssetComparer fileNameAssetComparer = new (SortAscending, asset => asset.FileName);
                    // cataloguedAssets.AsSpan().Sort(fileNameAssetComparer);
                    cataloguedAssets = SortAscending ?
                        cataloguedAssets.OrderBy(a => a.FileName).ToArray() :
                        cataloguedAssets.OrderByDescending(a => a.FileName).ToArray();
                    break;

                case SortCriteria.ThumbnailCreationDateTime:
                    // DateTimeAssetComparer dateTimeThumbnailCreationAssetComparer = new (SortAscending, asset => asset.ThumbnailCreationDateTime);
                    // cataloguedAssets.AsSpan().Sort(dateTimeThumbnailCreationAssetComparer);
                    cataloguedAssets = SortAscending ?
                        cataloguedAssets.OrderBy(a => a.ThumbnailCreationDateTime).ThenBy(a => a.FileName).ToArray() :
                        cataloguedAssets.OrderByDescending(a => a.ThumbnailCreationDateTime).ThenByDescending(a => a.FileName).ToArray();
                    break;

                case SortCriteria.FileCreationDateTime:
                    // cataloguedAssets.AsSpan().Sort(comparer);
                    // DateTimeAssetComparer dateTimeCreationAssetComparer = new (SortAscending, asset => asset.FileProperties.Creation);
                    // cataloguedAssets.AsSpan().Sort(dateTimeCreationAssetComparer);
                    cataloguedAssets = SortAscending ?
                        cataloguedAssets.OrderBy(a => a.FileProperties.Creation).ThenBy(a => a.FileName).ToArray() :
                        cataloguedAssets.OrderByDescending(a => a.FileProperties.Creation).ThenByDescending(a => a.FileName).ToArray();
                    break;

                case SortCriteria.FileModificationDateTime:
                    // DateTimeAssetComparer dateTimeModificationAssetComparer = new (SortAscending, asset => asset.FileProperties.Modification);
                    // cataloguedAssets.AsSpan().Sort(dateTimeModificationAssetComparer);
                    cataloguedAssets = SortAscending ?
                        cataloguedAssets.OrderBy(a => a.FileProperties.Modification).ThenBy(a => a.FileName).ToArray() :
                        cataloguedAssets.OrderByDescending(a => a.FileProperties.Modification).ThenByDescending(a => a.FileName).ToArray();
                    break;

                case SortCriteria.FileSize:
                    // LongAssetComparer sizeAssetComparer = new (SortAscending, asset => asset.FileProperties.Size);
                    // cataloguedAssets.AsSpan().Sort(sizeAssetComparer);
                    cataloguedAssets = SortAscending ?
                        cataloguedAssets.OrderBy(a => a.FileProperties.Size).ThenBy(a => a.FileName).ToArray() :
                        cataloguedAssets.OrderByDescending(a => a.FileProperties.Size).ThenByDescending(a => a.FileName).ToArray();
                    break;
            }
        }

        ObservableAssets = !isCataloguedAssetsEmpty ? new ObservableCollection<Asset>(cataloguedAssets) : [];
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
        if (ObservableAssets != null)
        {
            ObservableAssets.Add(asset);
            NotifyPropertyChanged(nameof(ObservableAssets));
            UpdateAppTitle();
        }
    }

    private void UpdateAsset(Asset asset)
    {
        if (ObservableAssets != null)
        {
            // TODO: Need to also check path and hash (even if the path is checked above)
            var updatedAsset = ObservableAssets.FirstOrDefault(
                a => string.Compare(a.FileName, asset.FileName, StringComparison.OrdinalIgnoreCase) == 0);

            if (updatedAsset != null)
            {
                RemoveAssets(new Asset[] { updatedAsset });
                AddAsset(asset);
                NotifyPropertyChanged(nameof(ObservableAssets));
                UpdateAppTitle();
            }
        }
    }

    private void AddFolder(Folder folder) => FolderAdded?.Invoke(this, new FolderAddedEventArgs { Folder = folder });

    private void RemoveFolder(Folder folder) => FolderRemoved?.Invoke(this, new FolderRemovedEventArgs { Folder = folder });
}
