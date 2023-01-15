using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Infrastructure;
using PhotoManager.UI.ViewModels;
using PhotoManager.UI.Windows;
using SimplePortableDatabase;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PhotoManager.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class App : System.Windows.Application
{
    private readonly ServiceProvider serviceProvider;
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    // TODO: Add a global exception handler.

    public App()
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        serviceProvider = serviceCollection.BuildServiceProvider() ?? new ServiceCollection().BuildServiceProvider();
    }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            if (!serviceProvider.GetService<IProcessService>()?.IsAlreadyRunning() ?? false)
            {
                var mainWindow = serviceProvider.GetService<MainWindow>();
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
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
            MessageBox.Show("The application failed to initialize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddSimplePortableDatabaseServices();
        services.AddSingleton<IDirectoryComparer, DirectoryComparer>();
        services.AddSingleton<IProcessService, ProcessService>();
        services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IAssetRepository, AssetRepository>();
        services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
        services.AddSingleton<ICatalogAssetsService, CatalogAssetsService>();
        services.AddSingleton<IMoveAssetsService, MoveAssetsService>();
        services.AddSingleton<IFindDuplicatedAssetsService, FindDuplicatedAssetsService>();
        services.AddSingleton<ISyncAssetsService, SyncAssetsService>();
        services.AddSingleton<Application.IApplication, Application.Application>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ApplicationViewModel>();
    }
}
