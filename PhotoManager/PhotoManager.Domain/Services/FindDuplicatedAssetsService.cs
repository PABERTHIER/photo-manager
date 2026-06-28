using Microsoft.Extensions.Logging;

namespace PhotoManager.Domain.Services;

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
        Asset[] assets = assetRepository.GetCataloguedAssets();
        Array.Sort(assets, CompareByFileNameThenFolderPath);

        if (userConfigurationService.AssetSettings.DetectThumbnails && userConfigurationService.HashSettings.UsingPHash)
        {
            return GetDuplicatesBetweenOriginalAndThumbnail(assets,
                userConfigurationService.HashSettings.PHashThreshold);
        }

        return GetDuplicatesByExactHash(assets);
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 10/210 => the most accurate
    // DHash the hammingDistance is 5/14
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    private List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnail(Asset[] assets, ushort threshold)
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

        List<List<Asset>> duplicateGroups = [];

        foreach (List<Asset> group in bkTree.GetAllGroups())
        {
            if (group.Count > 1)
            {
                duplicateGroups.Add(group);
            }
        }

        return duplicateGroups;
    }

    private List<List<Asset>> GetDuplicatesByExactHash(Asset[] assets)
    {
        Dictionary<string, HashGroup> groupsByHash = new(assets.Length, StringComparer.Ordinal);
        List<HashGroup> duplicateGroupsInOrder = [];

        for (int i = 0; i < assets.Length; i++)
        {
            Asset asset = assets[i];

            if (!groupsByHash.TryGetValue(asset.Hash, out HashGroup? group))
            {
                group = new(asset);
                groupsByHash.Add(asset.Hash, group);
                continue;
            }

            if (group.Assets is null)
            {
                group.Assets = [group.FirstAsset];
                duplicateGroupsInOrder.Add(group);
            }

            group.Assets.Add(asset);
        }

        List<List<Asset>> duplicateGroups = [];

        for (int i = 0; i < duplicateGroupsInOrder.Count; i++)
        {
            List<Asset> group = duplicateGroupsInOrder[i].Assets!;
            List<Asset> existingAssets = new(group.Count);

            for (int j = 0; j < group.Count; j++)
            {
                Asset asset = group[j];

                if (fileOperationsService.FileExists(asset.FullPath))
                {
                    existingAssets.Add(asset);
                }
            }

            if (existingAssets.Count > 1)
            {
                duplicateGroups.Add(existingAssets);
            }
        }

        return duplicateGroups;
    }

    private static int CompareByFileNameThenFolderPath(Asset left, Asset right)
    {
        int fileNameComparison = string.CompareOrdinal(left.FileName, right.FileName);

        return fileNameComparison != 0
            ? fileNameComparison
            : string.CompareOrdinal(left.Folder.Path, right.Folder.Path);
    }

    private sealed class HashGroup(Asset firstAsset)
    {
        public Asset FirstAsset { get; } = firstAsset;
        public List<Asset>? Assets { get; set; }
    }
}
