using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotoManager.Application;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.UI;
using PhotoManager.UI.Services;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class AppTests
{
    [Test]
    public void ConfigureServices_RegistersExpectedServices()
    {
        ServiceCollection services = new();

        ServiceDescriptor? optionsDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptions<>));
        Assert.That(optionsDescriptor, Is.Null);

        ServiceDescriptor? optionsSnapshotDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsSnapshot<>));
        Assert.That(optionsSnapshotDescriptor, Is.Null);

        ServiceDescriptor? optionsMonitorDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsMonitor<>));
        Assert.That(optionsMonitorDescriptor, Is.Null);

        ServiceDescriptor? optionsFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsFactory<>));
        Assert.That(optionsFactoryDescriptor, Is.Null);

        ServiceDescriptor? optionsMonitorCacheDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsMonitorCache<>));
        Assert.That(optionsMonitorCacheDescriptor, Is.Null);

        ServiceDescriptor? loggerFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ILoggerFactory));
        Assert.That(loggerFactoryDescriptor, Is.Null);

        ServiceDescriptor? loggerDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ILogger<>));
        Assert.That(loggerDescriptor, Is.Null);

        ServiceDescriptor? loggerFilterConfigureDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IConfigureOptions<LoggerFilterOptions>));
        Assert.That(loggerFilterConfigureDescriptor, Is.Null);

        // appsettings.json
        ServiceDescriptor? configurationRootDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IConfigurationRoot));
        Assert.That(configurationRootDescriptor, Is.Null);

        // Infrastructure
        ServiceDescriptor? pathProviderServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IPathProviderService));
        Assert.That(pathProviderServiceDescriptor, Is.Null);

        // Persistence
        ServiceDescriptor? sqliteConnectionFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(sqliteConnectionFactoryDescriptor, Is.Null);

        ServiceDescriptor? sqliteBackupServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(sqliteBackupServiceDescriptor, Is.Null);

        ServiceDescriptor? persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Null);

        // Infrastructure
        ServiceDescriptor? userConfigurationServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Null);

        ServiceDescriptor? fileOperationsServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IFileOperationsService));
        Assert.That(fileOperationsServiceDescriptor, Is.Null);

        ServiceDescriptor? imageProcessingServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IImageProcessingService));
        Assert.That(imageProcessingServiceDescriptor, Is.Null);

        ServiceDescriptor? thumbnailGeneratorDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IThumbnailGenerator));
        Assert.That(thumbnailGeneratorDescriptor, Is.Null);

        ServiceDescriptor? imageMetadataServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IImageMetadataService));
        Assert.That(imageMetadataServiceDescriptor, Is.Null);

        ServiceDescriptor? assetRepositoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Null);

        ServiceDescriptor? assetHashCalculatorServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Null);

        // Domain
        ServiceDescriptor? assetsComparatorDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Null);

        ServiceDescriptor? assetConversionServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetConversionService));
        Assert.That(assetConversionServiceDescriptor, Is.Null);

        ServiceDescriptor? assetCreationServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Null);

        ServiceDescriptor? catalogAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? catalogFolderPipelineDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(CatalogFolderPipeline));
        Assert.That(catalogFolderPipelineDescriptor, Is.Null);

        ServiceDescriptor? findDuplicatedAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? moveAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? syncAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Null);

        // Application
        ServiceDescriptor? applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Null);

        // UI
        ServiceDescriptor? singleInstanceServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISingleInstanceService));
        Assert.That(singleInstanceServiceDescriptor, Is.Null);

        ServiceDescriptor? mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Null);

        ServiceDescriptor? applicationViewModelDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        ConfigureServices(services);

        Assert.That(services, Has.Count.EqualTo(32));

        optionsDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptions<>));
        Assert.That(optionsDescriptor, Is.Not.Null);
        Assert.That(optionsDescriptor, Is.EqualTo(services[0]));
        Assert.That(optionsDescriptor.ImplementationInstance, Is.Null);
        Assert.That(optionsDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        // UnnamedOptionsManager<> is internal to Microsoft.Extensions.Options, so it is asserted by full name.
        Assert.That(optionsDescriptor.ImplementationType?.FullName,
            Is.EqualTo("Microsoft.Extensions.Options.UnnamedOptionsManager`1"));

        optionsSnapshotDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsSnapshot<>));
        Assert.That(optionsSnapshotDescriptor, Is.Not.Null);
        Assert.That(optionsSnapshotDescriptor, Is.EqualTo(services[1]));
        Assert.That(optionsSnapshotDescriptor.ImplementationInstance, Is.Null);
        Assert.That(optionsSnapshotDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        Assert.That(optionsSnapshotDescriptor.ImplementationType, Is.EqualTo(typeof(OptionsManager<>)));

        optionsMonitorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsMonitor<>));
        Assert.That(optionsMonitorDescriptor, Is.Not.Null);
        Assert.That(optionsMonitorDescriptor, Is.EqualTo(services[2]));
        Assert.That(optionsMonitorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(optionsMonitorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(optionsMonitorDescriptor.ImplementationType, Is.EqualTo(typeof(OptionsMonitor<>)));

        optionsFactoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsFactory<>));
        Assert.That(optionsFactoryDescriptor, Is.Not.Null);
        Assert.That(optionsFactoryDescriptor, Is.EqualTo(services[3]));
        Assert.That(optionsFactoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(optionsFactoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));
        Assert.That(optionsFactoryDescriptor.ImplementationType, Is.EqualTo(typeof(OptionsFactory<>)));

        optionsMonitorCacheDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IOptionsMonitorCache<>));
        Assert.That(optionsMonitorCacheDescriptor, Is.Not.Null);
        Assert.That(optionsMonitorCacheDescriptor, Is.EqualTo(services[4]));
        Assert.That(optionsMonitorCacheDescriptor.ImplementationInstance, Is.Null);
        Assert.That(optionsMonitorCacheDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(optionsMonitorCacheDescriptor.ImplementationType, Is.EqualTo(typeof(OptionsCache<>)));

        loggerFactoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ILoggerFactory));
        Assert.That(loggerFactoryDescriptor, Is.Not.Null);
        Assert.That(loggerFactoryDescriptor, Is.EqualTo(services[5]));
        Assert.That(loggerFactoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(loggerFactoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(loggerFactoryDescriptor.ImplementationType, Is.EqualTo(typeof(LoggerFactory)));

        loggerDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ILogger<>));
        Assert.That(loggerDescriptor, Is.Not.Null);
        Assert.That(loggerDescriptor, Is.EqualTo(services[6]));
        Assert.That(loggerDescriptor.ImplementationInstance, Is.Null);
        Assert.That(loggerDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(loggerDescriptor.ImplementationType, Is.EqualTo(typeof(Logger<>)));

        loggerFilterConfigureDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IConfigureOptions<LoggerFilterOptions>));
        Assert.That(loggerFilterConfigureDescriptor, Is.Not.Null);
        Assert.That(loggerFilterConfigureDescriptor, Is.EqualTo(services[7]));
        Assert.That(loggerFilterConfigureDescriptor.ImplementationInstance, Is.Not.Null);
        Assert.That(loggerFilterConfigureDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(loggerFilterConfigureDescriptor.ImplementationType, Is.Null);

        // appsettings.json
        configurationRootDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IConfigurationRoot));
        Assert.That(configurationRootDescriptor, Is.Not.Null);
        Assert.That(configurationRootDescriptor, Is.EqualTo(services[8]));
        Assert.That(configurationRootDescriptor.ImplementationInstance, Is.Not.Null);
        Assert.That(configurationRootDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(configurationRootDescriptor.ImplementationType, Is.Null);

        // Infrastructure
        pathProviderServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IPathProviderService));
        Assert.That(pathProviderServiceDescriptor, Is.Not.Null);
        Assert.That(pathProviderServiceDescriptor, Is.EqualTo(services[9]));
        Assert.That(pathProviderServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(pathProviderServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(pathProviderServiceDescriptor.ImplementationType, Is.EqualTo(typeof(PathProviderService)));

        // Persistence
        sqliteConnectionFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(sqliteConnectionFactoryDescriptor, Is.Not.Null);
        Assert.That(sqliteConnectionFactoryDescriptor, Is.EqualTo(services[10]));
        Assert.That(sqliteConnectionFactoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(sqliteConnectionFactoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(sqliteConnectionFactoryDescriptor.ImplementationType,
            Is.EqualTo(typeof(SqliteConnectionFactory)));

        sqliteBackupServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(sqliteBackupServiceDescriptor, Is.Not.Null);
        Assert.That(sqliteBackupServiceDescriptor, Is.EqualTo(services[11]));
        Assert.That(sqliteBackupServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(sqliteBackupServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(sqliteBackupServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(SqliteBackupService)));

        persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Not.Null);
        Assert.That(persistenceContextDescriptor, Is.EqualTo(services[12]));
        Assert.That(persistenceContextDescriptor.ImplementationInstance, Is.Null);
        Assert.That(persistenceContextDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        // Registered via a factory (the context self-initializes at the composition root).
        Assert.That(persistenceContextDescriptor.ImplementationType, Is.Null);
        Assert.That(persistenceContextDescriptor.ImplementationFactory, Is.Not.Null);

        // Infrastructure
        userConfigurationServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Not.Null);
        Assert.That(userConfigurationServiceDescriptor, Is.EqualTo(services[13]));
        Assert.That(userConfigurationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(userConfigurationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(userConfigurationServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(UserConfigurationService)));

        fileOperationsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IFileOperationsService));
        Assert.That(fileOperationsServiceDescriptor, Is.Not.Null);
        Assert.That(fileOperationsServiceDescriptor, Is.EqualTo(services[14]));
        Assert.That(fileOperationsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(fileOperationsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(fileOperationsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(FileOperationsService)));

        imageProcessingServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IImageProcessingService));
        Assert.That(imageProcessingServiceDescriptor, Is.Not.Null);
        Assert.That(imageProcessingServiceDescriptor, Is.EqualTo(services[15]));
        Assert.That(imageProcessingServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(imageProcessingServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(imageProcessingServiceDescriptor.ImplementationType, Is.EqualTo(typeof(ImageProcessingService)));

        thumbnailGeneratorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IThumbnailGenerator));
        Assert.That(thumbnailGeneratorDescriptor, Is.Not.Null);
        Assert.That(thumbnailGeneratorDescriptor, Is.EqualTo(services[16]));
        Assert.That(thumbnailGeneratorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(thumbnailGeneratorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(thumbnailGeneratorDescriptor.ImplementationType, Is.EqualTo(typeof(ThumbnailGenerator)));

        imageMetadataServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IImageMetadataService));
        Assert.That(imageMetadataServiceDescriptor, Is.Not.Null);
        Assert.That(imageMetadataServiceDescriptor, Is.EqualTo(services[17]));
        Assert.That(imageMetadataServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(imageMetadataServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(imageMetadataServiceDescriptor.ImplementationType, Is.EqualTo(typeof(ImageMetadataService)));

        assetRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Not.Null);
        Assert.That(assetRepositoryDescriptor, Is.EqualTo(services[18]));
        Assert.That(assetRepositoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(AssetRepository)));

        assetHashCalculatorServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Not.Null);
        Assert.That(assetHashCalculatorServiceDescriptor, Is.EqualTo(services[19]));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetHashCalculatorServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(AssetHashCalculatorService)));

        // Domain
        assetsComparatorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Not.Null);
        Assert.That(assetsComparatorDescriptor, Is.EqualTo(services[20]));
        Assert.That(assetsComparatorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetsComparatorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetsComparatorDescriptor.ImplementationType, Is.EqualTo(typeof(AssetsComparator)));

        assetConversionServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetConversionService));
        Assert.That(assetConversionServiceDescriptor, Is.Not.Null);
        Assert.That(assetConversionServiceDescriptor, Is.EqualTo(services[21]));
        Assert.That(assetConversionServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetConversionServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetConversionServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetConversionService)));

        assetCreationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Not.Null);
        Assert.That(assetCreationServiceDescriptor, Is.EqualTo(services[22]));
        Assert.That(assetCreationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetCreationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetCreationServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetCreationService)));

        catalogFolderPipelineDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(CatalogFolderPipeline));
        Assert.That(catalogFolderPipelineDescriptor, Is.Not.Null);
        Assert.That(catalogFolderPipelineDescriptor, Is.EqualTo(services[23]));
        Assert.That(catalogFolderPipelineDescriptor.ImplementationInstance, Is.Null);
        Assert.That(catalogFolderPipelineDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(catalogFolderPipelineDescriptor.ImplementationType, Is.EqualTo(typeof(CatalogFolderPipeline)));

        catalogAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(catalogAssetsServiceDescriptor, Is.EqualTo(services[24]));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(catalogAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(CatalogAssetsService)));

        findDuplicatedAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.EqualTo(services[25]));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(FindDuplicatedAssetsService)));

        moveAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(moveAssetsServiceDescriptor, Is.EqualTo(services[26]));
        Assert.That(moveAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(moveAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(moveAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(MoveAssetsService)));

        syncAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(syncAssetsServiceDescriptor, Is.EqualTo(services[27]));
        Assert.That(syncAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(syncAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(syncAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(SyncAssetsService)));

        // Application
        applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Not.Null);
        Assert.That(applicationDescriptor, Is.EqualTo(services[28]));
        Assert.That(applicationDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationDescriptor.ImplementationType, Is.EqualTo(typeof(PhotoManager.Application.Application)));

        // UI
        singleInstanceServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISingleInstanceService));
        Assert.That(singleInstanceServiceDescriptor, Is.Not.Null);
        Assert.That(singleInstanceServiceDescriptor, Is.EqualTo(services[29]));
        Assert.That(singleInstanceServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(singleInstanceServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(singleInstanceServiceDescriptor.ImplementationType, Is.EqualTo(typeof(SingleInstanceService)));

        mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Not.Null);
        Assert.That(mainWindowDescriptor, Is.EqualTo(services[30]));
        Assert.That(mainWindowDescriptor.ImplementationInstance, Is.Null);
        Assert.That(mainWindowDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(mainWindowDescriptor.ImplementationType, Is.EqualTo(typeof(MainWindow)));

        applicationViewModelDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Not.Null);
        Assert.That(applicationViewModelDescriptor, Is.EqualTo(services[31]));
        Assert.That(applicationViewModelDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationViewModelDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationViewModelDescriptor.ImplementationType, Is.EqualTo(typeof(ApplicationViewModel)));
    }

    [Test]
    public void BuildServiceProvider_WithScopeAndOnBuildValidation_DoesNotThrow()
    {
        ServiceCollection services = new();
        ConfigureServices(services);

        ServiceProviderOptions options = new() { ValidateScopes = true, ValidateOnBuild = true };

        ServiceProvider? serviceProvider = null;

        try
        {
            // ValidateOnBuild builds (but does not invoke) every call site, so the IPersistenceContext factory is
            // not run and no SQLite database is created. ValidateScopes guards against captive dependencies.
            Assert.DoesNotThrow(() => serviceProvider = services.BuildServiceProvider(options));
            Assert.That(serviceProvider, Is.Not.Null);
        }
        finally
        {
            serviceProvider?.Dispose();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        IConfigurationBuilder builder =
            new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        // Mirrors App.ConfigureServices:
        // Only the logging abstractions are registered (AddLogging) — the real Serilog file sink and console
        // provider from production are omitted so tests perform no file I/O and never mutate the global Log.Logger.
        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddInfrastructure();
        services.AddDomain();
        services.AddApplication();
        services.AddUi();
    }
}
