﻿using log4net;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhotoManager.UI.Controls;

/// <summary>
/// Interaction logic for ThumbnailsUserControl.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ThumbnailsUserControl
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public ThumbnailsUserControl()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public event EventHandler? ThumbnailSelected;

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext;

    // TODO: No async void -> async Task
    public async void GoToFolder(IApplication application, string selectedImagePath)
    {
        try
        {
            if (!ViewModel.IsRefreshingFolders)
            {
                // TODO: Each time the folder is switched, it will LoadBitmapThumbnailImage for each asset in the current dir
                // (if switching between two folders multiple time, it will call each time LoadBitmapThumbnailImage) -> not good for perf
                Asset[] assets = await GetAssets(application, selectedImagePath).ConfigureAwait(true);

                ViewModel.SetAssets(selectedImagePath, assets);

                if (ThumbnailsListView.Items.Count > 0 && ThumbnailsListView.Items[0] != null)
                {
                    ViewModel.ViewerPosition = 0;
                    ThumbnailsListView.ScrollIntoView(ThumbnailsListView.Items[0]!);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private static Task<Asset[]> GetAssets(IApplication application, string directory)
    {
        return Task.Run(() => application.GetAssetsByPath(directory));
    }

    private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            ThumbnailSelected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    // Triggered when double-clicked on the fullscreen image from ViewerUserControl to pass into thumbnail mode
    public void ShowImage() // TODO: Rename method to CloseImage
    {
        if (ThumbnailsListView.Items.Count > 0 && ThumbnailsListView.SelectedItem != null)
        {
            ThumbnailsListView.ScrollIntoView(ThumbnailsListView.SelectedItem);
        }
    }

    private void ThumbnailsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedAssets = ThumbnailsListView.SelectedItems.Cast<Asset>().ToArray();
    }
}
