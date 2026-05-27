using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.Controls;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.ViewModels.Enums;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Windows;

[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IApplication _application;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _backgroundWorkTask = Task.CompletedTask;
    private Task _catalogTask = Task.CompletedTask;

    public MainWindow() : this(
        App.ServiceProvider?.GetRequiredService<ApplicationViewModel>()
        ?? throw new InvalidOperationException("ApplicationViewModel service is not available."),
        App.ServiceProvider.GetRequiredService<IApplication>()
        ?? throw new InvalidOperationException("Application service is not available."),
        App.ServiceProvider.GetRequiredService<ILoggerFactory>())
    {
    }

    public MainWindow(ApplicationViewModel viewModel, IApplication application, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MainWindow>();
        _application = application;
        _cancellationTokenSource = new();

        try
        {
            InitializeComponent();
            DataContext = viewModel;

            FolderNavigationViewModel folderNavigationViewModel = new(
                ViewModel,
                new() { Id = Guid.NewGuid(), Path = ViewModel.CurrentFolderPath },
                application.GetRecentTargetPaths());
            FolderTreeView.DataContext = folderNavigationViewModel;
            FolderTreeView.SelectedPath = folderNavigationViewModel.SourceFolder.Path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        FolderTreeView = this.FindControl<FolderNavigationControl>(nameof(FolderTreeView))
            ?? throw new InvalidOperationException("FolderTreeView control was not found.");
        ViewerUserControl = this.FindControl<ViewerUserControl>(nameof(ViewerUserControl))
            ?? throw new InvalidOperationException("ViewerUserControl control was not found.");
        ThumbnailsUserControl = this.FindControl<ThumbnailsUserControl>(nameof(ThumbnailsUserControl))
            ?? throw new InvalidOperationException("ThumbnailsUserControl control was not found.");
    }

    private void Window_Opened(object? sender, EventArgs e)
    {
        try
        {
            _backgroundWorkTask = StartBackgroundWorkAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    // TODO: Add a help page to describe all shortcuts available
    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                switch (e.Key)
                {
                    case Key.C:
                        MoveAssets(preserveOriginalFiles: true);
                        break;

                    case Key.M:
                        MoveAssets(preserveOriginalFiles: false);
                        break;
                    // ReSharper disable once RedundantEmptySwitchSection
                    default:
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        DeleteSelectedAssets();
                        break;

                    case Key.PageUp:
                    case Key.Left:
                        ViewModel.GoToPreviousAsset();
                        ShowImage();
                        break;

                    case Key.PageDown:
                    case Key.Right:
                        ViewModel.GoToNextAsset();
                        ShowImage();
                        break;

                    case Key.F1:
                        ViewModel.ChangeAppMode();
                        ShowImage();
                        break;
                    // ReSharper disable once RedundantEmptySwitchSection
                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void ToggleImageView(object? sender, EventArgs e)
    {
        try
        {
            ViewModel.ChangeAppMode();
            ShowImage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void FolderTreeView_FolderSelected(object? sender, EventArgs e)
    {
        _ = FolderTreeViewFolderSelectedAsync();
    }

    private async Task FolderTreeViewFolderSelectedAsync()
    {
        try
        {
            await ThumbnailsUserControl.GoToFolderAsync(_application, FolderTreeView.SelectedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void FindDuplicates_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = FindDuplicatesAsync();
    }

    private async Task FindDuplicatesAsync()
    {
        try
        {
            List<List<Asset>> assetsSets = _application.GetDuplicatedAssets();

            if (assetsSets.Count > 0)
            {
                FindDuplicatedAssetsViewModel findDuplicatedAssetsViewModel = new(_application);
                findDuplicatedAssetsViewModel.SetDuplicates(assetsSets);
                FindDuplicatedAssetsWindow findDuplicatedAssetsWindow = new(findDuplicatedAssetsViewModel,
                    _loggerFactory.CreateLogger<FindDuplicatedAssetsWindow>());

                findDuplicatedAssetsWindow.GetExemptedFolderPath += GetExemptedFolderPath;
                findDuplicatedAssetsWindow.DeleteDuplicatedAssets += DeleteDuplicatedAssets;
                findDuplicatedAssetsWindow.RefreshAssetsCounter += RefreshAssetsCounter;

                await findDuplicatedAssetsWindow.ShowDialog(this);
            }
            else
            {
                await MessageDialogWindow.ShowAsync(this, "No duplicates have been found.", "Information");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void SyncAssets_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = SyncAssetsAsync();
    }

    private async Task SyncAssetsAsync()
    {
        try
        {
            SyncAssetsViewModel syncAssetsViewModel = new(_application);
            SyncAssetsWindow syncAssetsWindow = new(syncAssetsViewModel,
                _loggerFactory.CreateLogger<SyncAssetsWindow>());
            await syncAssetsWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void About_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _ = AboutAsync();
    }

    private async Task AboutAsync()
    {
        try
        {
            AboutWindow aboutWindow = new(ViewModel.AboutInformation, _loggerFactory.CreateLogger<AboutWindow>());
            await aboutWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void CopyAssets_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        MoveAssets(preserveOriginalFiles: true);
    }

    private void MoveAssets_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        MoveAssets(preserveOriginalFiles: false);
    }

    private void DeleteDuplicatedAssets(object sender, Asset[] assets)
    {
        try
        {
            if (assets.Length > 0)
            {
                DeleteAssets(assets);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void RefreshAssetsCounter(object sender)
    {
        ViewModel.CalculateGlobalAssetsCounter();
        ViewModel.CalculateTotalFilesCount();
    }

    private string GetExemptedFolderPath(object sender)
    {
        return ViewModel.GetExemptedFolderPath();
    }

    private void DeleteAssets_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeleteSelectedAssets();
    }

    private void Exit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void SortAssetsByFileName_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileName);
    }

    private void SortAssetsByFileSize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileSize);
    }

    private void SortAssetsByFileCreationDateTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileCreationDateTime);
    }

    private void SortAssetsByFileModificationDateTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileModificationDateTime);
    }

    private void SortAssetsByThumbnailCreationDateTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.ThumbnailCreationDateTime);
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        _cancellationTokenSource.Cancel();

        _ = _backgroundWorkTask.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                _logger.LogError(task.Exception, "BackgroundWorkTask faulted during shutdown");
            }
        }, TaskScheduler.Default);

        _backgroundWorkTask = Task.CompletedTask;
    }

    private async Task StartBackgroundWorkAsync()
    {
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ViewModel.SetStatusMessage($"Cataloging thumbnails for {ViewModel.CurrentFolderPath}");

            await InitializeOnceAsync(stopwatch);

            if (ViewModel.GetSyncAssetsEveryXMinutes())
            {
                ushort minutes = ViewModel.GetCatalogCooldownMinutes();
                TimeSpan delay = TimeSpan.FromMinutes(minutes);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    await InitializeOnceAsync(stopwatch);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error in background work");
        }
    }

    private async Task InitializeOnceAsync(Stopwatch stopwatch)
    {
        _catalogTask = ViewModel.CatalogAssets(
            NotifyCatalogChangeOnUiThread,
            _cancellationTokenSource.Token);

        try
        {
            await _catalogTask;
        }
        catch (OperationCanceledException)
        {
            // Expected: the user requested cancellation.
        }

        ViewModel.CalculateGlobalAssetsCounter();
        stopwatch.Stop();
        ViewModel.SetExecutionTime(stopwatch.Elapsed);
        ViewModel.CalculateTotalFilesCount();
    }

    private void NotifyCatalogChangeOnUiThread(CatalogChangeCallbackEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ViewModel.NotifyCatalogChange(e);
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() => ViewModel.NotifyCatalogChange(e)).GetAwaiter().GetResult();
    }

    private void ShowImage()
    {
        if (ViewModel.AppMode == AppMode.Viewer)
        {
            ViewerUserControl.ShowImage();
        }
        else
        {
            ThumbnailsUserControl.ScrollSelectedThumbnailIntoView();
        }
    }

    private void DeleteSelectedAssets()
    {
        try
        {
            Asset[] selectedAssets = ViewModel.SelectedAssets;

            if (selectedAssets.Length > 0)
            {
                DeleteAssets(selectedAssets);
                ShowImage();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void DeleteAssets(Asset[] assets)
    {
        _application.DeleteAssets(assets);
        ViewModel.RemoveAssets(assets);
    }

    private void MoveAssets(bool preserveOriginalFiles)
    {
        try
        {
            Asset[] assets = ViewModel.SelectedAssets;

            if (assets.Length == 0)
            {
                return;
            }

            FolderNavigationWindow folderNavigationWindow = new(
                new(ViewModel, assets[0].Folder, _application.GetRecentTargetPaths()),
                _loggerFactory.CreateLogger<FolderNavigationWindow>());

            folderNavigationWindow.Closed += (_, _) =>
            {
                if (folderNavigationWindow.ViewModel is { SelectedFolder: not null, HasConfirmed: true })
                {
                    bool result = _application.MoveAssets(assets, folderNavigationWindow.ViewModel.SelectedFolder,
                        preserveOriginalFiles);

                    if (result)
                    {
                        ViewModel.SetMoveAssetsLastSelectedFolder(folderNavigationWindow.ViewModel.SelectedFolder);
                        ViewModel.SetIsRefreshingFolders(true);
                        FolderTreeView.SelectedPath = folderNavigationWindow.ViewModel.SelectedFolder.Path;
                        FolderTreeView.Initialize();
                        ViewModel.SetIsRefreshingFolders(false);

                        if (!preserveOriginalFiles)
                        {
                            ViewModel.RemoveAssets(assets);

                            if (ViewModel.AppMode == AppMode.Viewer)
                            {
                                ViewerUserControl.ShowImage();
                            }
                        }
                    }
                }
            };

            folderNavigationWindow.Show(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }
}
