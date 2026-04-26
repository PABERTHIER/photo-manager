using System.Windows.Media.Imaging;

namespace PhotoManager.Benchmarks.Domain;

// Context:
// FindDuplicatedAssetsService.GetDuplicatesBetweenOriginalAndThumbnail previously used an
// O(n²) dictionary iteration: for each asset, it compared against all existing dictionary keys.
// For n=2000 assets this means ~2,000,000 Hamming distance calculations.

// Optimized approach: BK-tree (Burkhard-Keller tree) for nearest-neighbor search under
// Hamming distance. Each insert/search visits only the branches where
// |d(query, node) - d(query, child_key)| <= threshold, pruning most of the tree.

// Synthetic data: 20% of assets are near-duplicates (within threshold=10 of an original),
// 80% are unique. This reflects realistic photo library conditions.

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FindDuplicatedAssetsServiceGetDuplicatedAssetsBenchmarks
{
    private const int THRESHOLD = 10;
    private const int HASH_LENGTH = 210;

    private Asset[] _assets = null!;

    [Params(100, 500, 2000)] public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _assets = GenerateAssets(AssetCount);
    }

    [Benchmark(Baseline = true)]
    public List<List<Asset>> Original_DictionaryIteration()
    {
        List<Asset> assets = [.. _assets];
        Dictionary<string, List<Asset>> assetDictionary = [];

        for (int i = 0; i < assets.Count; i++)
        {
            Asset asset1 = assets[i];
            string hash1 = asset1.Hash;
            bool addedToSet = false;

            foreach ((string hash2, List<Asset> assetsAdded) in assetDictionary)
            {
                int hammingDistance = HashingHelper.CalculateHammingDistance(hash1, hash2,
                    NullLogger<FindDuplicatedAssetsService>.Instance);

                if (hammingDistance <= THRESHOLD)
                {
                    if (!assetsAdded.Contains(asset1))
                    {
                        assetsAdded.Add(asset1);
                        addedToSet = true;
                    }
                }
            }

            if (!assetDictionary.ContainsKey(hash1) && !addedToSet)
            {
                assetDictionary[hash1] = [asset1];
            }
        }

        return [.. assetDictionary.Values.Where(v => v.Count > 1)];
    }

    [Benchmark]
    public List<List<Asset>> Optimized_BkTree()
    {
        BkTree bkTree = new(NullLogger<FindDuplicatedAssetsService>.Instance);

        foreach (Asset asset in _assets)
        {
            if (!bkTree.TryAddToExistingGroups(asset.Hash, asset, THRESHOLD))
            {
                bkTree.Insert(asset.Hash, asset);
            }
        }

        return [.. bkTree.GetAllGroups().Where(g => g.Count > 1)];
    }

    private static Asset[] GenerateAssets(int count)
    {
        // Each "original" has up to 4 near-duplicate variants (distance 5-9, within threshold=10)
        // The remaining assets are unique originals (distance >> threshold from each other)
        int groupCount = count / 5;
        Asset[] assets = new Asset[count];
        int index = 0;

        for (int g = 0; g < groupCount && index < count; g++)
        {
            string baseHash = GenerateBaseHash(g);

            // 1 original
            assets[index++] = CreateAsset($"original_{g}.jpg", baseHash);

            // Up to 4 near-duplicates per original
            for (int v = 1; v <= 4 && index < count; v++)
            {
                string variantHash = CreateVariant(baseHash, v * 2, seed: (g * 100) + v);
                assets[index++] = CreateAsset($"variant_{g}_{v}.jpg", variantHash);
            }
        }

        // Fill remaining slots with unique singletons (no matches within threshold)
        while (index < count)
        {
            assets[index] = CreateAsset($"singleton_{index}.jpg", GenerateSingletonHash(index));
            index++;
        }

        return assets;
    }

    /// <summary>
    /// Generates a 210-char hash where a unique 30-char block at position [g*30 % 180]
    /// differs from all other base hashes, ensuring inter-base distance > threshold=10.
    /// </summary>
    private static string GenerateBaseHash(int g)
    {
        char[] chars = new char[HASH_LENGTH];
        Array.Fill(chars, '0');

        int blockStart = (g * 30) % 180;
        char uniqueChar = (char)('a' + (g % 26));

        for (int i = blockStart; i < blockStart + 30; i++)
        {
            chars[i] = uniqueChar;
        }

        return new string(chars);
    }

    /// <summary>
    /// Creates a variant by modifying <paramref name="numChanges"/> positions
    /// within the base hash's unique block, keeping distance within threshold.
    /// </summary>
    private static string CreateVariant(string baseHash, int numChanges, int seed)
    {
        char[] chars = baseHash.ToCharArray();
        Random rng = new(seed);

        for (int i = 0; i < numChanges; i++)
        {
            int pos = rng.Next(HASH_LENGTH);
            char current = chars[pos];
            chars[pos] = current == '0' ? '1' : '0';
        }

        return new string(chars);
    }

    private static string GenerateSingletonHash(int index)
    {
        // Singletons are spread across the hash space to ensure no accidental matches
        char[] chars = new char[HASH_LENGTH];
        Array.Fill(chars, '0');

        for (int i = 0; i < HASH_LENGTH; i += 20)
        {
            chars[i] = (char)('a' + ((index + i) % 26));
        }

        return new string(chars);
    }

    private static Asset CreateAsset(string fileName, string hash) => new()
    {
        FolderId = Guid.NewGuid(),
        Folder = new() { Id = Guid.Empty, Path = "" },
        FileName = fileName,
        ImageRotation = Rotation.Rotate0,
        Pixel = new()
        {
            Asset = new() { Width = 1920, Height = 1080 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = 1024 },
        ThumbnailCreationDateTime = DateTime.Now,
        Hash = hash,
        Metadata = new()
        {
            Corrupted = new() { IsTrue = false, Message = null },
            Rotated = new() { IsTrue = false, Message = null }
        }
    };
}
