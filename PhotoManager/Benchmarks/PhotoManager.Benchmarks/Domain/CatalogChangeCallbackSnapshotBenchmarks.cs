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
            _cataloguedAssetsByPath.Add(new()
            {
                FolderId = folder.Id,
                Folder = folder,
                FileName = $"Image_{i:D6}.jpg",
                Pixel = new()
                {
                    Asset = new() { Width = 1920, Height = 1080 },
                    Thumbnail = new() { Width = 200, Height = 150 }
                },
                Hash = i.ToString()
            });
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
