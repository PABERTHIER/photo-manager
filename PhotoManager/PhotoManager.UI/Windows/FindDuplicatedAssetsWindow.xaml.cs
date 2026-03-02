using Microsoft.Extensions.Logging;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for FindDuplicatedAssetsWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FindDuplicatedAssetsWindow
{
    private readonly ILogger<FindDuplicatedAssetsWindow> _logger;

    public FindDuplicatedAssetsWindow(FindDuplicatedAssetsViewModel viewModel,
        ILogger<FindDuplicatedAssetsWindow> logger)
    {
        _logger = logger;

        try
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.MessageBoxInformationSent += DisplayMessageBox;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
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
            DuplicatedAssetViewModel duplicatedAssetViewModel =
                (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;

            DeleteAssets([duplicatedAssetViewModel]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteAllLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel duplicatedAssetViewModel =
                (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            List<DuplicatedAssetViewModel> assetsToDelete =
                ViewModel.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

            DeleteAssets(assetsToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteAllNotExemptedLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;

            List<DuplicatedAssetViewModel>
                assetsToDelete = ViewModel.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            DeleteAssets(assetsToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
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
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void CopyToClipboard_OpenInFileExplorer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ViewModel.CurrentDuplicatedAsset == null)
            {
                _logger.LogError("No duplicated asset selected");
                return;
            }

            string assetPath = ViewModel.CurrentDuplicatedAsset.Asset.FullPath;
            Clipboard.SetText(assetPath);

            string args = $"/select, \"{assetPath}\"";
            Process.Start("explorer.exe", args); // TODO: Only for Windows
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void FindDuplicatedAssetsWindow_Closing(object sender, CancelEventArgs e)
    {
        RefreshAssetsCounter?.Invoke(this);
        // TODO: Event to refresh CalculateTotalFilesCount as well
    }

    private void DeleteAssets(List<DuplicatedAssetViewModel> assetsToDelete)
    {
        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(x => x.Asset)]);

        ViewModel.CollapseAssets(assetsToDelete);
    }

    private static void DisplayMessageBox(object sender, MessageBoxInformationSentEventArgs e)
    {
        MessageBox.Show(e.Message, e.Caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
