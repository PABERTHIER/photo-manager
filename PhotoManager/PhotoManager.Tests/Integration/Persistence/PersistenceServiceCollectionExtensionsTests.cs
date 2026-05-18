using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Persistence;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class PersistenceServiceCollectionExtensionsTests
{
    [Test]
    public void AddPersistence_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? factoryDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(factoryDescriptor, Is.Null);

        ServiceDescriptor? backupServiceDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(backupServiceDescriptor, Is.Null);

        ServiceDescriptor? persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        services.AddPersistence();

        Assert.That(services, Has.Count.EqualTo(3));

        factoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteConnectionFactory));
        Assert.That(factoryDescriptor, Is.Not.Null);
        Assert.That(factoryDescriptor, Is.EqualTo(services[0]));
        Assert.That(factoryDescriptor.ImplementationInstance, Is.Null);
        Assert.That(factoryDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(factoryDescriptor.ImplementationType, Is.EqualTo(typeof(SqliteConnectionFactory)));

        backupServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISqliteBackupService));
        Assert.That(backupServiceDescriptor, Is.Not.Null);
        Assert.That(backupServiceDescriptor, Is.EqualTo(services[1]));
        Assert.That(backupServiceDescriptor.ImplementationInstance, Is.Null);
        Assert.That(backupServiceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(backupServiceDescriptor.ImplementationType, Is.EqualTo(typeof(SqliteBackupService)));

        persistenceContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Not.Null);
        Assert.That(persistenceContextDescriptor, Is.EqualTo(services[2]));
        Assert.That(persistenceContextDescriptor.ImplementationInstance, Is.Null);
        Assert.That(persistenceContextDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(persistenceContextDescriptor.ImplementationType, Is.EqualTo(typeof(SqlitePersistenceContext)));
    }
}
