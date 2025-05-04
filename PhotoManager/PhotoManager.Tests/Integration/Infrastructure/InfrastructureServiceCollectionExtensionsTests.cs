using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class InfrastructureServiceCollectionExtensionsTests
{
    [Test]
    public void AddInfrastructure_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

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

        Assert.That(services, Is.Empty);

        services.AddInfrastructure();

        Assert.That(services, Has.Count.EqualTo(8));

        objectListStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IObjectListStorage));
        Assert.That(objectListStorageDescriptor, Is.Not.Null);
        Assert.That(objectListStorageDescriptor, Is.EqualTo(services[0]));
        Assert.That(objectListStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(objectListStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(objectListStorageDescriptor.ImplementationType, Is.EqualTo(typeof(ObjectListStorage)));

        blobStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBlobStorage));
        Assert.That(blobStorageDescriptor, Is.Not.Null);
        Assert.That(blobStorageDescriptor, Is.EqualTo(services[1]));
        Assert.That(blobStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(blobStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(blobStorageDescriptor.ImplementationType, Is.EqualTo(typeof(BlobStorage)));

        backupStorageDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IBackupStorage));
        Assert.That(backupStorageDescriptor, Is.Not.Null);
        Assert.That(backupStorageDescriptor, Is.EqualTo(services[2]));
        Assert.That(backupStorageDescriptor.ImplementationInstance, Is.Null);
        Assert.That(backupStorageDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(backupStorageDescriptor.ImplementationType, Is.EqualTo(typeof(BackupStorage)));

        databaseDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IDatabase));
        Assert.That(databaseDescriptor, Is.Not.Null);
        Assert.That(databaseDescriptor, Is.EqualTo(services[3]));
        Assert.That(databaseDescriptor.ImplementationInstance, Is.Null);
        Assert.That(databaseDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(databaseDescriptor.ImplementationType, Is.EqualTo(typeof(PhotoManager.Infrastructure.Database.Database)));

        userConfigurationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Not.Null);
        Assert.That(userConfigurationServiceDescriptor, Is.EqualTo(services[4]));
        Assert.That(userConfigurationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(userConfigurationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(userConfigurationServiceDescriptor.ImplementationType, Is.EqualTo(typeof(UserConfigurationService)));

        storageServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IStorageService));
        Assert.That(storageServiceDescriptor, Is.Not.Null);
        Assert.That(storageServiceDescriptor, Is.EqualTo(services[5]));
        Assert.That(storageServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(storageServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(storageServiceDescriptor.ImplementationType, Is.EqualTo(typeof(StorageService)));

        assetRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Not.Null);
        Assert.That(assetRepositoryDescriptor, Is.EqualTo(services[6]));
        Assert.That(assetRepositoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(AssetRepository)));

        assetHashCalculatorServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Not.Null);
        Assert.That(assetHashCalculatorServiceDescriptor, Is.EqualTo(services[7]));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetHashCalculatorServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetHashCalculatorService)));
    }
}
