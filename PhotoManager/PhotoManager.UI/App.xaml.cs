using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Windows;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace PhotoManager.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
[ExcludeFromCodeCoverage]
public partial class App
{
    private static readonly Mutex AppMutex = new(true, "PhotoManagerStartup");

    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Exposes the DI container for use in XAML-instantiated controls that cannot receive constructor injection
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    public App()
    {
        ServiceCollection serviceCollection = new();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        ServiceProvider = _serviceProvider;
    }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        ILogger<App> logger = _serviceProvider.GetRequiredService<ILogger<App>>();
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
                MessageBox.Show("The application is already running.", "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            MessageBox.Show("The application failed to initialize.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationBuilder builder =
            new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                "log.txt",
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u5} {SourceContext} - {Message:lj}{NewLine}{Exception}",
                fileSizeLimitBytes: 10L * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 10)
            .CreateLogger();

        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddSerilog(dispose: true);
            logging.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton(configuration);
        services.AddInfrastructure();
        services.AddDomain();
        services.AddApplication();
        services.AddUi();
    }
}
