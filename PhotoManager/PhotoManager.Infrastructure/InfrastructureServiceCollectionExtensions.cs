using Microsoft.Extensions.DependencyInjection;
using PhotoManager.Persistence;

namespace PhotoManager.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructure()
        {
            services.AddSingleton<IPathProviderService, PathProviderService>();

            services.AddPersistence();

            services.AddSingleton<IUserConfigurationService, UserConfigurationService>();
            services.AddSingleton<IFileOperationsService, FileOperationsService>();
            services.AddSingleton<IImageProcessingService, ImageProcessingService>();
            services.AddSingleton<IThumbnailGenerator, ImageMagickThumbnailGenerator>();
            services.AddSingleton<IImageMetadataService, ImageMetadataService>();
            services.AddSingleton<IAssetRepository, AssetRepository>();
            services.AddSingleton<IAssetHashCalculatorService, AssetHashCalculatorService>();
        }
    }
}
