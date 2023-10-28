namespace PhotoManager.Tests.Integration.Domain.FindDuplicatedAssets;

public class TestableFindDuplicatedAssetsService : FindDuplicatedAssetsService
{
    public List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnailTestable(List<Asset> assets, int threshold) => GetDuplicatesBetweenOriginalAndThumbnail(assets, threshold);

    public TestableFindDuplicatedAssetsService(IAssetRepository assetRepository, IStorageService storageService) : base(assetRepository, storageService)
    {
    }
}
