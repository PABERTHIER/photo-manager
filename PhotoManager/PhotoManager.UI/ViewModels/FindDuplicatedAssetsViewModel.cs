using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public class FindDuplicatedAssetsViewModel(IApplication application) : BaseViewModel(application)
{
    private List<DuplicatedSetViewModel> _duplicatedAssetSets = [];
    private int _duplicatedAssetSetsPosition;
    private int _duplicatedAssetPosition;

    public List<DuplicatedSetViewModel> DuplicatedAssetSets
    {
        get => _duplicatedAssetSets;
        private set
        {
            _duplicatedAssetSets = value;
            NotifyPropertyChanged(nameof(DuplicatedAssetSets));
            DuplicatedAssetSetsPosition = 0;
        }
    }

    public int DuplicatedAssetSetsPosition
    {
        get => _duplicatedAssetSetsPosition;
        set
        {
            _duplicatedAssetSetsPosition = value;
            NotifyPropertyChanged(nameof(DuplicatedAssetSetsPosition), nameof(CurrentDuplicatedAssetSet));
            DuplicatedAssetPosition = 0;
        }
    }

    public int DuplicatedAssetPosition
    {
        get => _duplicatedAssetPosition;
        set
        {
            _duplicatedAssetPosition = value;

            if (CurrentDuplicatedAsset is { Asset.ImageData: null })
            {
                Application.LoadThumbnail(CurrentDuplicatedAsset.Asset);
            }

            if (CurrentDuplicatedAsset is { Asset.ImageData: null })
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
            DuplicatedSetViewModel result = [];

            if (DuplicatedAssetSets.Count > 0 && DuplicatedAssetSetsPosition >= 0)
            {
                result = DuplicatedAssetSets[DuplicatedAssetSetsPosition];
            }

            return result;
        }
    }

    public DuplicatedAssetViewModel? CurrentDuplicatedAsset
    {
        get
        {
            DuplicatedAssetViewModel? result = null;

            if (CurrentDuplicatedAssetSet.Count > 0 && DuplicatedAssetPosition >= 0)
            {
                result = CurrentDuplicatedAssetSet[DuplicatedAssetPosition];
            }

            return result;
        }
    }

    public void SetDuplicates(List<List<Asset>> duplicatedAssetsSets)
    {
        if (duplicatedAssetsSets == null)
        {
            throw new ArgumentNullException(nameof(duplicatedAssetsSets));
        }

        List<DuplicatedSetViewModel> collection = [];

        foreach (List<Asset> duplicatedSet in duplicatedAssetsSets)
        {
            DuplicatedSetViewModel duplicatedSetViewModel = [];

            foreach (Asset asset in duplicatedSet)
            {
                duplicatedSetViewModel.Add(new (Application) { Asset = asset, ParentViewModel = duplicatedSetViewModel });
            }

            collection.Add(duplicatedSetViewModel);
        }

        DuplicatedAssetSets = collection;
    }

    public void Refresh()
    {
        List<List<Asset>> duplicatedAssetsSets = Application.GetDuplicatedAssets();
        SetDuplicates(duplicatedAssetsSets);
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
        var nextVisibleSet = DuplicatedAssetSets
            .Where(s => s.Visible == Visibility.Visible
                && DuplicatedAssetSets.IndexOf(s) > currentIndex)
            .FirstOrDefault();

        if (nextVisibleSet != null)
        {
            int nextIndex = DuplicatedAssetSets.IndexOf(nextVisibleSet);
            DuplicatedAssetSetsPosition = nextIndex;
        }
        else
        {
            NavigateToPreviousVisibleSet(currentIndex);
        }
    }

    private void NavigateToPreviousVisibleSet(int currentIndex)
    {
        var previousVisibleSet = DuplicatedAssetSets
            .Where(s => s.Visible == Visibility.Visible
                && DuplicatedAssetSets.IndexOf(s) < currentIndex)
            .LastOrDefault();

        if (previousVisibleSet != null)
        {
            int nextIndex = DuplicatedAssetSets.IndexOf(previousVisibleSet);
            DuplicatedAssetSetsPosition = nextIndex;
        }
        else
        {
            MessageBox.Show("All duplicates have been deleted. \nGood Job ;)", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
