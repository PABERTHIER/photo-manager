namespace PhotoManager.Domain;

public class FindDuplicatedAssetsService(IAssetRepository assetRepository, IStorageService storageService, IUserConfigurationService userConfigurationService) : IFindDuplicatedAssetsService
{
    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets()
    {
        List<List<Asset>> duplicatedAssetsSets = [];
        List<Asset> assets = assetRepository.GetCataloguedAssets();

        if (userConfigurationService.AssetSettings.DetectThumbnails && userConfigurationService.HashSettings.UsingPHash)
        {
            return GetDuplicatesBetweenOriginalAndThumbnail(assets, userConfigurationService.HashSettings.PHashThreshold);
        }

        List<IGrouping<string, Asset>> assetGroups = assets.GroupBy(a => a.Hash).Where(g => g.Count() > 1).ToList();

        foreach (IGrouping<string, Asset> group in assetGroups)
        {
            List<Asset> duplicatedSet = [..group];
            duplicatedSet.RemoveAll(asset => !storageService.FileExists(asset.FullPath));

            if (duplicatedSet.Count > 1)
            {
                duplicatedAssetsSets.Add(duplicatedSet);
            }
        }

        return duplicatedAssetsSets;
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 10/210 => the most accurate
    // DHash the hammingDistance is 5/14
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    private List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnail(List<Asset> assets, ushort threshold)
    {
        List<List<Asset>> duplicatedAssetsSets = [];

        // Create a dictionary to store assets by their Hash values
        Dictionary<string, List<Asset>> assetDictionary = [];

        assets.RemoveAll(asset => !storageService.FileExists(asset.FullPath));

        for (int i = 0; i < assets.Count; i++)
        {
            Asset asset1 = assets[i];
            string hash1 = asset1.Hash;

            // Create a flag to check if the asset is already added to any set
            bool addedToSet = false;

            // Check if the asset dictionary contains entries with similar Hash values
            foreach ((string? hash2, List<Asset>? assetsAdded) in assetDictionary)
            {
                // Calculate the Hamming distance between the Hash values
                int hammingDistance = HashingHelper.CalculateHammingDistance(hash1, hash2);

                // If the Hamming distance is below the threshold, add the asset to the duplicates list
                if (hammingDistance <= threshold)
                {
                    if (!assetsAdded.Contains(asset1))
                    {
                        assetsAdded.Add(asset1);
                        addedToSet = true;
                    }
                }
            }

            // If the asset is not in the dictionary and hasn't been added to any set, create a new entry
            if (!assetDictionary.ContainsKey(hash1) && !addedToSet)
            {
                assetDictionary[hash1] = [asset1];
            }
        }

        // Create the result list of duplicated asset sets
        foreach (List<Asset> assetSet in assetDictionary.Values)
        {
            if (assetSet.Count > 1)
            {
                duplicatedAssetsSets.Add(assetSet);
            }
        }

        return duplicatedAssetsSets;
    }
}
