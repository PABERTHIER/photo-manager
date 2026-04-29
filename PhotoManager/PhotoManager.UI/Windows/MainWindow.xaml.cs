using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.ViewModels.Enums;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow
{
    private readonly ILogger<MainWindow> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly IApplication _application;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _backgroundWorkTask = new(() => { });
    private Task _catalogTask = new(() => { });

    public MainWindow(ApplicationViewModel viewModel, IApplication application, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MainWindow>();

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

        _application = application;
        _cancellationTokenSource = new();
    }

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

    private void Window_Loaded(object sender, RoutedEventArgs e)
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
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                // To trigger these events below, need to hold the Ctrl key and press C or M in the meantime
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

    private void ToggleImageView(object sender, EventArgs e)
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

    private void FolderTreeView_FolderSelected(object sender, EventArgs e)
    {
        try
        {
            ThumbnailsUserControl.GoToFolder(_application, FolderTreeView.SelectedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void FindDuplicates_Click(object sender, RoutedEventArgs e)
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

                findDuplicatedAssetsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("No duplicates have been found.", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void SyncAssets_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SyncAssetsViewModel syncAssetsViewModel = new(_application);
            SyncAssetsWindow syncAssetsWindow = new(syncAssetsViewModel,
                _loggerFactory.CreateLogger<SyncAssetsWindow>());
            syncAssetsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AboutWindow aboutWindow = new(ViewModel.AboutInformation, _loggerFactory.CreateLogger<AboutWindow>());
            aboutWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void CopyAssets_Click(object sender, RoutedEventArgs e)
    {
        MoveAssets(preserveOriginalFiles: true);
    }

    private void MoveAssets_Click(object sender, RoutedEventArgs e)
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
    }

    private string GetExemptedFolderPath(object sender) => ViewModel.GetExemptedFolderPath();

    private void DeleteAssets_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedAssets();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SortAssetsByFileName_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileName);
    }

    private void SortAssetsByFileSize_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileSize);
    }

    private void SortAssetsByFileCreationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileCreationDateTime);
    }

    private void SortAssetsByFileModificationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.FileModificationDateTime);
    }

    private void SortAssetsByThumbnailCreationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteria.ThumbnailCreationDateTime);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
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
            ViewModel.StatusMessage = $"Cataloging thumbnails for {ViewModel.CurrentFolderPath}";

            // The calling thread cannot access this object because a different thread owns it
            await InitializeOnceAsync(stopwatch)
                .ConfigureAwait(true); // Due to the WPF context, need to set it true to prevent thread exceptions

            if (ViewModel.GetSyncAssetsEveryXMinutes()) // Disabling infinite loop to prevent reduced perf
            {
                ushort minutes = ViewModel.GetCatalogCooldownMinutes();
                TimeSpan delay = TimeSpan.FromMinutes(minutes);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(delay, _cancellationTokenSource.Token)
                            .ConfigureAwait(
                                true); // Due to the WPF context, need to set it true to prevent thread exceptions
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    await InitializeOnceAsync(stopwatch)
                        .ConfigureAwait(
                            true); // Due to the WPF context, need to set it true to prevent thread exceptions
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
            e =>
            {
                // The InvokeAsync method is used to avoid freezing the application when the task is cancelled + to keep updating the UI
                // To prevent this issue : The calling thread cannot access this object because a different thread owns it
                Dispatcher.InvokeAsync(() => ViewModel.NotifyCatalogChange(e));
            },
            _cancellationTokenSource.Token
        );

        try
        {
            await _catalogTask
                .ConfigureAwait(true); // Due to the WPF context, need to set it true to prevent thread exceptions
        }
        catch (OperationCanceledException)
        {
            // Expected: the user requested cancellation (window closing). The service has already
            // saved any in-flight catalog state and emitted CatalogProcessCancelled/CatalogProcessEnded.
        }

        ViewModel.CalculateGlobalAssetsCounter();
        stopwatch.Stop();
        ViewModel.SetExecutionTime(stopwatch.Elapsed);
        ViewModel.CalculateTotalFilesCount();
    }

    private void ShowImage()
    {
        if (ViewModel.AppMode == AppMode.Viewer)
        {
            ViewerUserControl.ShowImage();
        }
        else
        {
            ThumbnailsUserControl.ShowImage();
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
        _application.DeleteAssets(assets); // TODO: Need to rework how the deletion is handled
        ViewModel.RemoveAssets(assets);
    }

    private void MoveAssets(bool preserveOriginalFiles)
    {
        try
        {
            Asset[] assets = ViewModel.SelectedAssets;

            if (assets.Length > 0)
            {
                FolderNavigationWindow folderNavigationWindow = new(
                    new(
                        ViewModel,
                        assets[0].Folder,
                        _application.GetRecentTargetPaths()),
                    _loggerFactory.CreateLogger<FolderNavigationWindow>());

                folderNavigationWindow.Closed += (_, _) =>
                {
                    if (folderNavigationWindow.ViewModel is { SelectedFolder: not null, HasConfirmed: true })
                    {
                        bool result = _application.MoveAssets(
                            assets,
                            folderNavigationWindow.ViewModel.SelectedFolder,
                            preserveOriginalFiles);

                        if (result)
                        {
                            ViewModel.MoveAssetsLastSelectedFolder = folderNavigationWindow.ViewModel.SelectedFolder;
                            ViewModel.IsRefreshingFolders = true;
                            FolderTreeView.SelectedPath = folderNavigationWindow.ViewModel.SelectedFolder.Path;
                            FolderTreeView.Initialize();
                            ViewModel.IsRefreshingFolders = false;

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

                folderNavigationWindow.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }
}
