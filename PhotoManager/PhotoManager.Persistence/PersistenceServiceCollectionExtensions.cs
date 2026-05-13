using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Persistence.Sqlite;

namespace PhotoManager.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPersistence()
        {
            services.AddSingleton<IPersistenceContext, SqlitePersistenceContext>();
        }
    }
}
