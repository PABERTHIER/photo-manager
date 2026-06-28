using System.Collections.Concurrent;

namespace PhotoManager.Benchmarks.Infrastructure;

[MemoryDiagnoser]
[ShortRunJob]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetRepositoryGetCataloguedAssetsBenchmarks
{
    private ConcurrentDictionary<Guid, ConcurrentDictionary<string, Asset>> _assetsByFolderId = null!;

    [Params(100)]
    public int FolderCount { get; set; }

    [Params(1_000)]
    public int AssetsPerFolder { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _assetsByFolderId = new();

        for (int folderIndex = 0; folderIndex < FolderCount; folderIndex++)
        {
            Folder folder = new()
            {
                Id = Guid.NewGuid(),
                Path = $@"C:\Photos\Folder{folderIndex:D4}"
            };
            ConcurrentDictionary<string, Asset> folderAssets = new(StringComparer.Ordinal);
            _assetsByFolderId[folder.Id] = folderAssets;

            for (int assetIndex = 0; assetIndex < AssetsPerFolder; assetIndex++)
            {
                string fileName = $"Image_{AssetsPerFolder - assetIndex:D6}.jpg";
                folderAssets[fileName] = AssetBenchmarkBuilder.Create()
                    .WithFolder(folder)
                    .WithFileName(fileName)
                    .WithPixels(1920, 1080, 200, 150)
                    .WithFileSize(0)
                    .WithHash($"{folderIndex:D4}-{assetIndex:D6}")
                    .WithThumbnailCreationDateTime(DateTime.UnixEpoch)
                    .WithImageRotation(ImageRotation.Rotate0)
                    .WithCorrupted(false, null)
                    .WithRotated(false, null)
                    .Build();
            }
        }
    }

    [Benchmark(Baseline = true)]
    public Asset[] Current_SnapshotAndSort()
    {
        Asset[] assets = [.. _assetsByFolderId.Values.SelectMany(static inner => inner.Values)];

        Array.Sort(assets, static (a, b) =>
        {
            int compare = string.Compare(a.FileName, b.FileName, StringComparison.Ordinal);

            return compare != 0 ? compare : string.Compare(a.Folder.Path, b.Folder.Path, StringComparison.Ordinal);
        });

        return assets;
    }

    [Benchmark]
    public Asset[] Optimized_SnapshotOnly()
    {
        return [.. _assetsByFolderId.Values.SelectMany(static inner => inner.Values)];
    }
}
