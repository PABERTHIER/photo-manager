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

    private static Asset CreateAsset(int index) =>
        AssetBenchmarkBuilder.Create()
            .WithFolderPath("C:\\Assets", Guid.Empty)
            .WithFileName($"asset_{index:D5}.jpg")
            .WithPixels(1920, 1080, 200, 112)
            .WithFileSize(1024)
            .WithHash($"hash_{index:D5}")
            .WithThumbnailCreationDateTime(DateTime.Now)
            .WithImageRotation(ImageRotation.Rotate0)
            .WithCorrupted(false, null)
            .WithRotated(false, null)
            .Build();
}
