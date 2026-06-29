namespace PhotoManager.UI.ViewModels;

public class FindDuplicatedAssetsViewModel(IApplication application) : BaseViewModel
{
    public event MessageBoxInformationSentEventHandler? MessageBoxInformationSent;

    public DuplicatedSetViewModel[] DuplicatedAssetSets
    {
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged(nameof(DuplicatedAssetSets));
            DuplicatedAssetSetsPosition = 0;
        }
    } = [];

    public int DuplicatedAssetSetsPosition
    {
        get;
        set
        {
            field = NormalizeDuplicatedAssetSetsPosition(value);
            NotifyPropertyChanged(nameof(DuplicatedAssetSetsPosition), nameof(CurrentDuplicatedAssetSet));
            ResetDuplicatedAssetPosition();
        }
    }

    public int DuplicatedAssetPosition
    {
        get;
        set
        {
            field = NormalizeDuplicatedAssetPosition(value);

            if (CurrentDuplicatedAsset is { Asset.ImageData: null })
            {
                application.LoadThumbnail(CurrentDuplicatedAsset.Asset);
            }

            NotifyPropertyChanged(nameof(DuplicatedAssetPosition), nameof(CurrentDuplicatedAsset));
        }
    }

    public DuplicatedSetViewModel CurrentDuplicatedAssetSet
    {
        get
        {
            DuplicatedSetViewModel result = [];

            if (DuplicatedAssetSets.Length > 0
                && DuplicatedAssetSetsPosition >= 0
                && DuplicatedAssetSetsPosition < DuplicatedAssetSets.Length)
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

            if (CurrentDuplicatedAssetSet.Count > 0
                && DuplicatedAssetPosition >= 0
                && DuplicatedAssetPosition < CurrentDuplicatedAssetSet.Count)
            {
                result = CurrentDuplicatedAssetSet[DuplicatedAssetPosition];
            }

            return result;
        }
    }

    public void SetDuplicates(DuplicatedSetViewModel[] duplicatedAssetSets)
    {
        DuplicatedAssetSets = duplicatedAssetSets;
    }

    public static DuplicatedSetViewModel[] CreateDuplicatedAssetSets(Asset[][] assetsSets)
    {
        DuplicatedSetViewModel[] duplicatedAssetSets = new DuplicatedSetViewModel[assetsSets.Length];

        for (int i = 0; i < assetsSets.Length; i++)
        {
            Asset[] assets = assetsSets[i];
            DuplicatedSetViewModel duplicatedSetViewModel = [];

            foreach (Asset asset in assets)
            {
                duplicatedSetViewModel.Add(new() { Asset = asset, ParentViewModel = duplicatedSetViewModel });
            }

            duplicatedAssetSets[i] = duplicatedSetViewModel;
        }

        return duplicatedAssetSets;
    }

    public async Task Refresh()
    {
        // The catalog read + grouping and the view-model construction are the heavy part and must not
        // freeze the UI thread; only the assignment that raises PropertyChanged stays on the UI thread.
        DuplicatedSetViewModel[] duplicatedAssetSets = await Task.Run(() =>
            CreateDuplicatedAssetSets(application.GetDuplicatedAssets()));

        SetDuplicates(duplicatedAssetSets);
    }

    public DuplicatedAssetViewModel[] GetDuplicatedAssets(Asset asset)
    {
        if (DuplicatedAssetSets.Length == 0)
        {
            return [];
        }

        string assetFolderPath = asset.Folder.Path;
        string assetFileName = asset.FileName;

        // Locate the set that actually contains the clicked asset by its identity (folder path + file name) instead of
        // re-deriving it from the hash: under PHash a set groups near-duplicates that do NOT share the same Hash, so
        // the previous match against the set's representative hash (set[0].Hash) failed to find the set whenever the
        // clicked asset was not that representative, and "Delete all and keep this one" then silently deleted nothing.
        // (folder path + file name) is the catalogued asset identity (the Assets table primary key), so it uniquely
        // designates the clicked asset across every set.
        DuplicatedSetViewModel? duplicatedAssetSet = null;

        for (int i = 0; i < DuplicatedAssetSets.Length; i++)
        {
            DuplicatedSetViewModel currentSet = DuplicatedAssetSets[i];

            for (int j = 0; j < currentSet.Count; j++)
            {
                if (currentSet[j].Asset.Folder.Path == assetFolderPath && currentSet[j].Asset.FileName == assetFileName)
                {
                    duplicatedAssetSet = currentSet;
                    break;
                }
            }

            if (duplicatedAssetSet != null)
            {
                break;
            }
        }

        if (duplicatedAssetSet == null)
        {
            return [];
        }

        List<DuplicatedAssetViewModel> assetsToDelete = [];

        for (int i = 0; i < duplicatedAssetSet.Count; i++)
        {
            DuplicatedAssetViewModel duplicatedAsset = duplicatedAssetSet[i];

            if ((duplicatedAsset.Asset.Folder.Path == assetFolderPath && duplicatedAsset.Asset.FileName == assetFileName)
                || !duplicatedAsset.IsVisible)
            {
                continue;
            }

            assetsToDelete.Add(duplicatedAsset);
        }

        return [.. assetsToDelete];
    }

    public DuplicatedAssetViewModel[] GetNotExemptedDuplicatedAssets(string exemptedFolderPath)
    {
        if (DuplicatedAssetSets.Length == 0)
        {
            return [];
        }

        // A duplicate set already groups true duplicates whatever the hash mode (exact hash or PHash), so we reason
        // per set instead of cross-matching hashes: if a set holds an asset from the exempted folder, every other
        // visible member of that same set is a duplicate to delete. This is a single linear pass over all assets
        // (O(total)) instead of the previous O(filtered x exempted) scan, and it stays correct under PHash where set
        // members are within a Hamming threshold rather than sharing the exact same Hash.
        List<DuplicatedAssetViewModel> assetsToDelete = [];

        for (int i = 0; i < DuplicatedAssetSets.Length; i++)
        {
            DuplicatedSetViewModel duplicatedAssetSet = DuplicatedAssetSets[i];

            bool hasExemptedAsset = false;

            for (int j = 0; j < duplicatedAssetSet.Count; j++)
            {
                if (duplicatedAssetSet[j].Asset.Folder.Path == exemptedFolderPath)
                {
                    hasExemptedAsset = true;
                    break;
                }
            }

            if (!hasExemptedAsset)
            {
                continue;
            }

            for (int j = 0; j < duplicatedAssetSet.Count; j++)
            {
                DuplicatedAssetViewModel duplicatedAsset = duplicatedAssetSet[j];

                if (duplicatedAsset.IsVisible && duplicatedAsset.Asset.Folder.Path != exemptedFolderPath)
                {
                    assetsToDelete.Add(duplicatedAsset);
                }
            }
        }

        return [.. assetsToDelete];
    }

    public void CollapseAssets(DuplicatedAssetViewModel[] duplicatedAssets)
    {
        // Collapsing an asset only flips its own IsVisible (the asset's setter does not notify its set). The set's
        // DuplicatesCount/IsVisible aggregates are recomputed here once per distinct affected set, instead of once
        // per collapsed asset: each NotifyAssetChanged re-runs two LINQ Count scans, so notifying per asset made a
        // large "delete all but this" O(deleted); notifying per set makes it O(affected sets) for an identical
        // final state.
        HashSet<DuplicatedSetViewModel> affectedSets = [];

        for (int i = 0; i < duplicatedAssets.Length; i++)
        {
            DuplicatedAssetViewModel duplicatedAsset = duplicatedAssets[i];

            duplicatedAsset.IsVisible = false;
            affectedSets.Add(duplicatedAsset.ParentViewModel);
        }

        foreach (DuplicatedSetViewModel affectedSet in affectedSets)
        {
            affectedSet.NotifyAssetChanged();
        }

        // We want to navigate to another set only when we are collapsing the last duplicate of the current set
        if (CurrentDuplicatedAssetSet.IsVisible)
        {
            if (!CurrentDuplicatedAsset!.IsVisible)
            {
                ResetDuplicatedAssetPosition();
            }
        }
        else
        {
            NavigateToNextVisibleSet(DuplicatedAssetSetsPosition);
        }
    }

    private void ResetDuplicatedAssetPosition()
    {
        if (DuplicatedAssetSets.Length == 0 || !CurrentDuplicatedAssetSet.IsVisible)
        {
            DuplicatedAssetPosition = 0;

            return;
        }

        for (int i = 0; i < CurrentDuplicatedAssetSet.Count; i++)
        {
            if (!CurrentDuplicatedAssetSet[i].IsVisible)
            {
                continue;
            }

            DuplicatedAssetPosition = i;

            return;
        }
    }

    private int NormalizeDuplicatedAssetSetsPosition(int position)
    {
        if (DuplicatedAssetSets.Length == 0)
        {
            return 0;
        }

        if (position >= 0 && position < DuplicatedAssetSets.Length)
        {
            return position;
        }

        return Math.Clamp(DuplicatedAssetSetsPosition, 0, DuplicatedAssetSets.Length - 1);
    }

    private int NormalizeDuplicatedAssetPosition(int position)
    {
        DuplicatedSetViewModel currentDuplicatedAssetSet = CurrentDuplicatedAssetSet;

        if (currentDuplicatedAssetSet.Count == 0)
        {
            return 0;
        }

        if (position >= 0 && position < currentDuplicatedAssetSet.Count)
        {
            return position;
        }

        return Math.Clamp(DuplicatedAssetPosition, 0, currentDuplicatedAssetSet.Count - 1);
    }

    private void NavigateToNextVisibleSet(int currentIndex)
    {
        for (int i = currentIndex; i < DuplicatedAssetSets.Length; i++)
        {
            if (DuplicatedAssetSets[i].IsVisible)
            {
                DuplicatedAssetSetsPosition = i;
                return;
            }
        }

        // No visible set found after or at currentIndex, fallback
        NavigateToPreviousVisibleSet(currentIndex);
    }

    private void NavigateToPreviousVisibleSet(int currentIndex)
    {
        int duplicatedAssetSetsCount = DuplicatedAssetSets.Length;

        // If currentIndex is out of range, adjust it to the last valid index
        if (currentIndex >= duplicatedAssetSetsCount)
        {
            currentIndex = duplicatedAssetSetsCount - 1;
        }

        for (int i = currentIndex; i >= 0; i--)
        {
            if (DuplicatedAssetSets[i].IsVisible)
            {
                DuplicatedAssetSetsPosition = i;
                return;
            }
        }

        // If no visible set is found, reset the position to 0
        if (DuplicatedAssetSetsPosition != 0)
        {
            DuplicatedAssetSetsPosition = 0;
        }

        MessageBoxInformationSent?.Invoke(this, new()
        {
            Message = "All duplicates have been deleted. \nGood Job ;)",
            Caption = "Information"
        });
    }
}
