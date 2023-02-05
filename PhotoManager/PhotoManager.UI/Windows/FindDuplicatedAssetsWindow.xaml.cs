using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for DuplicatedAssetsWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class DuplicatedAssetsWindow : Window
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DuplicatedAssetsWindow(FindDuplicatedAssetsViewModel viewModel)
    {
        try
        {
            InitializeComponent();

            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public FindDuplicatedAssetsViewModel ViewModel
    {
        get { return (FindDuplicatedAssetsViewModel)DataContext; }
    }

    private List<DuplicatedSetViewModel> DuplicatedAssets
    {
        get { return ViewModel.GetAllDuplicatedAssets(); }
    }

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel viewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            Asset asset = viewModel.Asset;
            ViewModel.DeleteAsset(viewModel);

            // TODO: IN THE LIST BOXES, IF THE FILENAME INCLUDES _ IT IS NOT BEING SHOWN.
            Console.WriteLine("Delete " + asset.FullPath);
            // TODO: IF THE USER IS VIEWING THE FOLDER IN WHICH THE DUPLICATE WAS STORED, THE THUMBNAILS LIST WON'T REFRESH
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void DeleteAllLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel viewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            DeleteAllDuplicatedAssetsByHash(DuplicatedAssets, viewModel.Asset);
            ViewModel.Refresh();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void DeleteEveryDuplicatesLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DeleteEveryDuplicatedAssets(DuplicatedAssets);
            ViewModel.Refresh();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void RefreshLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            ViewModel.Refresh();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void DeleteAllDuplicatedAssetsByHash(List<DuplicatedSetViewModel> duplicatedAssets, Asset currentAsset)
    {
        if (duplicatedAssets == null || currentAsset == null)
        {
            log.Error("duplicatedAssets or currentAsset is null");
            return;
        }

        var currentRootFolderPath = currentAsset.Folder.Path;
        var duplicatedAssetByHashList = duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
        var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

        var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

        if (assetSelected == null)
        {
            log.Error("firstAsset is null");
            return;
        }

        var assetsToDelete = duplicatedAssetByHash?.Where(x => x != null && x != assetSelected).ToList() ?? new List<DuplicatedAssetViewModel>();

        foreach (var assetToDelete in assetsToDelete)
        {
            ViewModel.DeleteAsset(assetToDelete);
        }
    }

    private void DeleteEveryDuplicatedAssets(List<DuplicatedSetViewModel> duplicatedAssets)
    {
        if (duplicatedAssets == null)
        {
            log.Error("duplicatedAssets is null");
            return;
        }

        var exemptedAssets = DuplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path == Constants.PathLocationToExemptTheFolder).ToList();

        var duplicatedAssetsFiltered = DuplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path != Constants.PathLocationToExemptTheFolder).ToList();

        foreach (var assetToDelete in duplicatedAssetsFiltered)
        {
            if (exemptedAssets.Any(x => x.Asset.Hash == assetToDelete.Asset.Hash))
            {
                ViewModel.DeleteAsset(assetToDelete);
            }
        }
    }
}
