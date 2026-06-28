namespace PhotoManager.UI.Windows;

/// <summary>
/// Modal dialog window that shows a titled message to the user.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class MessageDialogWindow : Window
{
    public MessageDialogWindow()
    {
        InitializeComponent();
    }

    public MessageDialogWindow(string message, string title) : this()
    {
        Title = title;
        MessageTextBlock.Text = message;
    }

    public static async Task ShowAsync(Window owner, string message, string title)
    {
        MessageDialogWindow window = new(message, title);
        await window.ShowDialog(owner);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        MessageTextBlock = this.FindControl<TextBlock>(nameof(MessageTextBlock))
            ?? throw new InvalidOperationException("MessageTextBlock control was not found.");
    }

    private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
