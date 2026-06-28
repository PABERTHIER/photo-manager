using System.Runtime.InteropServices;

namespace PhotoManager.Benchmarks.Domain;

// Variants:
//   a. Current_TryGetValueIndexer    — TryGetValue (miss) then indexer set on a new folder = 2 hash lookups
//                                       per distinct folder; 1 lookup per repeated folder. The applied code.
//   b. GetValueRefOrAddDefault       — CollectionsMarshal.GetValueRefOrAddDefault: a single hash lookup that
//                                       returns a ref to the (possibly default) slot, removing the double lookup.
//   c. SortThenSegment               — sort a (folderPath, fileName) index array by folder path, then slice each
//                                       contiguous run into a string[]; no Dictionary, no List growth, but pays
//                                       an O(n log n) sort. Array-only, to answer "can we avoid List entirely?".

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob]
public class MoveAssetsServiceDeleteAssetsBenchmarks
{
    private Asset[] _assets = null!;

    // Total assets to delete in one call.
    [Params(50, 500, 5000)] public int AssetCount { get; set; }

    // Distinct source folders the duplicates span (duplicates are spread across folders by nature).
    [Params(4, 50)] public int FolderCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _assets = new Asset[AssetCount];

        for (int i = 0; i < AssetCount; i++)
        {
            _assets[i] = AssetBenchmarkBuilder.Create()
                .WithFolderPath($@"C:\Photos\Folder_{i % FolderCount}", Guid.Empty)
                .WithFileName($"IMG_{i:D5}.jpg")
                .WithPixels(1920, 1080, 200, 112)
                .WithFileSize(1024)
                .WithHash(string.Empty)
                .WithThumbnailCreationDateTime(DateTime.UnixEpoch.AddSeconds(i))
                .WithImageRotation(ImageRotation.Rotate0)
                .WithCorrupted(false, null)
                .WithRotated(false, null)
                .Build();
        }
    }

    [Benchmark(Baseline = true)]
    public Dictionary<string, List<string>> Current_TryGetValueIndexer()
    {
        Dictionary<string, List<string>> fileNamesByFolderPath = new(StringComparer.Ordinal);

        foreach (Asset asset in _assets)
        {
            if (!fileNamesByFolderPath.TryGetValue(asset.Folder.Path, out List<string>? fileNames))
            {
                fileNames = [];
                fileNamesByFolderPath[asset.Folder.Path] = fileNames;
            }

            fileNames.Add(asset.FileName);
        }

        return fileNamesByFolderPath;
    }

    [Benchmark]
    public Dictionary<string, List<string>> GetValueRefOrAddDefault()
    {
        Dictionary<string, List<string>> fileNamesByFolderPath = new(StringComparer.Ordinal);

        foreach (Asset asset in _assets)
        {
            ref List<string>? fileNames = ref CollectionsMarshal.GetValueRefOrAddDefault(
                fileNamesByFolderPath, asset.Folder.Path, out bool exists);

            if (!exists)
            {
                fileNames = [];
            }

            fileNames!.Add(asset.FileName);
        }

        return fileNamesByFolderPath;
    }

    [Benchmark]
    public Dictionary<string, string[]> SortThenSegment()
    {
        int count = _assets.Length;
        string[] folderPaths = new string[count];
        string[] fileNames = new string[count];

        for (int i = 0; i < count; i++)
        {
            folderPaths[i] = _assets[i].Folder.Path;
            fileNames[i] = _assets[i].FileName;
        }

        Array.Sort(folderPaths, fileNames, StringComparer.Ordinal);

        Dictionary<string, string[]> fileNamesByFolderPath = new(StringComparer.Ordinal);
        int runStart = 0;

        for (int i = 1; i <= count; i++)
        {
            if (i == count || !string.Equals(folderPaths[i], folderPaths[runStart], StringComparison.Ordinal))
            {
                string[] runFileNames = new string[i - runStart];
                Array.Copy(fileNames, runStart, runFileNames, 0, i - runStart);
                fileNamesByFolderPath[folderPaths[runStart]] = runFileNames;
                runStart = i;
            }
        }

        return fileNamesByFolderPath;
    }
}
