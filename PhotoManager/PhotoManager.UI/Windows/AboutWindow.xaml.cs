using log4net;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using System.Reflection;
using System.Windows;

namespace PhotoManager.UI.Windows;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class AboutWindow
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public AboutWindow(AboutInformation aboutInformation)
    {
        try
        {
            InitializeComponent();

            DataContext = aboutInformation;
            Title = $"About {aboutInformation.Product} {aboutInformation.Version}";
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void ConfirmButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
