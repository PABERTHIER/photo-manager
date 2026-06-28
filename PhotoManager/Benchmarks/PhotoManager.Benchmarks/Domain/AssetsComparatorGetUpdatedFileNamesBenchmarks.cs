namespace PhotoManager.Benchmarks.Domain;

// Context:
// AssetsComparator.GetUpdatedFileNames calls IsUpdatedAsset() — a static factory method
// that returns a non-capturing lambda. In modern .NET (Roslyn), non-capturing lambdas
// are typically cached as static fields at the declaration site, meaning the factory
// likely does not allocate on repeated calls. This benchmark verifies whether switching
// to a direct static method group (which guarantees delegate caching) produces any gain.

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetsComparatorGetUpdatedFileNamesBenchmarks
{
    private List<Asset> _cataloguedAssets = null!;

    // Realistic: ~30% of assets have been modified after their thumbnail was created
    [Params(100, 1000)] public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        DateTime thumbnailTime = DateTime.Now.AddDays(-2);
        DateTime newFileTime = DateTime.Now; // newer than thumbnail → updated
        DateTime oldFileTime = DateTime.Now.AddDays(-3); // older than thumbnail → not updated

        _cataloguedAssets =
        [
            .. Enumerable.Range(1, AssetCount).Select(i =>
                AssetBenchmarkBuilder.Create()
                    .WithFolderPath("", Guid.Empty)
                    .WithFileName($"IMG_{i:D4}.jpg")
                    .WithPixels(1920, 1080, 200, 112)
                    .WithFileProperties(1024, i % 10 < 3 ? newFileTime : oldFileTime, oldFileTime)
                    .WithHash(string.Empty)
                    .WithThumbnailCreationDateTime(thumbnailTime)
                    .WithImageRotation(ImageRotation.Rotate0)
                    .WithCorrupted(false, null)
                    .WithRotated(false, null)
                    .Build())
        ];
    }

    // ── Benchmarks ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public string[] Original_FuncFactory()
    {
        return [.. _cataloguedAssets.Where(IsUpdatedAsset_Factory()).Select(ca => ca.FileName)];
    }

    [Benchmark]
    public string[] Optimized_StaticMethodGroup()
    {
        return [.. _cataloguedAssets.Where(IsUpdatedAsset_Static).Select(ca => ca.FileName)];
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Func<Asset, bool> IsUpdatedAsset_Factory()
    {
        return a => a.FileProperties.Creation > a.ThumbnailCreationDateTime
                    || a.FileProperties.Modification > a.ThumbnailCreationDateTime;
    }

    private static bool IsUpdatedAsset_Static(Asset a) =>
        a.FileProperties.Creation > a.ThumbnailCreationDateTime
        || a.FileProperties.Modification > a.ThumbnailCreationDateTime;
}
