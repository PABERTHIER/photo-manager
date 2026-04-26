using PhotoManager.Domain.Comparers;

namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class LongAssetComparerCompareBenchmarks
{
    private Asset _assetSmaller = null!;
    private Asset _assetLarger = null!;
    private Asset _assetSameSizeA = null!;
    private Asset _assetSameSizeB = null!;

    private LongAssetComparer _original = null!;
    private IComparer<Asset> _optimizedCached = null!;
    private IComparer<Asset> _optimizedFlattened = null!;

    [GlobalSetup]
    public void Setup()
    {
        Guid folderId = Guid.NewGuid();

        _assetSmaller = CreateAsset(folderId, "Image 1.jpg", 29000);
        _assetLarger = CreateAsset(folderId, "Image 2.jpg", 35000);
        _assetSameSizeA = CreateAsset(folderId, "Image A.jpg", 29000);
        _assetSameSizeB = CreateAsset(folderId, "Image B.jpg", 29000);

        _original = new(true, a => a.FileProperties.Size);
        _optimizedCached = new LongAssetComparerCachedTieBreaker(true, a => a.FileProperties.Size);
        _optimizedFlattened = new LongAssetComparerFlattened(true, a => a.FileProperties.Size);
    }

    // ── Compare ───────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Original(string scenario)
    {
        return scenario == "different"
            ? _original.Compare(_assetSmaller, _assetLarger)
            : _original.Compare(_assetSameSizeA, _assetSameSizeB);
    }

    [Benchmark]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Optimized_CachedTieBreaker(string scenario)
    {
        return scenario == "different"
            ? _optimizedCached.Compare(_assetSmaller, _assetLarger)
            : _optimizedCached.Compare(_assetSameSizeA, _assetSameSizeB);
    }

    [Benchmark]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Optimized_Flattened(string scenario)
    {
        return scenario == "different"
            ? _optimizedFlattened.Compare(_assetSmaller, _assetLarger)
            : _optimizedFlattened.Compare(_assetSameSizeA, _assetSameSizeB);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Asset CreateAsset(Guid folderId, string fileName, long size) => new()
    {
        FolderId = folderId,
        Folder = new() { Id = folderId, Path = @"C:\Photos" },
        FileName = fileName,
        Pixel = new()
        {
            Asset = new() { Width = 1280, Height = 720 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = size, Creation = DateTime.Now, Modification = DateTime.Now },
        ThumbnailCreationDateTime = DateTime.Now,
        Hash = "abc"
    };
}

// Caches the StringAssetComparer as a field instead of creating it on every tie
file sealed class LongAssetComparerCachedTieBreaker(bool ascending, Func<Asset, long> longSelector) : IComparer<Asset>
{
    private readonly StringAssetComparer _tieBreaker = new(ascending, a => a.FileName);

    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        int result = longSelector(asset1).CompareTo(longSelector(asset2));

        if (result == 0)
        {
            return _tieBreaker.Compare(asset1, asset2);
        }

        return ascending ? result : -result;
    }
}

// Inlines all comparison logic, eliminating sub-comparer delegate hops entirely
file sealed class LongAssetComparerFlattened(bool ascending, Func<Asset, long> longSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        int result = longSelector(asset1).CompareTo(longSelector(asset2));

        if (result == 0)
        {
            result = string.CompareOrdinal(asset1.FileName, asset2.FileName);
        }

        return ascending ? result : -result;
    }
}
