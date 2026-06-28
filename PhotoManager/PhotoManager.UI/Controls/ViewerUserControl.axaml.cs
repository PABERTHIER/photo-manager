namespace PhotoManager.UI.Controls;

/// <summary>
/// User control that displays the currently selected asset (image or video viewer).
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ViewerUserControl : UserControl
{
    private readonly ILogger<ViewerUserControl> _logger;
    private Bitmap? _currentSource;

    public ViewerUserControl()
    {
        _logger = App.ServiceProvider?.GetService<ILogger<ViewerUserControl>>()
                  ?? NullLogger<ViewerUserControl>.Instance;
        InitializeComponent();
    }

    public event EventHandler? ThumbnailSelected;

    private ApplicationViewModel ViewModel => (ApplicationViewModel)DataContext!;

    public void ShowImage()
    {
        if (ViewModel.CurrentAsset != null)
        {
            SetImageSource(ViewModel.LoadBitmapImageFromPath());
        }
        else
        {
            SetImageSource(null);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetImageSource(null);
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        BackgroundImage = this.FindControl<Image>(nameof(BackgroundImage))
            ?? throw new InvalidOperationException("BackgroundImage control was not found.");
        Image = this.FindControl<Image>(nameof(Image))
            ?? throw new InvalidOperationException("Image control was not found.");
    }

    private void NextButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToNextAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void PreviousButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ViewModel.GoToPreviousAsset();
            ShowImage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            if (e.ClickCount == 2 && ViewModel.CurrentAsset != null)
            {
                ThumbnailSelected?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.ChangeAppMode();
    }

    private void SetImageSource(Bitmap? source)
    {
        if (ReferenceEquals(_currentSource, source))
        {
            return;
        }

        Bitmap? previousSource = _currentSource;
        _currentSource = source;
        Image.Source = source;
        BackgroundImage.Source = source;
        previousSource?.Dispose();
    }
}
