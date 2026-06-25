using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Controls;

/// <summary>
/// User control that displays the grid of asset thumbnails.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ThumbnailsUserControl : UserControl
{
    private readonly ILogger<ThumbnailsUserControl> _logger;

    public ThumbnailsUserControl()
    {
        _logger = App.ServiceProvider?.GetService<ILogger<ThumbnailsUserControl>>()
                  ?? NullLogger<ThumbnailsUserControl>.Instance;
        InitializeComponent();
    }

    public event EventHandler? ThumbnailSelected;

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext!;

    public async Task GoToFolderAsync(IApplication application, string selectedImagePath)
    {
        try
        {
            if (ViewModel.IsRefreshingFolders)
            {
                return;
            }

            Asset[] assets = await Task.Run(() => application.GetAssetsByPath(selectedImagePath));
            ViewModel.SetAssets(selectedImagePath, assets);

            if (ThumbnailsListBox.Items.Count > 0 && ThumbnailsListBox.Items[0] != null)
            {
                ViewModel.SetViewerPosition(0);
                ThumbnailsListBox.ScrollIntoView(ThumbnailsListBox.Items[0]!);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    public void ScrollSelectedThumbnailIntoView()
    {
        if (ThumbnailsListBox.Items.Count > 0 && ThumbnailsListBox.SelectedItem != null)
        {
            ThumbnailsListBox.ScrollIntoView(ThumbnailsListBox.SelectedItem);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        ThumbnailsListBox = this.FindControl<ListBox>(nameof(ThumbnailsListBox))
            ?? throw new InvalidOperationException("ThumbnailsListBox control was not found.");
    }

    private void Thumbnail_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            SetViewerPositionFromSender(sender);

            if (e.ClickCount == 2)
            {
                ThumbnailSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void ThumbnailsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ApplicationViewModel viewModel)
        {
            return;
        }

        if (ThumbnailsListBox.SelectedIndex >= 0)
        {
            viewModel.SetViewerPosition(ThumbnailsListBox.SelectedIndex);
        }

        viewModel.SetSelectedAssets(ThumbnailsListBox.SelectedItems?.OfType<Asset>().ToArray() ?? []);
    }

    private void SetViewerPositionFromSender(object? sender)
    {
        if (sender is not Control { DataContext: Asset asset } || DataContext is not ApplicationViewModel viewModel)
        {
            return;
        }

        int assetIndex = viewModel.ObservableAssets.IndexOf(asset);

        if (assetIndex >= 0)
        {
            viewModel.SetViewerPosition(assetIndex);
        }
    }
}
