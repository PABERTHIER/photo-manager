using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Infrastructure;
using PhotoManager.UI.Configuration;
using PhotoManager.UI.Services;
using PhotoManager.UI.Windows;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI;

[ExcludeFromCodeCoverage]
public class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;

    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceCollection serviceCollection = new();
        IConfigurationRoot configuration = BuildConfiguration();
        ConfigureServices(serviceCollection, configuration);

        _serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        ServiceProvider = _serviceProvider;
        ApplyConfiguredTheme(_serviceProvider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            StartDesktopApplication(desktop);
            desktop.Exit += (_, _) => DisposeServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void StartDesktopApplication(IClassicDesktopStyleApplicationLifetime desktop)
    {
        ServiceProvider serviceProvider = _serviceProvider
            ?? throw new InvalidOperationException("Service provider has not been initialized.");
        ILogger<App> logger = serviceProvider.GetRequiredService<ILogger<App>>();

        try
        {
            ISingleInstanceService singleInstanceService =
                serviceProvider.GetRequiredService<ISingleInstanceService>();

            if (singleInstanceService.TryAcquire())
            {
                desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
            }
            else
            {
                logger.LogWarning("The application is already running.");
                desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExMessage}", ex.Message);
            throw;
        }
    }

    private void DisposeServiceProvider()
    {
        ServiceProvider = null;
        _serviceProvider?.Dispose();
        _serviceProvider = null;
    }

    private void ApplyConfiguredTheme(ServiceProvider serviceProvider)
    {
        IUserConfigurationService userConfigurationService =
            serviceProvider.GetRequiredService<IUserConfigurationService>();
        RequestedThemeVariant =
            ThemeSettingsReader.GetRequestedThemeVariant(userConfigurationService.UiSettings.ThemeMode);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        IConfigurationBuilder builder =
            new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        return builder.Build();
    }

    private static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
    {
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
