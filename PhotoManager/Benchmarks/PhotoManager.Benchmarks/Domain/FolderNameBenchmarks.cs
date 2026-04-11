namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public abstract class FolderNameBenchmarks
{
    private const string PATH_WITH_MULTIPLE_SEGMENTS = @"C:\Users\Photos\Holidays\2024";
    private const string PATH_WITH_SINGLE_SEGMENT = "Photos";
    private const string PATH_EMPTY = "";

    [Benchmark(Baseline = true)]
    [Arguments(PATH_WITH_MULTIPLE_SEGMENTS)]
    [Arguments(PATH_WITH_SINGLE_SEGMENT)]
    [Arguments(PATH_EMPTY)]
    public static string Original(string path)
    {
        string[] pathParts = !string.IsNullOrWhiteSpace(path)
            ? path.Split(['\\'], StringSplitOptions.RemoveEmptyEntries)
            : [];
        string result = pathParts.Length > 0 ? pathParts[^1] : string.Empty;

        return result;
    }

    [Benchmark]
    [Arguments(PATH_WITH_MULTIPLE_SEGMENTS)]
    [Arguments(PATH_WITH_SINGLE_SEGMENT)]
    [Arguments(PATH_EMPTY)]
    public static string Optimized_Span(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> pathAsSpan = path.AsSpan().TrimEnd('\\');
        int lastSeparator = pathAsSpan.LastIndexOf('\\');

        return lastSeparator >= 0 ? pathAsSpan[(lastSeparator + 1)..].ToString() : pathAsSpan.ToString();
    }
}
