using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ExifHelperIsValidGdiPlusImageBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = Shared.LoadJpgImageBuffers();
        _logger = NullLogger.Instance;
    }

    // Current: BitmapFrame.Create triggers full pixel decode. The result is discarded;
    // only the absence of an exception indicates validity.
    [Benchmark(Baseline = true)]
    public bool[] Original()
    {
        return Shared.RunOnStaThread(() =>
        {
            bool[] results = new bool[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = ExifHelper.IsValidGdiPlusImage(_imageBuffers[i], _logger);
            }

            return results;
        });
    }

    // Optimized: BitmapDecoder with DelayCreation validates the image header/format only,
    // without decoding pixel data. Header-level corruption (unsupported format, truncated
    // header) is caught. Pixel-level corruption is NOT caught — it would only surface
    // later when the thumbnail decode fails.
    [Benchmark]
    public bool[] Optimized_DelayCreation()
    {
        return Shared.RunOnStaThread(() =>
        {
            bool[] results = new bool[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = IsValidGdiPlusImageHeaderOnly(_imageBuffers[i]);
            }

            return results;
        });
    }

    private static bool IsValidGdiPlusImageHeaderOnly(byte[] imageData)
    {
        try
        {
            using MemoryStream ms = new(imageData);
            BitmapDecoder.Create(
                ms,
                BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile,
                BitmapCacheOption.None);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
