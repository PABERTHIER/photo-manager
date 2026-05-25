using Microsoft.Extensions.Logging;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ExifHelperIsValidImageBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = Shared.LoadJpgImageBuffers();
        _logger = NullLogger.Instance;
    }

    [Benchmark(Baseline = true)]
    public bool[] Current_SkiaSharp()
    {
        bool[] results = new bool[_imageBuffers.Length];

        for (int i = 0; i < _imageBuffers.Length; i++)
        {
            results[i] = ExifHelper.IsValidImage(_imageBuffers[i], _logger);
        }

        return results;
    }
}
