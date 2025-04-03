using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    // TODO: Rename this class
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

    public MainWindow MainWindowInstance { get; set; }

    private List<DuplicatedSetViewModel> DuplicatedAssets => ViewModel.DuplicatedAssetSets; // TODO: Rename to DuplicatedAssetSets

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel viewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            Asset asset = viewModel.Asset;
            ViewModel.DeleteAsset(viewModel, MainWindowInstance);
            ViewModel.Refresh();

            Console.WriteLine("Delete " + asset.FullPath);
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

    private void CopyToClipboard_OpenInFileExplorer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var assetPath = ViewModel.CurrentDuplicatedAsset.Asset.FullPath;
            Clipboard.SetText(assetPath);

            string args = $"/select, \"{assetPath}\"";
            Process.Start("explorer.exe", args);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void FindDuplicatesAssetsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        MainWindowInstance.RefreshAssetsCounter();
    }

    // TODO: Move the logic to services for both methods (no logic here !)
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

        ViewModel.DeleteAssets(assetsToDelete, MainWindowInstance);
    }

    private void DeleteEveryDuplicatedAssets(List<DuplicatedSetViewModel> duplicatedAssets)
    {
        if (duplicatedAssets == null)
        {
            log.Error("duplicatedAssets is null");
            return;
        }

        var exemptedAssets = DuplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path == MainWindowInstance.GetExemptedFolderPath()).ToList();

        var duplicatedAssetsFiltered = DuplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path != MainWindowInstance.GetExemptedFolderPath()).ToList();

        var assetsToDelete = duplicatedAssetsFiltered.Join(exemptedAssets,
            x => x.Asset.Hash,
            y => y.Asset.Hash,
            (x, y) => x)
            .ToList();

        ViewModel.DeleteAssets(assetsToDelete, MainWindowInstance);
    }
}
