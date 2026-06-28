using System.Runtime.InteropServices;

namespace PhotoManager.Benchmarks.Domain;

// Context:
// AssetsComparator.GetDeletedFileNames iterates a List<Asset> via LINQ Select then Except.
// Except builds a pre-sized HashSet<string> from fileNames (string[] implements ICollection),
// so there is no intermediate array allocation issue — unlike GetNewFileNames.
// The remaining cost is the LINQ Select + Except pipeline overhead.
// A pre-built HashSet loop avoids that overhead entirely (winner from round 1).
//
// Three variants compare List<Asset> iteration strategies inside the winning loop:
//   a. foreach on List<Asset>        — uses List<T>.Enumerator struct (current applied winner)
//   b. for with index on List<Asset> — avoids enumerator, uses direct indexer + bounds check
//   c. CollectionsMarshal.AsSpan     — removes bounds checks entirely; fastest possible List<T> iteration

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetsComparatorGetDeletedFileNamesBenchmarks
{
    private string[] _fileNames = null!;
    private List<Asset> _cataloguedAssets = null!;

    // Realistic: 80% of catalogued assets still exist on disk; 20% have been deleted
    [Params(100, 1000)] public int CataloguedCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // cataloguedAssets contains CataloguedCount assets
        _cataloguedAssets =
        [
            .. Enumerable.Range(1, CataloguedCount).Select(i =>
                AssetBenchmarkBuilder.Create()
                    .WithFolderPath("", Guid.Empty)
                    .WithFileName($"IMG_{i:D4}.jpg")
                    .WithPixels(1920, 1080, 200, 112)
                    .WithFileSize(1024)
                    .WithHash(string.Empty)
                    .WithThumbnailCreationDateTime(DateTime.Now)
                    .WithImageRotation(ImageRotation.Rotate0)
                    .WithCorrupted(false, null)
                    .WithRotated(false, null)
                    .Build())
        ];

        // fileNames is 80% of catalogued assets (i.e. 20% have been deleted)
        _fileNames = [.. _cataloguedAssets.Take(CataloguedCount * 8 / 10).Select(a => a.FileName)];
    }

    // ── Benchmarks ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public string[] Original_SelectExcept()
    {
        return [.. _cataloguedAssets.Select(ca => ca.FileName).Except(_fileNames)];
    }

    [Benchmark]
    public string[] Optimized_PrebuiltHashSetLoop()
    {
        HashSet<string> fileNameSet = [.. _fileNames];
        List<string> result = new(_cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in _cataloguedAssets)
        {
            if (!fileNameSet.Contains(cataloguedAsset.FileName))
            {
                result.Add(cataloguedAsset.FileName);
            }
        }

        return [.. result];
    }

    [Benchmark]
    public string[] Optimized_ForIndex()
    {
        // for with index: avoids List<T>.Enumerator struct, uses direct indexer access
        HashSet<string> fileNameSet = [.. _fileNames];
        List<string> result = new(_cataloguedAssets.Count);

        for (int i = 0; i < _cataloguedAssets.Count; i++)
        {
            Asset a = _cataloguedAssets[i];
            if (!fileNameSet.Contains(a.FileName))
            {
                result.Add(a.FileName);
            }
        }

        return [.. result];
    }

    [Benchmark]
    public string[] Optimized_Span()
    {
        // CollectionsMarshal.AsSpan: removes bounds checks on List<Asset> iteration entirely
        HashSet<string> fileNameSet = [.. _fileNames];
        List<string> result = new(_cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(_cataloguedAssets))
        {
            if (!fileNameSet.Contains(cataloguedAsset.FileName))
            {
                result.Add(cataloguedAsset.FileName);
            }
        }

        return [.. result];
    }

}
