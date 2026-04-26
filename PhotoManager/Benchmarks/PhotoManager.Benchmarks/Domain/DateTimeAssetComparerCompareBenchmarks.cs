using PhotoManager.Domain.Comparers;

namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class DateTimeAssetComparerCompareBenchmarks
{
    private Asset _assetEarlier = null!;
    private Asset _assetLater = null!;

    // Same second, different filename — forces tie-breaker through the full delegate chain
    // (ticks / TimeSpan.TicksPerSecond produces equal longs for sub-second differences)
    private Asset _assetSameDateA = null!;
    private Asset _assetSameDateB = null!;

    private DateTimeAssetComparer _original = null!;
    private IComparer<Asset> _optimizedCached = null!;
    private IComparer<Asset> _optimizedFlattened = null!;

    [GlobalSetup]
    public void Setup()
    {
        Guid folderId = Guid.NewGuid();
        DateTime date1 = new(2024, 6, 15, 12, 0, 0, DateTimeKind.Local);
        DateTime date2 = new(2024, 6, 10, 12, 0, 0, DateTimeKind.Local);

        _assetEarlier = CreateAsset(folderId, "Image 1.jpg", date2);
        _assetLater = CreateAsset(folderId, "Image 2.jpg", date1);
        _assetSameDateA = CreateAsset(folderId, "Image A.jpg", date1);
        _assetSameDateB = CreateAsset(folderId, "Image B.jpg", date1.AddTicks(500)); // same second

        _original = new(true, a => a.FileProperties.Modification);
        _optimizedCached = new DateTimeAssetComparerCachedComparer(true, a => a.FileProperties.Modification);
        _optimizedFlattened = new DateTimeAssetComparerFlattened(true, a => a.FileProperties.Modification);
    }

    // ── Compare ───────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Original(string scenario)
    {
        return scenario == "different"
            ? _original.Compare(_assetEarlier, _assetLater)
            : _original.Compare(_assetSameDateA, _assetSameDateB);
    }

    [Benchmark]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Optimized_CachedComparer(string scenario)
    {
        return scenario == "different"
            ? _optimizedCached.Compare(_assetEarlier, _assetLater)
            : _optimizedCached.Compare(_assetSameDateA, _assetSameDateB);
    }

    [Benchmark]
    [Arguments("different")]
    [Arguments("equal")]
    public int Compare_Optimized_Flattened(string scenario)
    {
        return scenario == "different"
            ? _optimizedFlattened.Compare(_assetEarlier, _assetLater)
            : _optimizedFlattened.Compare(_assetSameDateA, _assetSameDateB);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Asset CreateAsset(Guid folderId, string fileName, DateTime modification) => new()
    {
        FolderId = folderId,
        Folder = new() { Id = folderId, Path = @"C:\Photos" },
        FileName = fileName,
        Pixel = new()
        {
            Asset = new() { Width = 1280, Height = 720 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = 29000, Creation = modification, Modification = modification },
        ThumbnailCreationDateTime = modification,
        Hash = "abc"
    };
}

// Caches the LongAssetComparer as a field instead of creating it (+ a closure) on every Compare() call
file sealed class DateTimeAssetComparerCachedComparer(bool ascending, Func<Asset, DateTime> dateTimeSelector)
    : IComparer<Asset>
{
    private readonly LongAssetComparer _longComparer =
        new(ascending, a => dateTimeSelector(a).Ticks / TimeSpan.TicksPerSecond);

    public int Compare(Asset? asset1, Asset? asset2) => _longComparer.Compare(asset1, asset2);
}

// Inlines all comparison logic, eliminating sub-comparer and delegate hops entirely
file sealed class DateTimeAssetComparerFlattened(bool ascending, Func<Asset, DateTime> dateTimeSelector)
    : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        long ticks1 = dateTimeSelector(asset1).Ticks / TimeSpan.TicksPerSecond;
        long ticks2 = dateTimeSelector(asset2).Ticks / TimeSpan.TicksPerSecond;
        int result = ticks1.CompareTo(ticks2);

        if (result == 0)
        {
            result = string.CompareOrdinal(asset1.FileName, asset2.FileName);
        }

        return ascending ? result : -result;
    }
}
