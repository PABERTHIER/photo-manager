using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Tests.Integration.Domain;

[TestFixture]
public class DomainServiceCollectionExtensionsTests
{
    [Test]
    public void AddDomain_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? assetsComparatorDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Null);

        ServiceDescriptor? assetCreationServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Null);

        ServiceDescriptor? catalogAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? findDuplicatedAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? moveAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Null);

        ServiceDescriptor? syncAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        services.AddDomain();

        Assert.That(services, Has.Count.EqualTo(6));

        assetsComparatorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetsComparator));
        Assert.That(assetsComparatorDescriptor, Is.Not.Null);
        Assert.That(assetsComparatorDescriptor, Is.EqualTo(services[0]));
        Assert.That(assetsComparatorDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetsComparatorDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetsComparatorDescriptor.ImplementationType, Is.EqualTo(typeof(AssetsComparator)));

        assetCreationServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAssetCreationService));
        Assert.That(assetCreationServiceDescriptor, Is.Not.Null);
        Assert.That(assetCreationServiceDescriptor, Is.EqualTo(services[1]));
        Assert.That(assetCreationServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(assetCreationServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(assetCreationServiceDescriptor.ImplementationType, Is.EqualTo(typeof(AssetCreationService)));

        catalogAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ICatalogAssetsService));
        Assert.That(catalogAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(catalogAssetsServiceDescriptor, Is.EqualTo(services[2]));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(catalogAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(catalogAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(CatalogAssetsService)));

        findDuplicatedAssetsServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IFindDuplicatedAssetsService));
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor, Is.EqualTo(services[3]));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(findDuplicatedAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(findDuplicatedAssetsServiceDescriptor.ImplementationType,
            Is.EqualTo(typeof(FindDuplicatedAssetsService)));

        moveAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMoveAssetsService));
        Assert.That(moveAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(moveAssetsServiceDescriptor, Is.EqualTo(services[4]));
        Assert.That(moveAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(moveAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(moveAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(MoveAssetsService)));

        syncAssetsServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISyncAssetsService));
        Assert.That(syncAssetsServiceDescriptor, Is.Not.Null);
        Assert.That(syncAssetsServiceDescriptor, Is.EqualTo(services[5]));
        Assert.That(syncAssetsServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(syncAssetsServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(syncAssetsServiceDescriptor.ImplementationType, Is.EqualTo(typeof(SyncAssetsService)));
    }
}
