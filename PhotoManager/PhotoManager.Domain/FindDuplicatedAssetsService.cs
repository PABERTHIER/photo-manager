namespace PhotoManager.Domain;

public class FindDuplicatedAssetsService : IFindDuplicatedAssetsService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;

    public FindDuplicatedAssetsService(IAssetRepository assetRepository, IStorageService storageService)
    {
        _assetRepository = assetRepository;
        _storageService = storageService;
    }

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets()
    {
#pragma warning disable CS0162 // Unreachable code detected
        List<List<Asset>> duplicatedAssetsSets = new();
        List<Asset> assets = new(_assetRepository.GetCataloguedAssets());

        if (AssetConstants.DetectThumbnails && AssetConstants.UsingPHash)
        {
            return GetDuplicatesBetweenOriginalAndThumbnail(assets, AssetConstants.PHashThreshold);
        }

        var assetGroups = assets.GroupBy(a => a.Hash).Where(g => g.Count() > 1).ToList();

        foreach (var group in assetGroups)
        {
            List<Asset> duplicatedSet = group.ToList();
            duplicatedSet.RemoveAll(asset => !_storageService.FileExists(asset.FullPath));

            if (duplicatedSet.Count > 1)
            {
                duplicatedAssetsSets.Add(duplicatedSet);
                LoadFileInformation(duplicatedSet);
            }
        }

        return duplicatedAssetsSets;
#pragma warning restore CS0162 // Unreachable code detected
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 36/210 => the more accurate
    // DHash the hammingDistance is 16/17
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    protected List<List<Asset>> GetDuplicatesBetweenOriginalAndThumbnail(List<Asset> assets, int threshold)
    {
        List<List<Asset>> duplicatedAssetsSets = new();

        // Create a dictionary to store assets by their Hash values
        Dictionary<string, List<Asset>> assetDictionary = new();

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
                assetDictionary[hash1] = new List<Asset> { asset1 };
            }
        }

        // Create the result list of duplicated asset sets
        foreach (List<Asset> assetSet in assetDictionary.Values)
        {
            if (assetSet.Count > 1)
            {
                LoadFileInformation(assetSet);
                duplicatedAssetsSets.Add(assetSet);
            }
        }

        return duplicatedAssetsSets;
    }

    private void LoadFileInformation(List<Asset> duplicatedSet)
    {
        foreach (Asset asset in duplicatedSet)
        {
            _storageService.LoadFileInformation(asset);
        }
    }
}
