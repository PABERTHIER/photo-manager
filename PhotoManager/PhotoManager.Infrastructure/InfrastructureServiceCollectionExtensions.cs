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
            services.AddSingleton<IPathProviderService, PathProviderService>();
            services.AddSingleton<IFileOperationsService, FileOperationsService>();
            services.AddSingleton<IImageProcessingService, ImageProcessingService>();
            services.AddSingleton<IImageMetadataService, ImageMetadataService>();
            services.AddSingleton<IAssetRepository, AssetRepository>();
            services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
        }

        // Known issue, see: https://github.com/dotnet/roslyn/issues/82691
#pragma warning disable IDE0051
        private void AddDatabase()
#pragma warning restore IDE0051
        {
            services.AddSingleton<IObjectListStorage, ObjectListStorage>();
            services.AddSingleton<IBlobStorage, BlobStorage>();
            services.AddSingleton<IBackupStorage, BackupStorage>();
            services.AddSingleton<IDatabase, Database.Database>();
        }
    }
}
