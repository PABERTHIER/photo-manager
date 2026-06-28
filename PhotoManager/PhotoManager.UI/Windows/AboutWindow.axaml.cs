namespace PhotoManager.UI.Windows;

/// <summary>
/// Window displaying application information such as product, version and author details.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    public AboutWindow(AboutInformation aboutInformation, ILogger<AboutWindow> logger)
    {
        try
        {
            InitializeComponent();
            DataContext = aboutInformation;
            Title = $"About {aboutInformation.Product} {aboutInformation.Version}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
