using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Persistence;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class PersistenceServiceCollectionExtensionsTests
{
    [Test]
    public void AddPersistence_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? persistenceContextDescriptor =
            services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        services.AddPersistence();

        Assert.That(services, Has.Count.EqualTo(1));

        persistenceContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IPersistenceContext));
        Assert.That(persistenceContextDescriptor, Is.Not.Null);
        Assert.That(persistenceContextDescriptor, Is.EqualTo(services[0]));
        Assert.That(persistenceContextDescriptor.ImplementationInstance, Is.Null);
        Assert.That(persistenceContextDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(persistenceContextDescriptor.ImplementationType, Is.EqualTo(typeof(SqlitePersistenceContext)));
    }
}
