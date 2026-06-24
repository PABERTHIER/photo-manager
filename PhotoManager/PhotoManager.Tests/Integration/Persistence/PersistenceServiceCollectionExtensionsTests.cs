using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Persistence;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Persistence;

[TestFixture]
public class PersistenceServiceCollectionExtensionsTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

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
        // The context is registered via a factory (it self-initializes at the composition root), so it exposes an
        // ImplementationFactory rather than an ImplementationType.
        Assert.That(persistenceContextDescriptor.ImplementationType, Is.Null);
        Assert.That(persistenceContextDescriptor.ImplementationFactory, Is.Not.Null);
    }

    [Test]
    public void AddPersistence_ResolvedPersistenceContext_CreatesInitializedSingletonContext()
    {
        ServiceCollection services = new();
        IPathProviderService pathProviderService = Substitute.For<IPathProviderService>();
        pathProviderService.ResolveDatabaseDirectory().Returns(_databaseDirectory);

        services.AddPersistence();
        services.AddSingleton(pathProviderService);
        services.AddSingleton<ILogger<SqliteConnectionFactory>>(new TestLogger<SqliteConnectionFactory>());
        services.AddSingleton<ILogger<SqlitePersistenceContext>>(new TestLogger<SqlitePersistenceContext>());

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            IPersistenceContext persistenceContext = serviceProvider.GetRequiredService<IPersistenceContext>();

            Assert.That(persistenceContext, Is.InstanceOf<SqlitePersistenceContext>());
            Assert.That(persistenceContext.Configuration, Is.Not.Null);
            Assert.That(File.Exists(Path.Combine(_databaseDirectory!, SqlitePersistenceContext.DATABASE_FILE_NAME)),
                Is.True);

            // The factory runs once and the same instance is returned: the context is a self-initializing singleton.
            Assert.That(serviceProvider.GetRequiredService<IPersistenceContext>(), Is.SameAs(persistenceContext));

            pathProviderService.Received(1).ResolveDatabaseDirectory();
        }
    }
}
