using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IPersistenceContext, SqlitePersistenceContext>();
        }
    }
}
