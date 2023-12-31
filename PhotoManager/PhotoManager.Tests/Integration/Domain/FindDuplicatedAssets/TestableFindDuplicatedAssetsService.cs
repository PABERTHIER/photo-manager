namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

public class TestableFindDuplicatedAssetsService(IAssetRepository assetRepository, IStorageService storageService, IUserConfigurationService userConfigurationService) :
    FindDuplicatedAssetsService(assetRepository, storageService, userConfigurationService)
{
    public List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnailTestable(List<Asset> assets, ushort threshold) => GetDuplicatesBetweenOriginalAndThumbnail(assets, threshold);
}
