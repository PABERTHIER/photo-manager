using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using PhotoManager.Domain;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.Windows;

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
