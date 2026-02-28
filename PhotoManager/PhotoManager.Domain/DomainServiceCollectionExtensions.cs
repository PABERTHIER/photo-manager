using Microsoft.Extensions.DependencyInjection;

namespace PhotoManager.Domain;

public static class DomainServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddDomain()
        {
            services.AddSingleton<IAssetsComparator, AssetsComparator>();
            services.AddSingleton<IAssetCreationService, AssetCreationService>();
            services.AddSingleton<ICatalogAssetsService, CatalogAssetsService>();
            services.AddSingleton<IFindDuplicatedAssetsService, FindDuplicatedAssetsService>();
            services.AddSingleton<IMoveAssetsService, MoveAssetsService>();
            services.AddSingleton<ISyncAssetsService, SyncAssetsService>();
        }
    }
}
