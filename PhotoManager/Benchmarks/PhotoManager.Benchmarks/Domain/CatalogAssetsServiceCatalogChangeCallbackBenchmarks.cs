namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CatalogAssetsServiceCatalogChangeCallbackBenchmarks
{
    private Asset[] _assets = null!;

    [Params(100, 1_000, 10_000)]
    public int AssetCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _assets = new Asset[AssetCount];

        for (int i = 0; i < _assets.Length; i++)
        {
            _assets[i] = CreateAsset(i);
        }
    }

    [Benchmark(Baseline = true)]
    public long Current_CloneListOnEveryCallback()
    {
        List<Asset> cataloguedAssets = new(AssetCount);
        long observedCount = 0;

        for (int i = 0; i < _assets.Length; i++)
        {
            cataloguedAssets.Add(_assets[i]);
            CatalogChangeCallbackEventArgs eventArgs = new()
            {
                Asset = _assets[i],
                CataloguedAssetsByPath = [.. cataloguedAssets],
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            };

            observedCount += eventArgs.CataloguedAssetsByPath.Count;
        }

        return observedCount;
    }

    [Benchmark]
    public long Optimized_PassCurrentListReference()
    {
        List<Asset> cataloguedAssets = new(AssetCount);
        long observedCount = 0;

        for (int i = 0; i < _assets.Length; i++)
        {
            cataloguedAssets.Add(_assets[i]);
            CatalogChangeCallbackEventArgs eventArgs = new()
            {
                Asset = _assets[i],
                CataloguedAssetsByPath = cataloguedAssets,
                Reason = CatalogChangeReason.AssetCreated,
                Message = string.Empty
            };

            observedCount += eventArgs.CataloguedAssetsByPath.Count;
        }

        return observedCount;
    }

    private static Asset CreateAsset(int index)
    {
        return new()
        {
            FolderId = Guid.Empty,
            Folder = new() { Id = Guid.Empty, Path = "C:\\Assets" },
            FileName = $"asset_{index:D5}.jpg",
            ImageRotation = ImageRotation.Rotate0,
            Pixel = new()
            {
                Asset = new() { Width = 1920, Height = 1080 },
                Thumbnail = new() { Width = 200, Height = 112 }
            },
            FileProperties = new() { Size = 1024 },
            ThumbnailCreationDateTime = DateTime.Now,
            Hash = $"hash_{index:D5}",
            Metadata = new()
            {
                Corrupted = new() { IsTrue = false, Message = null },
                Rotated = new() { IsTrue = false, Message = null }
            }
        };
    }
}
