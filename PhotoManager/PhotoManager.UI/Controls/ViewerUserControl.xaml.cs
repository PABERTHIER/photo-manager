﻿using log4net;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.ViewModels.Enums;
using PhotoManager.UI.Windows;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Controls;

/// <summary>
/// Interaction logic for ViewerUserControl.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ViewerUserControl : UserControl
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public event ThumbnailSelectedEventHandler ThumbnailSelected;

    public ViewerUserControl()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private ApplicationViewModel ViewModel
    {
        get { return (ApplicationViewModel)DataContext; }
    }

    private void NextButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.GoToNextAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void PreviousButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel?.GoToPreviousAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            ThumbnailSelected?.Invoke(this, new ThumbnailSelectedEventArgs() { Asset = ViewModel.CurrentAsset });
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ChangeAppMode(AppMode.Thumbnails);
    }

    // Triggered when double clicked on the thumbnail image from ThumbnailsUserControl to pass into fullscreen mode
    public void ShowImage()
    {
        if (ViewModel.ViewerPosition >= 0)
        {
            BitmapImage? source;
            bool isHeic = ViewModel.CurrentAsset.FileName.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

            if (isHeic)
            {
                source = ViewModel.LoadBitmapHeicImageFromPath();
            }
            else
            {
                source = ViewModel.LoadBitmapImageFromPath();
            }

            if (source != null)
            {
                image.Source = source;
                backgroundImage.Source = source;
            }
        }
        else
        {
            image.Source = null;
            backgroundImage.Source = null;
        }
    }
}
