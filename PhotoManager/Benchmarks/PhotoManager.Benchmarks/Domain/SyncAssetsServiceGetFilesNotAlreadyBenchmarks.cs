namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SyncAssetsServiceGetFilesNotAlreadyBenchmarks
{
    private string[] _newFileNames = null!;
    private string[][] _subDirectoryFileNames = null!;

    [Params(1, 5, 10)] public int SubDirectoryCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _newFileNames = [.. Enumerable.Range(1, 200).Select(i => $"IMG_{i:D4}.jpg")];
        _subDirectoryFileNames = new string[SubDirectoryCount][];

        for (int i = 0; i < SubDirectoryCount; i++)
        {
            _subDirectoryFileNames[i] = [.. Enumerable.Range((i * 20) + 1, 50).Select(n => $"IMG_{n:D4}.jpg")];
        }
    }

    // ── Benchmarks ────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public string[] Original_LoopWithExceptPerSubDirectory()
    {
        string[] result = _newFileNames;

        for (int i = 0; i < _subDirectoryFileNames.Length; i++)
        {
            result = GetNewFileNamesToSync_Original(result, _subDirectoryFileNames[i]);
        }

        return result;
    }

    [Benchmark]
    public string[] Optimized_CombinedHashSet()
    {
        HashSet<string> allDestinationFileNames = [];

        for (int i = 0; i < _subDirectoryFileNames.Length; i++)
        {
            allDestinationFileNames.UnionWith(_subDirectoryFileNames[i]);
        }

        return [.. _newFileNames.Except(allDestinationFileNames)];
    }

    // ── Helpers: replicates original AssetsComparator logic ──────────────────

    private static string[] GetNewFileNamesToSync_Original(string[] sourceFileNames, string[] destinationFileNames)
    {
        return [.. sourceFileNames.Except(destinationFileNames).Where(IsValidAsset)];
    }

    private static bool IsValidAsset(string assetFileName)
    {
        return ImageHelper.IsImageFile(assetFileName) || VideoHelper.IsVideoFile(assetFileName);
    }
}
