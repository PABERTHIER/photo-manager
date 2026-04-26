using System.Runtime.InteropServices;

namespace PhotoManager.Benchmarks.Domain;

// Context:
// AssetsComparator.GetNewFileNames materializes an intermediate string[] from the catalogued
// assets before passing it to Except. Since Except internally builds a HashSet<string>,
// the array allocation can be avoided by building the HashSet directly.
//
// Four approaches compare the overall strategy:
//   1. Original    : spread to intermediate string[] → Except builds Set<T> from it (pre-sized via ICollection)
//   2. IEnumerable : skip intermediate array → Except builds Set<T> lazily (not pre-sized)
//   3. HashSet+Except: pre-build HashSet → Except builds Set<T> from it (pre-sized via ICollection.Count)
//   4. HashSet+Loop: pre-build HashSet → custom loop (avoids all LINQ overhead; winner from round 1)
//
// Three additional variants compare List<Asset> iteration strategies inside the winning loop:
//   4a. foreach on List<Asset>        — uses List<T>.Enumerator struct (current applied winner)
//   4b. for with index on List<Asset> — avoids enumerator, uses direct indexer + bounds check
//   4c. CollectionsMarshal.AsSpan     — removes bounds checks entirely; fastest possible List<T> iteration

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AssetsComparatorGetNewFileNamesBenchmarks
{
    private string[] _fileNames = null!;
    private List<Asset> _cataloguedAssets = null!;

    // Realistic distribution: 70% images, 15% videos, 15% other files; 50% already catalogued
    [Params(100, 1000)] public int FileCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string[] extensions =
            [".jpg", ".jpg", ".jpg", ".jpg", ".png", ".png", ".gif", ".mp4", ".avi", ".mkv", ".txt", ".exe", ".pdf"];
        _fileNames =
        [
            .. Enumerable.Range(1, FileCount).Select(i =>
                $"IMG_{i:D4}{extensions[i % extensions.Length]}")
        ];

        // Half the files are already catalogued
        _cataloguedAssets = [.. _fileNames.Take(FileCount / 2).Select(CreateAsset)];
    }

    // ── Benchmarks ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public string[] Original_SpreadToArrayThenExcept()
    {
        // Replicates: [.. cataloguedAssets.Select(ca => ca.FileName)] → intermediate string[]
        return GetNewFileNamesList_Original(_fileNames, [.. _cataloguedAssets.Select(ca => ca.FileName)]);
    }

    [Benchmark]
    public string[] Optimized_IEnumerableExcept()
    {
        // Skips intermediate string[] but Except can't pre-size its internal Set<T>
        return [.. _fileNames.Except(_cataloguedAssets.Select(ca => ca.FileName)).Where(IsValidAsset)];
    }

    [Benchmark]
    public string[] Optimized_PrebuiltHashSetThenExcept()
    {
        // Pre-builds a properly-sized HashSet<string>, then passes to Except.
        // Except detects ICollection<string>.Count and pre-sizes its internal Set<T>.
        HashSet<string> cataloguedSet = new(_cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in _cataloguedAssets)
        {
            cataloguedSet.Add(cataloguedAsset.FileName);
        }

        return [.. _fileNames.Except(cataloguedSet).Where(IsValidAsset)];
    }

    [Benchmark]
    public string[] Optimized_PrebuiltHashSetLoop()
    {
        // Completely avoids LINQ overhead.
        // Safe: filesystem directory listings do not produce duplicate file names.
        HashSet<string> cataloguedSet = new(_cataloguedAssets.Count);

        foreach (Asset a in _cataloguedAssets)
        {
            cataloguedSet.Add(a.FileName);
        }

        List<string> result = new(_fileNames.Length);

        foreach (string fileName in _fileNames)
        {
            if (!cataloguedSet.Contains(fileName) && IsValidAsset(fileName))
            {
                result.Add(fileName);
            }
        }

        return [.. result];
    }

    [Benchmark]
    public string[] Optimized_PrebuiltHashSetLoop_ForIndex()
    {
        // for with index: avoids List<T>.Enumerator struct, uses direct indexer access
        HashSet<string> cataloguedSet = new(_cataloguedAssets.Count);

        for (int i = 0; i < _cataloguedAssets.Count; i++)
        {
            cataloguedSet.Add(_cataloguedAssets[i].FileName);
        }

        List<string> result = new(_fileNames.Length);

        foreach (string fileName in _fileNames)
        {
            if (!cataloguedSet.Contains(fileName) && IsValidAsset(fileName))
            {
                result.Add(fileName);
            }
        }

        return [.. result];
    }

    [Benchmark]
    public string[] Optimized_PrebuiltHashSetLoop_Span()
    {
        // CollectionsMarshal.AsSpan: removes bounds checks on List<Asset> iteration entirely
        HashSet<string> cataloguedSet = new(_cataloguedAssets.Count);

        foreach (Asset cataloguedAsset in CollectionsMarshal.AsSpan(_cataloguedAssets))
        {
            cataloguedSet.Add(cataloguedAsset.FileName);
        }

        List<string> result = new(_fileNames.Length);

        foreach (string fileName in _fileNames)
        {
            if (!cataloguedSet.Contains(fileName) && IsValidAsset(fileName))
            {
                result.Add(fileName);
            }
        }

        return [.. result];
    }

    // ── Helpers: replicates original AssetsComparator logic ──────────────────

    private static string[] GetNewFileNamesList_Original(string[] fileNames, string[] destinationFileNames)
    {
        return [.. fileNames.Except(destinationFileNames).Where(IsValidAsset)];
    }

    private static bool IsValidAsset(string assetFileName)
    {
        return ImageHelper.IsImageFile(assetFileName) || VideoHelper.IsVideoFile(assetFileName);
    }

    private static Asset CreateAsset(string fileName) => new()
    {
        FolderId = Guid.Empty,
        Folder = new() { Id = Guid.Empty, Path = "" },
        FileName = fileName,
        Pixel = new()
        {
            Asset = new() { Width = 1920, Height = 1080 },
            Thumbnail = new() { Width = 200, Height = 112 }
        },
        FileProperties = new() { Size = 1024 },
        Hash = string.Empty,
        ThumbnailCreationDateTime = DateTime.Now,
        Metadata = new()
        {
            Corrupted = new() { IsTrue = false, Message = null },
            Rotated = new() { IsTrue = false, Message = null }
        }
    };
}
