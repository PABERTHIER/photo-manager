namespace PhotoManager.UI.Windows;

/// <summary>
/// Window listing the available keyboard shortcuts.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class ShortcutsWindow : Window
{
    public ShortcutsWindow() : this(
        new(),
        App.ServiceProvider?.GetRequiredService<ILogger<ShortcutsWindow>>()
        ?? throw new InvalidOperationException("ShortcutsWindow logger service is not available."))
    {
    }

    public ShortcutsWindow(ShortcutsViewModel viewModel, ILogger<ShortcutsWindow> logger)
    {
        try
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
