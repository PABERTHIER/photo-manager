using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Domain.Interfaces.Persistence;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class InfrastructureServiceCollectionExtensionsTests
{
    [Test]
    public void AddInfrastructure_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? pathProviderServiceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IPathProviderService));
        Assert.That(pathProviderServiceDescriptor, Is.Null);

        ServiceDescriptor? sqliteConnectionFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(sqliteConnectionFactoryDescriptor, Is.Null);

        ServiceDescriptor? sqliteBackupServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(sqliteBackupServiceDescriptor, Is.Null);

        ServiceDescriptor? persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Null);

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

        Assert.That(services, Is.Empty);

        services.AddInfrastructure();

        Assert.That(services, Has.Count.EqualTo(11));

        pathProviderServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IPathProviderService));
        Assert.That(pathProviderServiceDescriptor, Is.Not.Null);
        Assert.That(pathProviderServiceDescriptor, Is.EqualTo(services[0]));
        Assert.That(pathProviderServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(pathProviderServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(pathProviderServiceDescriptor.ImplementationType, Is.EqualTo(typeof(PathProviderService)));

        sqliteConnectionFactoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(sqliteConnectionFactoryDescriptor, Is.Not.Null);
        Assert.That(sqliteConnectionFactoryDescriptor, Is.EqualTo(services[1]));
        Assert.That(sqliteConnectionFactoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(sqliteConnectionFactoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(sqliteConnectionFactoryDescriptor.ImplementationType,
            Is.EqualTo(typeof(SqliteConnectionFactory)));

        sqliteBackupServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(sqliteBackupServiceDescriptor, Is.Not.Null);
        Assert.That(sqliteBackupServiceDescriptor, Is.EqualTo(services[2]));
        Assert.That(sqliteBackupServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(sqliteBackupServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(sqliteBackupServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(SqliteBackupService)));

        persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Not.Null);
        Assert.That(persistenceContextDescriptor, Is.EqualTo(services[3]));
        Assert.That(persistenceContextDescriptor.ImplementationInstance, Is.Null);
        Assert.That(persistenceContextDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        // Registered via a factory (the context self-initializes at the composition root).
        Assert.That(persistenceContextDescriptor.ImplementationType, Is.Null);
        Assert.That(persistenceContextDescriptor.ImplementationFactory, Is.Not.Null);

        userConfigurationServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IUserConfigurationService));
        Assert.That(userConfigurationServiceDescriptor, Is.Not.Null);
        Assert.That(userConfigurationServiceDescriptor, Is.EqualTo(services[4]));
        Assert.That(userConfigurationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(userConfigurationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(userConfigurationServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(UserConfigurationService)));

        fileOperationsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IFileOperationsService));
        Assert.That(fileOperationsServiceDescriptor, Is.Not.Null);
        Assert.That(fileOperationsServiceDescriptor, Is.EqualTo(services[5]));
        Assert.That(fileOperationsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(fileOperationsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(fileOperationsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(FileOperationsService)));

        imageProcessingServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IImageProcessingService));
        Assert.That(imageProcessingServiceDescriptor, Is.Not.Null);
        Assert.That(imageProcessingServiceDescriptor, Is.EqualTo(services[6]));
        Assert.That(imageProcessingServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(imageProcessingServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(imageProcessingServiceDescriptor.ImplementationType, Is.EqualTo(typeof(ImageProcessingService)));

        thumbnailGeneratorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IThumbnailGenerator));
        Assert.That(thumbnailGeneratorDescriptor, Is.Not.Null);
        Assert.That(thumbnailGeneratorDescriptor, Is.EqualTo(services[7]));
        Assert.That(thumbnailGeneratorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(thumbnailGeneratorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(thumbnailGeneratorDescriptor.ImplementationType, Is.EqualTo(typeof(ImageMagickThumbnailGenerator)));

        imageMetadataServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IImageMetadataService));
        Assert.That(imageMetadataServiceDescriptor, Is.Not.Null);
        Assert.That(imageMetadataServiceDescriptor, Is.EqualTo(services[8]));
        Assert.That(imageMetadataServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(imageMetadataServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(imageMetadataServiceDescriptor.ImplementationType, Is.EqualTo(typeof(ImageMetadataService)));

        assetRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetRepository));
        Assert.That(assetRepositoryDescriptor, Is.Not.Null);
        Assert.That(assetRepositoryDescriptor, Is.EqualTo(services[9]));
        Assert.That(assetRepositoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetRepositoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetRepositoryDescriptor.ImplementationType, Is.EqualTo(typeof(AssetRepository)));

        assetHashCalculatorServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetHashCalculatorService));
        Assert.That(assetHashCalculatorServiceDescriptor, Is.Not.Null);
        Assert.That(assetHashCalculatorServiceDescriptor, Is.EqualTo(services[10]));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetHashCalculatorServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetHashCalculatorServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(AssetHashCalculatorService)));
    }
}
