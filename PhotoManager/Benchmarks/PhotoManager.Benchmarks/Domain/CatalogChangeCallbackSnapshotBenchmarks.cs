namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CatalogChangeCallbackSnapshotBenchmarks
{
    private List<Asset> _cataloguedAssetsByPath = null!;

    [Params(1_000, 10_000)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Folder folder = new()
        {
            Id = Guid.NewGuid(),
            Path = @"C:\Photos"
        };
        _cataloguedAssetsByPath = new(AssetCount);

        for (int i = 0; i < AssetCount; i++)
        {
            _cataloguedAssetsByPath.Add(AssetBenchmarkBuilder.Create()
                .WithFolder(folder)
                .WithFileName($"Image_{i:D6}.jpg")
                .WithPixels(1920, 1080, 200, 150)
                .WithFileSize(0)
                .WithHash(i.ToString())
                .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
                .WithImageRotation(ImageRotation.Rotate0)
                .WithCorrupted(false, null)
                .WithRotated(false, null)
                .Build());
        }
    }

    [Benchmark(Baseline = true)]
    public int Current_CloneEveryCallback()
    {
        int observedAssets = 0;

        for (int i = 0; i < _cataloguedAssetsByPath.Count; i++)
        {
            Asset[] snapshot = [.. _cataloguedAssetsByPath.Take(i + 1)];
            observedAssets += snapshot.Length;
        }

        return observedAssets;
    }

    [Benchmark]
    public int Optimized_ReuseSnapshotWhenStable()
    {
        Asset[] snapshot = [.. _cataloguedAssetsByPath];
        int observedAssets = 0;

        for (int i = 0; i < _cataloguedAssetsByPath.Count; i++)
        {
            observedAssets += snapshot.Length;
        }

        return observedAssets;
    }
}
