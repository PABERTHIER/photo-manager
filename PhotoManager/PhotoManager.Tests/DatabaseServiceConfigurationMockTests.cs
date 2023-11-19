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

        Assert.IsTrue(container.IsRegistered<IObjectListStorage>());
        Assert.IsTrue(container.IsRegistered<IBlobStorage>());
        Assert.IsTrue(container.IsRegistered<IBackupStorage>());
        Assert.IsTrue(container.IsRegistered<IDatabase>());
    }
}
