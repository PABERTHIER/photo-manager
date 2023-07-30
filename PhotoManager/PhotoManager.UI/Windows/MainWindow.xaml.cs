using log4net;
using PhotoManager.Application;
using PhotoManager.Constants;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

public class ThumbnailSelectedEventArgs : EventArgs
{
    public Asset Asset { get; set; }
}

public delegate void ThumbnailSelectedEventHandler(object sender, ThumbnailSelectedEventArgs e);

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IApplication _application;
    private readonly CancellationTokenSource _cts;
    Task catalogTask;

    public static MainWindow Current { get; private set; }

    public MainWindow(ApplicationViewModel viewModel, IApplication application)
    {
        try
        {
            InitializeComponent();
            Current = this;
            _cts = new CancellationTokenSource();

            _application = application;
            var aboutInformation = application.GetAboutInformation(GetType().Assembly);
            viewModel.Product = aboutInformation.Product;
            viewModel.Version = aboutInformation.Version;
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public ApplicationViewModel ViewModel
    {
        get { return (ApplicationViewModel)DataContext; }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.ChangeAppMode(AppModeEnum.Thumbnails);
            thumbnailsUserControl.GoToFolder(_application, ViewModel?.CurrentFolder);
            folderTreeView.SelectedPath = ViewModel?.CurrentFolder;
            await DoBackgroundWork();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
        finally
        {
            ViewModel.StatusMessage = "";
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.C:
                        MoveAssets(preserveOriginalFiles: true);
                        break;

                    case Key.M:
                        MoveAssets(preserveOriginalFiles: false);
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        DeleteAssets();
                        break;

                    case Key.PageUp:
                    case Key.Left:
                        ViewModel?.GoToPreviousAsset();
                        ShowImage();
                        break;

                    case Key.PageDown:
                    case Key.Right:
                        ViewModel?.GoToNextAsset();
                        ShowImage();
                        break;

                    case Key.F1:
                        ViewModel?.ChangeAppMode();
                        ShowImage();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void ShowImage()
    {
        if (ViewModel.AppMode == AppModeEnum.Viewer)
        {
            viewerUserControl.ShowImage();
        }
        else
        {
            thumbnailsUserControl.ShowImage();
        }
    }

    private void ThumbnailsUserControl_ThumbnailSelected(object sender, ThumbnailSelectedEventArgs e)
    {
        try
        {
            ViewModel?.GoToAsset(e.Asset, AppModeEnum.Viewer);
            ShowImage();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void ViewerUserControl_ThumbnailSelected(object sender, ThumbnailSelectedEventArgs e)
    {
        try
        {
            ViewModel?.GoToAsset(e.Asset, AppModeEnum.Thumbnails);
            ShowImage();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void FolderTreeView_FolderSelected(object sender, EventArgs e)
    {
        try
        {
            thumbnailsUserControl.GoToFolder(_application, folderTreeView.SelectedPath);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperCenter_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Center);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperFill_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Fill);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperFit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Fit);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperSpan_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Span);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperStretch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Stretch);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SetAsWallpaperTile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _application.SetAsWallpaper(ViewModel?.CurrentAsset, WallpaperStyle.Tile);
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void FindDuplicates_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var duplicates = _application.GetDuplicatedAssets();

            if (duplicates.Count > 0)
            {
                FindDuplicatedAssetsViewModel viewModel = new(_application);
                viewModel.SetDuplicates(duplicates);
                DuplicatedAssetsWindow duplicatedAssetsWindow = new(viewModel) // DuplicatedAssetsWindow is FindDuplicatedAssetsWindow.xaml
                {
                    MainWindowInstance = Current
                };
                duplicatedAssetsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("No duplicates have been found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void SyncAssets_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SyncAssetsViewModel viewModel = new(_application);
            SyncAssetsWindow syncAssetsWindow = new(viewModel);
            syncAssetsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var about = _application.GetAboutInformation(GetType().Assembly);
            AboutWindow duplicatedAssetsWindow = new(about);
            duplicatedAssetsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            log.Error(ex);
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

    private void MoveAssets(bool preserveOriginalFiles)
    {
        try
        {
            var assets = ViewModel.SelectedAssets;

            if (assets != null && assets.Length > 0)
            {
                FolderNavigationWindow folderNavigationWindow = new(
                    new FolderNavigationViewModel(
                        _application,
                        assets.First().Folder,
                        ViewModel.LastSelectedFolder,
                        _application.GetRecentTargetPaths()));

                folderNavigationWindow.Closed += (sender, e) =>
                {
                    if (folderNavigationWindow.ViewModel.HasConfirmed)
                    {
                        bool result = true;

                        result = _application.MoveAssets(assets,
                            folderNavigationWindow.ViewModel.SelectedFolder,
                            preserveOriginalFiles);

                        if (result)
                        {
                            ViewModel.LastSelectedFolder = folderNavigationWindow.ViewModel.SelectedFolder;
                            ViewModel.IsRefreshingFolders = true;
                            folderTreeView.Initialize();
                            ViewModel.IsRefreshingFolders = false;

                            if (!preserveOriginalFiles)
                            {
                                ViewModel.RemoveAssets(assets);

                                if (ViewModel.AppMode == AppModeEnum.Viewer)
                                {
                                    viewerUserControl.ShowImage();
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
            log.Error(ex);
        }
    }

    private void DeleteAssets()
    {
        try
        {
            var assets = ViewModel.SelectedAssets;

            if (assets != null)
            {
                _application.DeleteAssets(assets, deleteFiles: true);
                ViewModel.RemoveAssets(assets);
                ShowImage();
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public void DeleteDuplicateAssets(Asset[] assets )
    {
        try
        {
            if (assets != null)
            {
                _application.DeleteAssets(assets, deleteFiles: true);
                ViewModel.RemoveAssets(assets);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public void RefreshAssetsCounter()
    {
        ViewModel.CalculateGlobaleAssetsCounter();
    }

    private void DeleteAssets_Click(object sender, RoutedEventArgs e)
    {
        DeleteAssets();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SortAssetsByFileName_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteriaEnum.FileName);
    }

    private void SortAssetsByFileSize_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteriaEnum.FileSize);
    }

    private void SortAssetsByFileCreationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteriaEnum.FileCreationDateTime);
    }

    private void SortAssetsByFileModificationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteriaEnum.FileModificationDateTime);
    }

    private void SortAssetsByThumbnailCreationDateTime_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SortAssetsByCriteria(SortCriteriaEnum.ThumbnailCreationDateTime);
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var taskCancellation = Task.Run(() =>
        {
            _cts.Cancel();
        });

        await taskCancellation.ConfigureAwait(true);
        await catalogTask.ConfigureAwait(true);
        //e.Cancel = catalogTask != null && !catalogTask.IsCompleted; // Now that all tasks are canceled, the window can be closed properly
    }

    private async Task DoBackgroundWork()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        ViewModel.StatusMessage = "Cataloging thumbnails for " + ViewModel.CurrentFolder;

        // Disabling infinite loop to prevent reduced perfs
        if (AssetConstants.SyncAssetsEveryXMinutes)
        {
            int minutes = ViewModel.GetCatalogCooldownMinutes();

            while (true)
            {
                await Initialization(stopwatch);
                await Task.Delay(1000 * 60 * minutes, CancellationToken.None).ConfigureAwait(true);
            }
        }

        await Initialization(stopwatch);
    }

    private async Task Initialization(Stopwatch stopwatch)
    {
        try
        {
            catalogTask = ViewModel.CatalogAssets(
            async (e) =>
            {
                // The InvokeAsync method is used to avoid freezing the application when the task is cancelled.
                await Dispatcher.InvokeAsync(() => ViewModel.NotifyCatalogChange(e));
            }, _cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            log.Error(ex);
        }

        await catalogTask.ConfigureAwait(true);
        ViewModel?.CalculateGlobaleAssetsCounter();
        stopwatch.Stop();
        ViewModel?.SetExecutionTime(stopwatch.Elapsed);
        ViewModel?.CalculateTotalFilesNumber();
    }
}
