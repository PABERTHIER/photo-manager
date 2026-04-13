namespace PhotoManager.Benchmarks.Domain;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MoveAssetsServiceCopyBenchmarks
{
    private string _destinationFilePath = null!;

    [GlobalSetup]
    public void Setup()
    {
        _destinationFilePath = @"C:\Users\Photos\Holidays\2024\Summer\IMG_0001.jpg";
    }

    [Benchmark(Baseline = true)]
    public string GetDirectoryPath_Original()
    {
        return new FileInfo(_destinationFilePath).Directory!.FullName;
    }

    [Benchmark]
    public string GetDirectoryPath_Optimized_PathGetDirectoryName()
    {
        return Path.GetDirectoryName(_destinationFilePath)!;
    }
}
