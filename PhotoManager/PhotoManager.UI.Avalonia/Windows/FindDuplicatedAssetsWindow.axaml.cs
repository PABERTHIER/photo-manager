using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.UI.Models;
using PhotoManager.UI.ViewModels;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Windows;

[ExcludeFromCodeCoverage]
public partial class FindDuplicatedAssetsWindow : Window
{
    private readonly ILogger<FindDuplicatedAssetsWindow> _logger;

    public FindDuplicatedAssetsWindow()
    {
        _logger = NullLogger<FindDuplicatedAssetsWindow>.Instance;
        InitializeComponent();
    }

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

    public FindDuplicatedAssetsViewModel ViewModel => (FindDuplicatedAssetsViewModel)DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (((Control)sender!).DataContext is DuplicatedAssetViewModel duplicatedAssetViewModel)
            {
                DeleteAssets([duplicatedAssetViewModel]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteAllButThisButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (((Control)sender!).DataContext is DuplicatedAssetViewModel duplicatedAssetViewModel)
            {
                List<DuplicatedAssetViewModel> assetsToDelete =
                    ViewModel.GetDuplicatedAssets(duplicatedAssetViewModel.Asset);

                DeleteAssets(assetsToDelete);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteAllNotExemptedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            string exemptedFolderPath = GetExemptedFolderPath?.Invoke(this) ?? string.Empty;
            List<DuplicatedAssetViewModel> assetsToDelete =
                ViewModel.GetNotExemptedDuplicatedAssets(exemptedFolderPath);

            DeleteAssets(assetsToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void RefreshButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

    private void CopyToClipboardOpenInFileExplorerButton_Click(object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = CopyToClipboardOpenInFileExplorerAsync();
    }

    private async Task CopyToClipboardOpenInFileExplorerAsync()
    {
        try
        {
            if (ViewModel.CurrentDuplicatedAsset == null)
            {
                _logger.LogError("No duplicated asset selected");
                return;
            }

            string assetPath = ViewModel.CurrentDuplicatedAsset.Asset.FullPath;
            IClipboard? clipboard = GetTopLevel(this)?.Clipboard;

            if (clipboard != null)
            {
                await clipboard.SetTextAsync(assetPath);
            }

            OpenFileManager(assetPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void FindDuplicatedAssetsWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        RefreshAssetsCounter?.Invoke(this);
    }

    private void DeleteAssets(List<DuplicatedAssetViewModel> assetsToDelete)
    {
        DeleteDuplicatedAssets?.Invoke(this, [.. assetsToDelete.Select(static asset => asset.Asset)]);
        ViewModel.CollapseAssets(assetsToDelete);
    }

    private void DisplayMessageBox(object sender, MessageBoxInformationSentEventArgs e)
    {
        _ = DisplayMessageBoxAsync(e);
    }

    private async Task DisplayMessageBoxAsync(MessageBoxInformationSentEventArgs e)
    {
        try
        {
            await MessageDialogWindow.ShowAsync(this, e.Message, e.Caption);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private static void OpenFileManager(string assetPath)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer.exe", $"/select, \"{assetPath}\"");
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"-R \"{assetPath}\"");
            return;
        }

        string? directoryPath = Path.GetDirectoryName(assetPath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Process.Start("xdg-open", directoryPath);
        }
    }
}
