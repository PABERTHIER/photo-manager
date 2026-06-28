using PhotoManager.UI.Controls;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Window hosting the folder tree used to navigate asset directories.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class FolderNavigationWindow : Window
{
    public FolderNavigationWindow()
    {
        InitializeComponent();
    }

    public FolderNavigationWindow(FolderNavigationViewModel viewModel, ILogger<FolderNavigationWindow> logger)
    {
        try
        {
            InitializeComponent();
            DataContext = viewModel;
            FolderTreeView.DataContext = viewModel;
            FolderTreeView.SelectedPath = viewModel.LastSelectedFolder != null
                ? viewModel.LastSelectedFolder.Path
                : viewModel.SourceFolder.Path;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    public FolderNavigationViewModel ViewModel => (FolderNavigationViewModel)DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        FolderTreeView = this.FindControl<FolderNavigationControl>(nameof(FolderTreeView))
            ?? throw new InvalidOperationException("FolderTreeView control was not found.");
    }

    private void FolderTreeView_FolderSelected(object? sender, EventArgs e)
    {
        ViewModel.TargetPath = FolderTreeView.SelectedPath;
    }

    private void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Confirm();
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Cancel();
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
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
