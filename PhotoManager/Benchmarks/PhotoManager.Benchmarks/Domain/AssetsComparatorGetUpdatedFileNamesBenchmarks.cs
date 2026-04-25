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
            .. Enumerable.Range(1, AssetCount).Select(i => CreateAsset(
                $"IMG_{i:D4}.jpg",
                fileCreation: i % 10 < 3 ? newFileTime : oldFileTime,
                fileModification: oldFileTime,
                thumbnailTime: thumbnailTime))
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

    private static Asset CreateAsset(
        string fileName,
        DateTime fileCreation,
        DateTime fileModification,
        DateTime thumbnailTime)
    {
        return new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "" },
            FileName = fileName,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new() { Creation = fileCreation, Modification = fileModification, Size = 1024 },
            Hash = string.Empty,
            ThumbnailCreationDateTime = thumbnailTime,
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }
}
