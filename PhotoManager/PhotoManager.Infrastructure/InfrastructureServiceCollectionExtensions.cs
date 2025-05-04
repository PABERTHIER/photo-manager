using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddDatabase();
        services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IAssetRepository, AssetRepository>();
        services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
    }

    private static void AddDatabase(this IServiceCollection services)
    {
        services.AddSingleton<IObjectListStorage, ObjectListStorage>();
        services.AddSingleton<IBlobStorage, BlobStorage>();
        services.AddSingleton<IBackupStorage, BackupStorage>();
        services.AddSingleton<IDatabase, Database.Database>();
    }
}
