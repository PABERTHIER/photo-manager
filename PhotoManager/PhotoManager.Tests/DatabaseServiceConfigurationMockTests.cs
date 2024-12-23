using Autofac;

namespace PhotoManager.Tests;

[TestFixture]
public class DatabaseServiceConfigurationMockTests
{
    [Test]
    public void RegisterDatabaseTypes_RegistersAllRequiredTypes()
    {
        ContainerBuilder containerBuilder = new();

        containerBuilder.RegisterDatabaseTypes();
        IContainer container = containerBuilder.Build();

        Assert.That(container.IsRegistered<IObjectListStorage>(), Is.True);
        Assert.That(container.IsRegistered<IBlobStorage>(), Is.True);
        Assert.That(container.IsRegistered<IBackupStorage>(), Is.True);
        Assert.That(container.IsRegistered<IDatabase>(), Is.True);
    }
}
