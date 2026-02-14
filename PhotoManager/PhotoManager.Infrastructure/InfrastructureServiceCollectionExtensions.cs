using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructure()
        {
            services.AddDatabase();
            services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IAssetRepository, AssetRepository>();
            services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
        }

        private void AddDatabase()
        {
            services.AddSingleton<IObjectListStorage, ObjectListStorage>();
            services.AddSingleton<IBlobStorage, BlobStorage>();
            services.AddSingleton<IBackupStorage, BackupStorage>();
            services.AddSingleton<IDatabase, Database.Database>();
        }
    }
}
