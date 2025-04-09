using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Models;
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
public partial class DuplicatedAssetsWindow
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // TODO: Rename this class
    public DuplicatedAssetsWindow(FindDuplicatedAssetsViewModel viewModel)
    {
        try
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.MessageBoxInformationSent += DisplayMessageBox;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;
    public event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
    public event DeleteDuplicateAssetsEventHandler? DeleteDuplicateAssets;

    public FindDuplicatedAssetsViewModel ViewModel => (FindDuplicatedAssetsViewModel)DataContext;

    private List<DuplicatedSetViewModel> DuplicatedAssets => ViewModel.DuplicatedAssetSets; // TODO: Rename to DuplicatedAssetSets

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel duplicatedAssetViewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            DeleteDuplicateAssets?.Invoke(this, [duplicatedAssetViewModel.Asset]);

            ViewModel.CollapseAssets([duplicatedAssetViewModel]);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void DeleteAllLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel duplicatedAssetViewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            DeleteAllDuplicatedAssetsByHash(DuplicatedAssets, duplicatedAssetViewModel.Asset);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
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
            Log.Error(ex);
        }
    }

    private void DeleteEveryDuplicatesLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DeleteEveryDuplicatedAssets(DuplicatedAssets);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void CopyToClipboard_OpenInFileExplorer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var assetPath = ViewModel.CurrentDuplicatedAsset.Asset.FullPath;
            Clipboard.SetText(assetPath);

            string args = $"/select, \"{assetPath}\"";
            Process.Start("explorer.exe", args); // TODO: Only for Windows
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void FindDuplicatesAssetsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        RefreshAssetsCounter?.Invoke(this);
    }

    // TODO: Move the logic to services for both methods (no logic here !)
    private void DeleteAllDuplicatedAssetsByHash(List<DuplicatedSetViewModel> duplicatedAssets, Asset currentAsset)
    {
        if (duplicatedAssets == null || currentAsset == null)
        {
            Log.Error("duplicatedAssets or currentAsset is null");
            return;
        }

        var currentRootFolderPath = currentAsset.Folder.Path;
        var duplicatedAssetByHashList = duplicatedAssets.Where(x => x.Any(y => y.Asset.Hash == currentAsset.Hash));
        var duplicatedAssetByHash = duplicatedAssetByHashList.FirstOrDefault();

        var assetSelected = duplicatedAssetByHash?.FirstOrDefault(x => x?.Asset.Folder.Path == currentRootFolderPath && x?.Asset.FileName == currentAsset.FileName);

        if (assetSelected == null)
        {
            Log.Error("firstAsset is null");
            return;
        }

        var assetsToDelete = duplicatedAssetByHash?
            .Where(x => x != null && x != assetSelected)
            .ToList() 
            ?? new List<DuplicatedAssetViewModel>();

        DeleteDuplicateAssets?.Invoke(this, assetsToDelete.Select(x => x.Asset).ToArray());

        ViewModel.CollapseAssets(assetsToDelete);
    }

    private void DeleteEveryDuplicatedAssets(List<DuplicatedSetViewModel> duplicatedAssets)
    {
        if (duplicatedAssets == null)
        {
            Log.Error("duplicatedAssets is null");
            return;
        }

        string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;

        var exemptedAssets = DuplicatedAssets.Where(x => x != null).SelectMany(x => x).Where(y => y != null && y.Asset.Folder.Path == exemptedFolderPath).ToList();

        var duplicatedAssetsFiltered = DuplicatedAssets
            .Where(x => x != null)
            .SelectMany(x => x)
            .Where(y => y != null && y.Asset.Folder.Path != exemptedFolderPath)
            .ToList();

        var assetsToDelete = duplicatedAssetsFiltered.Join(exemptedAssets,
            x => x.Asset.Hash,
            y => y.Asset.Hash,
            (x, y) => x)
            .ToList();

        DeleteDuplicateAssets?.Invoke(this, assetsToDelete.Select(x => x.Asset).ToArray());

        ViewModel.CollapseAssets(assetsToDelete);
    }

    private static void DisplayMessageBox(object sender, MessageBoxInformationSentEventArgs e)
    {
        MessageBox.Show(e.Message, e.Caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
