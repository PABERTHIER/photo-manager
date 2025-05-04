using Microsoft.Extensions.DependencyInjection;
using PhotoManager.UI;
using PhotoManager.UI.Windows;

namespace PhotoManager.Tests.Integration.UI;

[TestFixture]
public class UiServiceCollectionExtensionsTests
{
    [Test]
    public void AddUi_RegistersServicesAsSingleton()
    {
        ServiceCollection services = new();

        ServiceDescriptor? mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Null);

        ServiceDescriptor? applicationViewModelDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Null);

        Assert.That(services, Is.Empty);

        services.AddUi();

        Assert.That(services, Has.Count.EqualTo(2));

        mainWindowDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MainWindow));
        Assert.That(mainWindowDescriptor, Is.Not.Null);
        Assert.That(mainWindowDescriptor, Is.EqualTo(services[0]));
        Assert.That(mainWindowDescriptor.ImplementationInstance, Is.Null);
        Assert.That(mainWindowDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(mainWindowDescriptor.ImplementationType, Is.EqualTo(typeof(MainWindow)));

        applicationViewModelDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ApplicationViewModel));
        Assert.That(applicationViewModelDescriptor, Is.Not.Null);
        Assert.That(applicationViewModelDescriptor, Is.EqualTo(services[1]));
        Assert.That(applicationViewModelDescriptor.ImplementationInstance, Is.Null);
        Assert.That(applicationViewModelDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
        Assert.That(applicationViewModelDescriptor.ImplementationType, Is.EqualTo(typeof(ApplicationViewModel)));
    }
}
