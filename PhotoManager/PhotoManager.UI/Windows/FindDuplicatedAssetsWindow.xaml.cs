using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for DuplicatedAssetsWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class DuplicatedAssetsWindow : Window
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DuplicatedAssetsWindow(FindDuplicatedAssetsViewModel viewModel)
    {
        try
        {
            InitializeComponent();

            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public FindDuplicatedAssetsViewModel ViewModel
    {
        get { return (FindDuplicatedAssetsViewModel)DataContext; }
    }

    private List<DuplicatedSetViewModel> DuplicatedAssets
    {
        get { return ViewModel.GetAllDuplicatedAssets(); }
    }

    private void DeleteLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel viewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            Asset asset = viewModel.Asset;
            ViewModel.DeleteAsset(viewModel);

            // TODO: IN THE LIST BOXES, IF THE FILENAME INCLUDES _ IT IS NOT BEING SHOWN.
            Console.WriteLine("Delete " + asset.FullPath);
            // TODO: IF THE USER IS VIEWING THE FOLDER IN WHICH THE DUPLICATE WAS STORED, THE THUMBNAILS LIST WON'T REFRESH
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void DeleteAllLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            DuplicatedAssetViewModel viewModel = (DuplicatedAssetViewModel)((FrameworkElement)e.Source).DataContext;
            Asset asset = viewModel.Asset;
            var currentRootFolderPath = asset.Folder.Path;
            var duplicatedAssets = DuplicatedAssets;
            var isFirstOne = false;

            foreach (var duplicatedAssetList in duplicatedAssets)
            {
                isFirstOne = !isFirstOne;
                foreach (var duplicatedAsset in duplicatedAssetList)
                {
                    if (!isFirstOne && duplicatedAsset.Asset.Folder.Path == currentRootFolderPath)
                    {
                        ViewModel.DeleteAsset(duplicatedAsset);
                        continue;
                    }

                    if (isFirstOne)
                    {
                        isFirstOne = !isFirstOne;
                    }                        
                }
            }

            ViewModel.Refresh();
        }
        catch (Exception ex)
        {
            log.Error(ex);
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
            log.Error(ex);
        }
    }
}
