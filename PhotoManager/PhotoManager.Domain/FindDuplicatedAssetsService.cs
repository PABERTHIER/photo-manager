namespace PhotoManager.Domain;

public class FindDuplicatedAssetsService : IFindDuplicatedAssetsService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;

    public FindDuplicatedAssetsService(
        IAssetRepository assetRepository,
        IStorageService storageService)
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
        List<List<Asset>> result = new();
        List<Asset> assets = new(_assetRepository.GetCataloguedAssets());

        if (Constants.AssetConstants.DetectThumbnails && Constants.AssetConstants.UsingPHash)
        {
            return FindPHashDuplicates();
        }

        var assetGroups = assets.GroupBy(a => a.Hash).ToList();
        assetGroups = assetGroups.Where(g => g.Count() > 1).ToList();

        foreach (var group in assetGroups)
        {
            result.Add(group.ToList());
        }

        // Removes stale assets, whose files no longer exists.
        foreach (List<Asset> duplicatedSet in result)
        {
            List<Asset> assetsToRemove = new();

            for (int i = 0; i < duplicatedSet.Count; i++)
            {
                if (!_storageService.FileExists(duplicatedSet[i].FullPath))
                {
                    assetsToRemove.Add(duplicatedSet[i]);
                }
            }

            foreach (Asset asset in assetsToRemove)
            {
                duplicatedSet.Remove(asset);
            }
        }

        result = result.Where(r => r.Count > 1).ToList();

        // Loads the file information for each asset.
        foreach (List<Asset> duplicatedSet in result)
        {
            foreach (Asset asset in duplicatedSet)
            {
                _storageService.GetFileInformation(asset);
            }
        }

        return result;
    }

    // Between Original and Thumbnail:
    // PHash the hammingDistance is 36/210
    // DHash the hammingDistance is 16/17
    // MD5Hash the hammingDistance is 32/32
    // SHA512 the hammingDistance is 118/128
    private List<List<Asset>> FindPHashDuplicates()
    {
        //Adjust it if needed, the max advised is less than 90 (for example 68 can detect false positives)
        //By keeping 40, it can detect a Thumbnail and an original with low quality as duplicates
        const int threshold = 40; //5 or 6 is often used as a default value in image comparison libraries
        List<Asset> assets = new(_assetRepository.GetCataloguedAssets());
        List<List<Asset>> duplicates = new();

        // Loop through all possible pairs of assets
        for (int i = 0; i < assets.Count - 1; i++)
        {
            for (int j = i + 1; j < assets.Count; j++)
            {
                var asset1 = assets[i];
                var asset2 = assets[j];
                // Calculate the Hamming distance between the pair of assets
                int hammingDistance = HashingHelper.CalculateHammingDistance(asset1.Hash, asset2.Hash);

                // If the Hamming distance is below the threshold, add the pair of assets to the duplicates list
                if (hammingDistance <= threshold)
                {
                    // Check if the duplicates list already contains a list with one of the assets
                    bool addedToExistingList = false;
                    foreach (List<Asset> list in duplicates)
                    {
                        if (list.Contains(assets[i]))
                        {
                            list.Add(assets[j]);
                            addedToExistingList = true;
                            break;
                        }
                        else if (list.Contains(assets[j]))
                        {
                            list.Add(assets[i]);
                            addedToExistingList = true;
                            break;
                        }
                    }

                    // If the pair of assets is not added to an existing list, create a new list for them
                    if (!addedToExistingList)
                    {
                        List<Asset> newDuplicatesList = new()
                        {
                            assets[i],
                            assets[j]
                        };

                        duplicates.Add(newDuplicatesList);
                    }
                }
            }
        }

        return duplicates;
    }
}
