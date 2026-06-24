using PhotoManager.Domain.Interfaces.Persistence;

namespace PhotoManager.Tests;

public static class UserConfigurationServiceTestExtensions
{
    extension(IConfigurationRoot configuration)
    {
        // Builds a UserConfigurationService for tests that only exercise the JSON-backed configuration.
        // Persistence is a required dependency of the service; tests that do not exercise persisted settings
        // supply a substitute whose Configuration table is empty, so the service reads from this IConfigurationRoot.
        public UserConfigurationService CreateUserConfigurationService()
        {
            IPersistenceContext persistenceContext = Substitute.For<IPersistenceContext>();
            persistenceContext.Configuration.GetAll().Returns(new Dictionary<string, string>());

            return new UserConfigurationService(configuration, persistenceContext);
        }
    }
}
