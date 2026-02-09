using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Windows;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace PhotoManager.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class App
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static readonly Mutex AppMutex = new(true, "PhotoManagerStartup");

    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        ServiceCollection serviceCollection = new();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            if (AppMutex.WaitOne(TimeSpan.Zero, true))
            {
                MainWindow? mainWindow = _serviceProvider.GetService<MainWindow>();

                if (mainWindow != null)
                {
                    mainWindow.Show();
                }
                else
                {
                    Shutdown();
                }
            }
            else
            {
                MessageBox.Show("The application is already running.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            MessageBox.Show("The application failed to initialize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddInfrastructure();
        services.AddDomain();
        services.AddApplication();
        services.AddUi();
    }
}
