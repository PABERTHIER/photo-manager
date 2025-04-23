using log4net;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.ViewModels.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

// TODO: Move it into Models folder -> ThumbnailSelectedEventHandler
public class ThumbnailSelectedEventArgs : EventArgs
{
    public Asset Asset { get; set; } // TODO: Remove this arg
}

public delegate void ThumbnailSelectedEventHandler(object sender, ThumbnailSelectedEventArgs e);

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly IApplication _application;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task _catalogTask;

    // TODO: Rework aboutInformation to get it only once (need to Add Author into the VM)
    public MainWindow(ApplicationViewModel viewModel, IApplication application)
    {
        try
        {
            InitializeComponent();
            Current = this;
            _cancellationTokenSource = new();

            _application = application;
            DataContext = viewModel;

            SetAboutInformation();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static MainWindow Current { get; private set; }

    public ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.ChangeAppMode(AppMode.Thumbnails);
            thumbnailsUserControl.GoToFolder(_application, ViewModel?.CurrentFolderPath);
            folderTreeView.SelectedPath = ViewModel?.CurrentFolderPath;
            await DoBackgroundWork();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        finally
        {
            ViewModel.StatusMessage = "";
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
                        DeleteAssets();
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
            Log.Error(ex);
        }
    }

    private void ThumbnailsUserControl_ThumbnailSelected(object sender, ThumbnailSelectedEventArgs e)
    {
        try
        {
            ViewModel.GoToAsset(e.Asset, AppMode.Viewer);
            ShowImage();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void ViewerUserControl_ThumbnailSelected(object sender, ThumbnailSelectedEventArgs e)
    {
        try
        {
            ViewModel.GoToAsset(e.Asset, AppMode.Thumbnails);
            ShowImage();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
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
            Log.Error(ex);
        }
    }

    private void FindDuplicates_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var duplicates = _application.GetDuplicatedAssets();

            if (duplicates.Count > 0)
            {
                FindDuplicatedAssetsViewModel viewModel = new (_application);
                viewModel.SetDuplicates(duplicates);
                FindDuplicatedAssetsWindow findDuplicatedAssetsWindow = new (viewModel);

                // TODO: For each event, test to define the event with the method and check if the method is well triggered
                findDuplicatedAssetsWindow.GetExemptedFolderPath += GetExemptedFolderPath;
                findDuplicatedAssetsWindow.DeleteDuplicatedAssets += DeleteDuplicatedAssets;
                findDuplicatedAssetsWindow.RefreshAssetsCounter += RefreshAssetsCounter;

                findDuplicatedAssetsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("No duplicates have been found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void SyncAssets_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SyncAssetsViewModel syncAssetsViewModel = new (_application);
            SyncAssetsWindow syncAssetsWindow = new (syncAssetsViewModel);
            syncAssetsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // TODO: Why doing it twice ? Already done in the ctor, use instead VM
            AboutInformation aboutInformation = _application.GetAboutInformation(GetType().Assembly);
            AboutWindow aboutWindow = new (aboutInformation);  // TODO: Add Author above
            aboutWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
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
                        ViewModel.MoveAssetsLastSelectedFolder,
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
                            ViewModel.MoveAssetsLastSelectedFolder = folderNavigationWindow.ViewModel.SelectedFolder;
                            ViewModel.IsRefreshingFolders = true;
                            folderTreeView.Initialize();
                            ViewModel.IsRefreshingFolders = false;

                            if (!preserveOriginalFiles)
                            {
                                ViewModel.RemoveAssets(assets);

                                if (ViewModel.AppMode == AppMode.Viewer)
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
            Log.Error(ex);
        }
    }

    private void DeleteAssets()
    {
        try
        {
            Asset[] selectedAssets = ViewModel.SelectedAssets;

            if (selectedAssets.Length > 0)
            {
                _application.DeleteAssets(selectedAssets); // TODO: Need to rework how the deletion is handled
                ViewModel.RemoveAssets(selectedAssets);
                ShowImage();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void DeleteDuplicatedAssets(object sender, Asset[] assets)
    {
        try
        {
            if (assets.Length > 0)
            {
                _application.DeleteAssets(assets); // TODO: Can make one method with above
                ViewModel.RemoveAssets(assets);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void RefreshAssetsCounter(object sender)
    {
        ViewModel.CalculateGlobalAssetsCounter();
    }

    private string GetExemptedFolderPath(object sender) => ViewModel.GetExemptedFolderPath();

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

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Task taskCancellation = Task.Run(() =>
        {
            _cancellationTokenSource.Cancel();
        });

        await taskCancellation.ConfigureAwait(true);
        await _catalogTask.ConfigureAwait(true);
        //e.Cancel = catalogTask != null && !catalogTask.IsCompleted; // Now that all tasks are canceled, the window can be closed properly
    }

    private async Task DoBackgroundWork()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        ViewModel.StatusMessage = "Cataloging thumbnails for " + ViewModel.CurrentFolderPath;

        if (ViewModel.GetSyncAssetsEveryXMinutes()) // Disabling infinite loop to prevent reduced perf
        {
            ushort minutes = ViewModel.GetCatalogCooldownMinutes();

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
            _catalogTask = ViewModel.CatalogAssets(
            async (e) =>
            {
                // The InvokeAsync method is used to avoid freezing the application when the task is cancelled.
                await Dispatcher.InvokeAsync(() => ViewModel.NotifyCatalogChange(e));
            }, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            Log.Error(ex);
        }

        await _catalogTask.ConfigureAwait(true);
        ViewModel.CalculateGlobalAssetsCounter();
        stopwatch.Stop();
        ViewModel.SetExecutionTime(stopwatch.Elapsed);
        ViewModel.CalculateTotalFilesCount();
    }

    private void SetAboutInformation()
    {
        AboutInformation aboutInformation = _application.GetAboutInformation(GetType().Assembly);
        ViewModel.Product = aboutInformation.Product; // TODO: Add Author here and above
        ViewModel.Version = aboutInformation.Version;
    }

    private void ShowImage()
    {
        if (ViewModel.AppMode == AppMode.Viewer)
        {
            viewerUserControl.ShowImage();
        }
        else
        {
            thumbnailsUserControl.ShowImage();
        }
    }
}
