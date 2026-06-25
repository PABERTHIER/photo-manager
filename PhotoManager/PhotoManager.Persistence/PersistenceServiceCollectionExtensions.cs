using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Domain.Interfaces.Persistence;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPersistence()
        {
            services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
            services.AddSingleton<ISqliteBackupService, SqliteBackupService>();

            // The context owns its own initialization: it is created and initialized once here, at the composition
            // root, so every consumer (UserConfigurationService, AssetRepository, ...) receives a ready context.
            // This avoids any consumer having to call Initialize and keeps the database bootstrap in a single place.
            services.AddSingleton<IPersistenceContext>(serviceProvider =>
            {
                SqlitePersistenceContext context = new(
                    serviceProvider.GetRequiredService<ISqliteConnectionFactory>(),
                    serviceProvider.GetRequiredService<ISqliteBackupService>(),
                    serviceProvider.GetRequiredService<ILogger<SqlitePersistenceContext>>());
                context.Initialize(
                    serviceProvider.GetRequiredService<IPathProviderService>().ResolveDatabaseDirectory());

                return context;
            });
        }
    }
}
