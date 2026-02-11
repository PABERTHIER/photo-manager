using log4net;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Controls;

/// <summary>
/// Interaction logic for ViewerUserControl.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ViewerUserControl
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public ViewerUserControl()
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

    private void NextButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToNextAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void PreviousButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToPreviousAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ViewModel.CurrentAsset != null)
            {
                ThumbnailSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ChangeAppMode();
    }

    // Triggered when double-clicked on the thumbnail image from ThumbnailsUserControl to pass into fullscreen mode
    public void ShowImage()
    {
        if (ViewModel is { CurrentAsset: not null })
        {
            bool isHeic = ViewModel.CurrentAsset.FileName.EndsWith(".heic", StringComparison.OrdinalIgnoreCase);

            BitmapImage source = isHeic ? ViewModel.LoadBitmapHeicImageFromPath() : ViewModel.LoadBitmapImageFromPath();

            Image.Source = source;
            BackgroundImage.Source = source;
        }
        else
        {
            Image.Source = null;
            BackgroundImage.Source = null;
        }
    }
}
