namespace PhotoManager.Domain;

public class FindDuplicatedAssetsService(IAssetRepository assetRepository, IStorageService storageService, IUserConfigurationService userConfigurationService) : IFindDuplicatedAssetsService
{
    private readonly IAssetRepository _assetRepository = assetRepository;
    private readonly IStorageService _storageService = storageService;
    private readonly IUserConfigurationService _userConfigurationService = userConfigurationService;

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets()
    {
        List<List<Asset>> duplicatedAssetsSets = [];
        List<Asset> assets = new (_assetRepository.GetCataloguedAssets());

        if (_userConfigurationService.AssetSettings.DetectThumbnails && _userConfigurationService.HashSettings.UsingPHash)
        {
            return GetDuplicatesBetweenOriginalAndThumbnail(assets, _userConfigurationService.HashSettings.PHashThreshold);
        }

        List<IGrouping<string, Asset>> assetGroups = assets.GroupBy(a => a.Hash).Where(g => g.Count() > 1).ToList();

        foreach (IGrouping<string, Asset> group in assetGroups)
        {
            List<Asset> duplicatedSet = [.. group];
            duplicatedSet.RemoveAll(asset => !_storageService.FileExists(asset.FullPath));

            if (duplicatedSet.Count > 1)
            {
                duplicatedAssetsSets.Add(duplicatedSet);
                _storageService.UpdateAssetsFileDateTimeProperties(duplicatedSet);
            }
        }

        return duplicatedAssetsSets;
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 36/210 => the more accurate
    // DHash the hammingDistance is 16/17
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    private List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnail(List<Asset> assets, ushort threshold)
    {
        List<List<Asset>> duplicatedAssetsSets = [];

        // Create a dictionary to store assets by their Hash values
        Dictionary<string, List<Asset>> assetDictionary = [];

        assets.RemoveAll(asset => !_storageService.FileExists(asset.FullPath));

        for (int i = 0; i < assets.Count; i++)
        {
            Asset asset1 = assets[i];
            string hash1 = asset1.Hash;

            // Create a flag to check if the asset is already added to any set
            bool addedToSet = false;

            // Check if the asset dictionary contains entries with similar Hash values
            foreach (KeyValuePair<string, List<Asset>> kvp in assetDictionary)
            {
                string hash2 = kvp.Key;

                // Calculate the Hamming distance between the Hash values
                int hammingDistance = HashingHelper.CalculateHammingDistance(hash1, hash2);

                // If the Hamming distance is below the threshold, add the asset to the duplicates list
                if (hammingDistance <= threshold)
                {
                    if (!kvp.Value.Contains(asset1))
                    {
                        kvp.Value.Add(asset1);
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
                _storageService.UpdateAssetsFileDateTimeProperties(assetSet);
                duplicatedAssetsSets.Add(assetSet);
            }
        }

        return duplicatedAssetsSets;
    }
}
