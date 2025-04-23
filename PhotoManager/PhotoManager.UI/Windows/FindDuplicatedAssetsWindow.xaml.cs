using log4net;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for FindDuplicatedAssetsWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FindDuplicatedAssetsWindow
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public FindDuplicatedAssetsWindow(FindDuplicatedAssetsViewModel viewModel)
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

    public event GetExemptedFolderPathEventHandler? GetExemptedFolderPath;
    public event DeleteDuplicatedAssetsEventHandler? DeleteDuplicatedAssets;
    public event RefreshAssetsCounterEventHandler? RefreshAssetsCounter;

    public FindDuplicatedAssetsViewModel ViewModel => (FindDuplicatedAssetsViewModel)DataContext;

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel duplicatedAssetViewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;

            DeleteAssets([duplicatedAssetViewModel]);
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
            List<DuplicatedAssetViewModel> assetsToDelete = ViewModel.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

            DeleteAssets(assetsToDelete);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void DeleteAllNotExemptedLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;

            List<DuplicatedAssetViewModel> assetsToDelete = ViewModel.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            DeleteAssets(assetsToDelete);
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

    private void CopyToClipboard_OpenInFileExplorer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ViewModel.CurrentDuplicatedAsset == null)
            {
                Log.Error("No duplicated asset selected");
                return;
            }

            string assetPath = ViewModel.CurrentDuplicatedAsset.Asset.FullPath;
            Clipboard.SetText(assetPath);

            string args = $"/select, \"{assetPath}\"";
            Process.Start("explorer.exe", args); // TODO: Only for Windows
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void FindDuplicatedAssetsWindow_Closing(object sender, CancelEventArgs e)
    {
        RefreshAssetsCounter?.Invoke(this);
    }

    private void DeleteAssets(List<DuplicatedAssetViewModel> assetsToDelete)
    {
        DeleteDuplicatedAssets?.Invoke(this, assetsToDelete.Select(x => x.Asset).ToArray());

        ViewModel.CollapseAssets(assetsToDelete);
    }

    private static void DisplayMessageBox(object sender, MessageBoxInformationSentEventArgs e)
    {
        MessageBox.Show(e.Message, e.Caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
