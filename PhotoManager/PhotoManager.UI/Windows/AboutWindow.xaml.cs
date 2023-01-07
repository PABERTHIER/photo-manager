using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using log4net;
using System;
using System.Reflection;
using System.Windows;

namespace PhotoManager.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class AboutWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                log.Error(ex);
            }
        }

        private void ConfirmButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
