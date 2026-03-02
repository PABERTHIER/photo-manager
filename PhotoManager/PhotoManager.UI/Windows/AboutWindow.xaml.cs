using Microsoft.Extensions.Logging;
using PhotoManager.Domain;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class AboutWindow
{
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

    private void ConfirmButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
