using PhotoManager.Domain.Comparers;

namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringAssetComparerCompareBenchmarks
{
    private Asset _assetA = null!;
    private Asset _assetB = null!;
    private Asset _assetCommonPrefixA = null!;
    private Asset _assetCommonPrefixB = null!;

    private StringAssetComparer _ascending = null!;
    private IComparer<Asset> _ascendingOptimized = null!;

    [GlobalSetup]
    public void Setup()
    {
        Guid folderId = Guid.NewGuid();

        _assetA = CreateAsset(folderId, "Image 1.jpg");
        _assetB = CreateAsset(folderId, "Image 2.jpg");
        _assetCommonPrefixA = CreateAsset(folderId, "IMG_20240101_120000.jpg");
        _assetCommonPrefixB = CreateAsset(folderId, "IMG_20240102_120000.jpg");

        _ascending = new(true, a => a.FileName);
        _ascendingOptimized = new StringAssetComparerOptimized(true, a => a.FileName);
    }

    // ── Compare ───────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [Arguments("short")]
    [Arguments("common_prefix")]
    public int Compare_Original(string scenario)
    {
        return scenario == "short"
            ? _ascending.Compare(_assetA, _assetB)
            : _ascending.Compare(_assetCommonPrefixA, _assetCommonPrefixB);
    }

    [Benchmark]
    [Arguments("short")]
    [Arguments("common_prefix")]
    public int Compare_Optimized_CompareOrdinal(string scenario)
    {
        return scenario == "short"
            ? _ascendingOptimized.Compare(_assetA, _assetB)
            : _ascendingOptimized.Compare(_assetCommonPrefixA, _assetCommonPrefixB);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Asset CreateAsset(Guid folderId, string fileName) => new()
    {
        FolderId = folderId,
        Folder = new() { Id = folderId, Path = @"C:\Photos" },
        FileName = fileName,
        Pixel = new()
        {
            Asset = new() { Width = 1280, Height = 720 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = 29000, Creation = DateTime.Now, Modification = DateTime.Now },
        ThumbnailCreationDateTime = DateTime.Now,
        Hash = "abc"
    };
}

// Replaces string.Compare(..., StringComparison.Ordinal) with string.CompareOrdinal
file sealed class StringAssetComparerOptimized(bool ascending, Func<Asset, string> stringSelector) : IComparer<Asset>
{
    public int Compare(Asset? asset1, Asset? asset2)
    {
        if (asset1 == null || asset2 == null)
        {
            throw new ArgumentNullException(asset1 == null ? nameof(asset1) : nameof(asset2));
        }

        int result = string.CompareOrdinal(stringSelector(asset1), stringSelector(asset2));
        return ascending ? result : -result;
    }
}
