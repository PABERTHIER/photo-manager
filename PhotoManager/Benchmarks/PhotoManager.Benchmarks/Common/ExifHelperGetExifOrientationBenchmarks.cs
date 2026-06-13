using Microsoft.Extensions.Logging;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ExifHelperGetExifOrientationBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    private const ushort DEFAULT_EXIF_ORIENTATION = 1;
    private const ushort CORRUPTED_IMAGE_ORIENTATION = 0;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = Shared.LoadJpgImageBuffers();
        _logger = NullLogger.Instance;
    }

    [Benchmark(Baseline = true)]
    public ushort[] Current_SkiaSharp()
    {
        ushort[] results = new ushort[_imageBuffers.Length];

        for (int i = 0; i < _imageBuffers.Length; i++)
        {
            results[i] = ExifHelper.GetExifOrientation(
                _imageBuffers[i],
                DEFAULT_EXIF_ORIENTATION,
                CORRUPTED_IMAGE_ORIENTATION,
                _logger);
        }

        return results;
    }
}
