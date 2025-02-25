﻿using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Infrastructure;
using PhotoManager.Infrastructure.Database.Storage;
using PhotoManager.Infrastructure.Database;
using PhotoManager.UI.ViewModels;
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
public partial class App : System.Windows.Application
{
    private readonly ServiceProvider serviceProvider;
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static readonly Mutex AppMutex = new (true, "PhotoManagerStartup");
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
            if (AppMutex.WaitOne(TimeSpan.Zero, true))
            {
                MainWindow? mainWindow = serviceProvider.GetService<MainWindow>();

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
            log.Error(ex);
            MessageBox.Show("The application failed to initialize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        string? baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // TODO: Needed ?
        string configFilePath = Path.Combine(baseDirectory, "appsettings.json");

        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        // TODO: group by module
        services.AddSingleton(configuration);
        services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
        services.AddSingleton<IObjectListStorage, ObjectListStorage>();
        services.AddSingleton<IBlobStorage, BlobStorage>();
        services.AddSingleton<IBackupStorage, BackupStorage>();
        services.AddSingleton<IDatabase, Database>();
        services.AddSingleton<IAssetsComparator, AssetsComparator>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IAssetRepository, AssetRepository>();
        services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
        services.AddSingleton<IAssetCreationService, AssetCreationService>();
        services.AddSingleton<ICatalogAssetsService, CatalogAssetsService>();
        services.AddSingleton<IMoveAssetsService, MoveAssetsService>();
        services.AddSingleton<IFindDuplicatedAssetsService, FindDuplicatedAssetsService>();
        services.AddSingleton<ISyncAssetsService, SyncAssetsService>();
        services.AddSingleton<Application.IApplication, Application.Application>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ApplicationViewModel>();
    }

    // TODO: if needed to test the registers: here is the sample for database, need to be completed
    //[TestMethod]
    //public void AddDatabaseServices_AddsAllRequiredServices()
    //{
    //    // Arrange
    //    var services = new ServiceCollection();

    //    // Act
    //    services.AddDatabaseServices();

    //    // Assert
    //    Assert.That(services.Any(descriptor => descriptor.ServiceType == typeof(IObjectListStorage)), Is.True);
    //    Assert.That(services.Any(descriptor => descriptor.ServiceType == typeof(IBlobStorage)), Is.True);
    //    Assert.That(services.Any(descriptor => descriptor.ServiceType == typeof(IBackupStorage)), Is.True);
    //    Assert.That(services.Any(descriptor => descriptor.ServiceType == typeof(IDatabase)), Is.True);
    //}
}
