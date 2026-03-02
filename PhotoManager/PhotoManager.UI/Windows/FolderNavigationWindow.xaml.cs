using Microsoft.Extensions.Logging;
using PhotoManager.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for FolderNavigationWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FolderNavigationWindow
{
    public FolderNavigationWindow(FolderNavigationViewModel viewModel, ILogger<FolderNavigationWindow> logger)
    {
        try
        {
            InitializeComponent();

            DataContext = viewModel;
            FolderTreeView.SelectedPath = viewModel.LastSelectedFolder != null
                ? viewModel.LastSelectedFolder.Path
                : viewModel.SourceFolder.Path;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    public FolderNavigationViewModel ViewModel => (FolderNavigationViewModel)DataContext;

    private void FolderTreeView_FolderSelected(object sender, EventArgs e)
    {
        ViewModel.TargetPath = FolderTreeView.SelectedPath;
    }

    private void Confirm_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        Confirm();
    }

    private void Cancel_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        Cancel();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Confirm();
                break;

            case Key.Escape:
                Cancel();
                break;
            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }
    }

    private void Confirm()
    {
        if (ViewModel.CanConfirm)
        {
            ViewModel.HasConfirmed = true;
        }

        Close();
    }

    private void Cancel()
    {
        ViewModel.HasConfirmed = false;
        Close();
    }
}
