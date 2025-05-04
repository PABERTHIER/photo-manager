using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Application;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationServiceCollectionExtensionsTests
{
    [Test]
    public void AddApplication_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        services.AddApplication();

        Assert.That(services, Has.Count.EqualTo(1));

        applicationDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IApplication));
        Assert.That(applicationDescriptor, Is.Not.Null);
        Assert.That(applicationDescriptor, Is.EqualTo(services[0]));
        Assert.That(applicationDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationDescriptor.ImplementationType, Is.EqualTo(typeof(PhotoManager.Application.Application)));
    }
}
