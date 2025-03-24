using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public class FindDuplicatedAssetsViewModel : BaseViewModel
{
    private List<List<Asset>> _duplicatedAssets;
    private List<DuplicatedSetViewModel> _collection;
    private int _duplicatedAssetSetsPosition;
    private int _duplicatedAssetPosition;

    public FindDuplicatedAssetsViewModel(IApplication application) : base(application)
    {
    }

    public List<DuplicatedSetViewModel> DuplicatedAssetSetsCollection
    {
        get { return _collection; }
        private set
        {
            _collection = value;
            NotifyPropertyChanged(nameof(DuplicatedAssetSetsCollection));
            DuplicatedAssetSetsPosition = 0;
        }
    }

    public List<DuplicatedSetViewModel> GetAllDuplicatedAssets()
    {
        return _collection;
    }

    public int DuplicatedAssetSetsPosition
    {
        get { return _duplicatedAssetSetsPosition; }
        set
        {
            _duplicatedAssetSetsPosition = value;
            NotifyPropertyChanged(nameof(DuplicatedAssetSetsPosition), nameof(CurrentDuplicatedAssetSet));
            DuplicatedAssetPosition = 0;
        }
    }

    public void SetDuplicates(List<List<Asset>> duplicatedAssets)
    {
        if (duplicatedAssets == null)
        {
            throw new ArgumentNullException(nameof(duplicatedAssets));
        }

        _duplicatedAssets = duplicatedAssets;
        List<DuplicatedSetViewModel> collection = new();

        foreach (var duplicatedSet in duplicatedAssets)
        {
            DuplicatedSetViewModel duplicatedSetViewModel = new();

            foreach (var asset in duplicatedSet)
            {
                duplicatedSetViewModel.Add(
                    new DuplicatedAssetViewModel(Application)
                    {
                        Asset = asset,
                        Visible = Visibility.Visible,
                        ParentViewModel = duplicatedSetViewModel
                    });
            }

            collection.Add(duplicatedSetViewModel);
        }

        DuplicatedAssetSetsCollection = collection;
    }

    public int DuplicatedAssetPosition
    {
        get { return _duplicatedAssetPosition; }
        set
        {
            _duplicatedAssetPosition = value;

            DuplicatedAssetViewModel assetViewModel = CurrentDuplicatedAsset;

            if (assetViewModel != null && assetViewModel.Asset != null && assetViewModel.Asset.ImageData == null)
            {
                Application.LoadThumbnail(assetViewModel.Asset);
            }

            if (assetViewModel != null && assetViewModel.Asset != null && assetViewModel.Asset.ImageData == null)
            {
                Refresh();
            }
            else
            {
                NotifyPropertyChanged(nameof(DuplicatedAssetPosition), nameof(CurrentDuplicatedAsset));
            }
        }
    }

    public DuplicatedSetViewModel CurrentDuplicatedAssetSet
    {
        get
        {
            DuplicatedSetViewModel result = null;

            if (DuplicatedAssetSetsCollection != null && DuplicatedAssetSetsCollection.Count > 0 && DuplicatedAssetSetsPosition >= 0)
            {
                result = DuplicatedAssetSetsCollection[DuplicatedAssetSetsPosition];
            }

            return result;
        }
    }

    public DuplicatedAssetViewModel CurrentDuplicatedAsset
    {
        get
        {
            DuplicatedAssetViewModel result = null;

            if (CurrentDuplicatedAssetSet != null && CurrentDuplicatedAssetSet.Count > 0 && DuplicatedAssetPosition >= 0)
            {
                result = CurrentDuplicatedAssetSet[DuplicatedAssetPosition];
            }

            return result;
        }
    }

    public void Refresh()
    {
        var duplicates = Application.GetDuplicatedAssets();
        SetDuplicates(duplicates);
    }

    public void DeleteAsset(DuplicatedAssetViewModel assetViewModel, MainWindow mainWindowInstance)
    {
        mainWindowInstance.DeleteDuplicateAssets(new Asset[] { assetViewModel.Asset });

        assetViewModel.Visible = Visibility.Collapsed;

        if (assetViewModel.ParentViewModel.Where(x => x.Visible == Visibility.Visible).Count() <= 1) // We want to navigate only when we are deleting the last duplicate
        {
            NavigateToNextVisibleSet(DuplicatedAssetSetsPosition);
        }
    }

    public void DeleteAssets(List<DuplicatedAssetViewModel> assetViewModelList, MainWindow mainWindowInstance)
    {
        mainWindowInstance.DeleteDuplicateAssets(assetViewModelList.Select(x => x.Asset).ToArray());

        assetViewModelList.ForEach(x => x.Visible = Visibility.Collapsed);

        NavigateToNextVisibleSet(DuplicatedAssetSetsPosition);
    }

    private void NavigateToNextVisibleSet(int currentIndex)
    {
        var nextVisibleSet = DuplicatedAssetSetsCollection
            .Where(s => s.Visible == Visibility.Visible
                && DuplicatedAssetSetsCollection.IndexOf(s) > currentIndex)
            .FirstOrDefault();

        if (nextVisibleSet != null)
        {
            int nextIndex = DuplicatedAssetSetsCollection.IndexOf(nextVisibleSet);
            DuplicatedAssetSetsPosition = nextIndex;
        }
        else
        {
            NavigateToPreviousVisibleSet(currentIndex);
        }
    }

    private void NavigateToPreviousVisibleSet(int currentIndex)
    {
        var previousVisibleSet = DuplicatedAssetSetsCollection
            .Where(s => s.Visible == Visibility.Visible
                && DuplicatedAssetSetsCollection.IndexOf(s) < currentIndex)
            .LastOrDefault();

        if (previousVisibleSet != null)
        {
            int nextIndex = DuplicatedAssetSetsCollection.IndexOf(previousVisibleSet);
            DuplicatedAssetSetsPosition = nextIndex;
        }
        else
        {
            MessageBox.Show("All duplicates have been deleted. \nGood Job ;)", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

// TODO: Merge this VM into FindDuplicatedAssetsViewModel since it's just a List<DuplicatedAssetViewModel>
public class DuplicatedSetViewModel : List<DuplicatedAssetViewModel>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    // TODO: Delete this code since it's the same in BaseViewModel ?
    protected void NotifyPropertyChanged(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public string FileName => this[0].Asset.FileName;

    public int DuplicatesCount => GetVisibleDuplicates();

    public Visibility Visible => GetVisibleDuplicates() > 1 ? Visibility.Visible : Visibility.Collapsed;

    public void NotifyAssetChanged(DuplicatedAssetViewModel asset)
    {
        NotifyPropertyChanged(nameof(DuplicatesCount), nameof(Visible));
    }

    private int GetVisibleDuplicates()
    {
        return this.Count(vm => vm.Visible == Visibility.Visible);
    }
}
