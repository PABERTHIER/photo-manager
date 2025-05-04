using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Application;
using PhotoManager.UI;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI;

// For STA concern and WPF resources initialization issues, the best choice has been to "mock" App
// The goal is to test what does App
[TestFixture]
public class AppTests
{
    [Test]
    public void ConfigureServices_RegistersAllServicesAsSingleton()
    {
        ServiceCollection services = new();

        // appsettings.json
        ServiceDescriptor? configurationRootDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IConfigurationRoot));
        Assert.That(configurationRootDescriptor, Is.Null);

        // Infrastructure
        ServiceDescriptor? objectListStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IObjectListStorage));
        Assert.That(objectListStorageDescriptor, Is.Null);

        ServiceDescriptor? blobStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBlobStorage));
        Assert.That(blobStorageDescriptor, Is.Null);

        ServiceDescriptor? backupStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBackupStorage));
        Assert.That(backupStorageDescriptor, Is.Null);

        ServiceDescriptor? databaseDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IDatabase));
        Assert.That(databaseDescriptor, Is.Null);

        ServiceDescriptor? userConfigurationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Null);

        ServiceDescriptor? storageServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IStorageService));
        Assert.That(storageServiceDescriptor, Is.Null);

        ServiceDescriptor? assetRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Null);

        ServiceDescriptor? assetHashCalculatorServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Null);

        // Domain
        ServiceDescriptor? assetsComparatorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Null);

        ServiceDescriptor? assetCreationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Null);

        ServiceDescriptor? catalogAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? findDuplicatedAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? moveAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? syncAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Null);

        // Application
        ServiceDescriptor? applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Null);

        // UI
        ServiceDescriptor? mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Null);

        ServiceDescriptor? applicationViewModelDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        ConfigureServices(services);

        Assert.That(services, Has.Count.EqualTo(18));

        // appsettings.json
        configurationRootDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IConfigurationRoot));
        Assert.That(configurationRootDescriptor, Is.Not.Null);
        Assert.That(configurationRootDescriptor, Is.EqualTo(services[0]));
        Assert.That(configurationRootDescriptor.ImplementationInstance, Is.Not.Null);
        Assert.That(configurationRootDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(configurationRootDescriptor.ImplementationType, Is.Null);

        // Infrastructure
        objectListStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IObjectListStorage));
        Assert.That(objectListStorageDescriptor, Is.Not.Null);
        Assert.That(objectListStorageDescriptor, Is.EqualTo(services[1]));
        Assert.That(objectListStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(objectListStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(objectListStorageDescriptor.ImplementationType, Is.EqualTo(typeof(ObjectListStorage)));

        blobStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBlobStorage));
        Assert.That(blobStorageDescriptor, Is.Not.Null);
        Assert.That(blobStorageDescriptor, Is.EqualTo(services[2]));
        Assert.That(blobStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(blobStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(blobStorageDescriptor.ImplementationType, Is.EqualTo(typeof(BlobStorage)));

        backupStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBackupStorage));
        Assert.That(backupStorageDescriptor, Is.Not.Null);
        Assert.That(backupStorageDescriptor, Is.EqualTo(services[3]));
        Assert.That(backupStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(backupStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(backupStorageDescriptor.ImplementationType, Is.EqualTo(typeof(BackupStorage)));

        databaseDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IDatabase));
        Assert.That(databaseDescriptor, Is.Not.Null);
        Assert.That(databaseDescriptor, Is.EqualTo(services[4]));
        Assert.That(databaseDescriptor.ImplementationInstance, Is.Null);
        Assert.That(databaseDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(databaseDescriptor.ImplementationType, Is.EqualTo(typeof(Database)));

        userConfigurationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Not.Null);
        Assert.That(userConfigurationServiceDescriptor, Is.EqualTo(services[5]));
        Assert.That(userConfigurationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(userConfigurationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(userConfigurationServiceDescriptor.ImplementationType, Is.EqualTo(typeof(UserConfigurationService)));

        storageServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IStorageService));
        Assert.That(storageServiceDescriptor, Is.Not.Null);
        Assert.That(storageServiceDescriptor, Is.EqualTo(services[6]));
        Assert.That(storageServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(storageServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(storageServiceDescriptor.ImplementationType, Is.EqualTo(typeof(StorageService)));

        assetRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Not.Null);
        Assert.That(assetRepositoryDescriptor, Is.EqualTo(services[7]));
        Assert.That(assetRepositoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(AssetRepository)));

        assetHashCalculatorServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Not.Null);
        Assert.That(assetHashCalculatorServiceDescriptor, Is.EqualTo(services[8]));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetHashCalculatorServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetHashCalculatorService)));

        // Domain
        assetsComparatorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Not.Null);
        Assert.That(assetsComparatorDescriptor, Is.EqualTo(services[9]));
        Assert.That(assetsComparatorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetsComparatorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetsComparatorDescriptor.ImplementationType, Is.EqualTo(typeof(AssetsComparator)));

        assetCreationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Not.Null);
        Assert.That(assetCreationServiceDescriptor, Is.EqualTo(services[10]));
        Assert.That(assetCreationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetCreationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetCreationServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetCreationService)));

        catalogAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(catalogAssetsServiceDescriptor, Is.EqualTo(services[11]));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(catalogAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(CatalogAssetsService)));

        findDuplicatedAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.EqualTo(services[12]));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(FindDuplicatedAssetsService)));

        moveAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(moveAssetsServiceDescriptor, Is.EqualTo(services[13]));
        Assert.That(moveAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(moveAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(moveAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(MoveAssetsService)));

        syncAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(syncAssetsServiceDescriptor, Is.EqualTo(services[14]));
        Assert.That(syncAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(syncAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(syncAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(SyncAssetsService)));

        // Application
        applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Not.Null);
        Assert.That(applicationDescriptor, Is.EqualTo(services[15]));
        Assert.That(applicationDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationDescriptor.ImplementationType, Is.EqualTo(typeof(PhotoManager.Application.Application)));

        // UI
        mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Not.Null);
        Assert.That(mainWindowDescriptor, Is.EqualTo(services[16]));
        Assert.That(mainWindowDescriptor.ImplementationInstance, Is.Null);
        Assert.That(mainWindowDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(mainWindowDescriptor.ImplementationType, Is.EqualTo(typeof(MainWindow)));

        applicationViewModelDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Not.Null);
        Assert.That(applicationViewModelDescriptor, Is.EqualTo(services[17]));
        Assert.That(applicationViewModelDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationViewModelDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationViewModelDescriptor.ImplementationType, Is.EqualTo(typeof(ApplicationViewModel)));
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
