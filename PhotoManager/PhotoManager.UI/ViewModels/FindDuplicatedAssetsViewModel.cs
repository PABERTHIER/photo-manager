﻿using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.Models;
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

    public event MessageBoxInformationSentEventHandler? MessageBoxInformationSent;

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
            ResetDuplicatedAssetPosition();
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

    public void SetDuplicates(List<List<Asset>> assetsSets)
    {
        if (assetsSets == null)
        {
            throw new ArgumentNullException(nameof(assetsSets));
        }

        List<DuplicatedSetViewModel> duplicatedAssetSets = [];

        foreach (List<Asset> assets in assetsSets)
        {
            DuplicatedSetViewModel duplicatedSetViewModel = [];

            foreach (Asset asset in assets)
            {
                duplicatedSetViewModel.Add(new (Application) { Asset = asset, ParentViewModel = duplicatedSetViewModel });
            }

            duplicatedAssetSets.Add(duplicatedSetViewModel);
        }

        DuplicatedAssetSets = duplicatedAssetSets;
    }

    public void Refresh()
    {
        List<List<Asset>> duplicatedAssetsSets = Application.GetDuplicatedAssets();
        SetDuplicates(duplicatedAssetsSets);
    }

    public List<DuplicatedAssetViewModel> GetDuplicatedAssets(Asset currentAsset)
    {
        if (DuplicatedAssetSets.Count == 0)
        {
            return [];
        }

        string currentRootFolderPath = currentAsset.Folder.Path;
        IEnumerable<DuplicatedSetViewModel> duplicatedAssetByHashList = DuplicatedAssetSets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
        DuplicatedSetViewModel? duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

        DuplicatedAssetViewModel? duplicatedAsset = duplicatedAssetByHash?.FirstOrDefault(x =>
            x.Asset.Folder.Path == currentRootFolderPath && x.Asset.FileName == currentAsset.FileName);

        if (duplicatedAsset == null)
        {
            return [];
        }

        List<DuplicatedAssetViewModel> assetsToDelete = duplicatedAssetByHash?.Where(x => x != duplicatedAsset && x.Visible == Visibility.Visible).ToList() ?? [];

        return assetsToDelete;
    }
    public void CollapseAssets(List<DuplicatedAssetViewModel> duplicatedAssets)
    {
        if (duplicatedAssets.Count == 1)
        {
            duplicatedAssets[0].Visible = Visibility.Collapsed;

            // We want to navigate to another set only when we are collapsing the last duplicate of the current set
            if (CurrentDuplicatedAssetSet.Visible == Visibility.Visible)
            {
                if (CurrentDuplicatedAsset?.Visible == Visibility.Collapsed)
                {
                    ResetDuplicatedAssetPosition();
                }
            }
            else
            {
                NavigateToNextVisibleSet(DuplicatedAssetSetsPosition);
            }

            return;
        }

        for (int i = 0; i < duplicatedAssets.Count; i++)
        {
            duplicatedAssets[i].Visible = Visibility.Collapsed;
        }

        if (CurrentDuplicatedAssetSet.Visible != Visibility.Visible)
        {
            NavigateToNextVisibleSet(DuplicatedAssetSetsPosition);
        }
    }

    private void ResetDuplicatedAssetPosition()
    {
        if (DuplicatedAssetSets.Count == 0 || CurrentDuplicatedAssetSet.Visible != Visibility.Visible)
        {
            DuplicatedAssetPosition = 0;

            return;
        }

        for (int i = 0; i < CurrentDuplicatedAssetSet.Count; i++)
        {
            if (CurrentDuplicatedAssetSet[i].Visible != Visibility.Visible)
            {
                continue;
            }

            DuplicatedAssetPosition = i;

            return;
        }
    }

    private void NavigateToNextVisibleSet(int currentIndex)
    {
        for (int i = currentIndex; i < DuplicatedAssetSets.Count; i++)
        {
            if (DuplicatedAssetSets[i].Visible == Visibility.Visible)
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
        int duplicatedAssetSetsCount = DuplicatedAssetSets.Count;

        // If currentIndex is out of range, adjust it to the last valid index
        if (currentIndex >= duplicatedAssetSetsCount)
        {
            currentIndex = duplicatedAssetSetsCount - 1;
        }

        for (int i = currentIndex; i >= 0; i--)
        {
            if (DuplicatedAssetSets[i].Visible == Visibility.Visible)
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
