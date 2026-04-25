using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain;

public class FindDuplicatedAssetsService(
    IAssetRepository assetRepository,
    IFileOperationsService fileOperationsService,
    IUserConfigurationService userConfigurationService,
    ILogger<FindDuplicatedAssetsService> logger)
    : IFindDuplicatedAssetsService
{
    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets()
    {
        List<Asset> assets = assetRepository.GetCataloguedAssets();

        if (userConfigurationService.AssetSettings.DetectThumbnails && userConfigurationService.HashSettings.UsingPHash)
        {
            return GetDuplicatesBetweenOriginalAndThumbnail(assets,
                userConfigurationService.HashSettings.PHashThreshold);
        }

        List<Asset> validAssets = assets
            .AsParallel()
            .AsOrdered()
            .Where(asset => fileOperationsService.FileExists(asset.FullPath))
            .ToList();

        return
        [
            .. validAssets
                .GroupBy(a => a.Hash)
                .Where(g => g.Count() > 1)
                .Select(g => g.ToList())
        ];
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 10/210 => the most accurate
    // DHash the hammingDistance is 5/14
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    private List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnail(List<Asset> assets, ushort threshold)
    {
        List<Asset> validAssets = assets
            .AsParallel()
            .AsOrdered()
            .Where(asset => fileOperationsService.FileExists(asset.FullPath))
            .ToList();

        BkTree bkTree = new(logger);

        foreach (Asset asset in validAssets)
        {
            if (!bkTree.TryAddToExistingGroups(asset.Hash, asset, threshold))
            {
                bkTree.Insert(asset.Hash, asset);
            }
        }

        return [.. bkTree.GetAllGroups().Where(g => g.Count > 1)];
    }
}
