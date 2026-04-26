using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

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

    // Current: BitmapFrame.Create without DelayCreation eagerly decodes full pixel data
    // just to access the EXIF metadata embedded in the file header.
    [Benchmark(Baseline = true)]
    public ushort[] Original()
    {
        return Shared.RunOnStaThread(() =>
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
        });
    }

    // Optimized: BitmapDecoder with DelayCreation reads only the image header/metadata,
    // skipping full pixel decode. EXIF data lives in the APP1 segment (JPEG header),
    // so it is accessible without decoding any pixel data.
    [Benchmark]
    public ushort[] Optimized_DelayCreation()
    {
        return Shared.RunOnStaThread(() =>
        {
            ushort[] results = new ushort[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = GetExifOrientationDelayCreation(
                    _imageBuffers[i],
                    DEFAULT_EXIF_ORIENTATION,
                    CORRUPTED_IMAGE_ORIENTATION);
            }

            return results;
        });
    }

    private static ushort GetExifOrientationDelayCreation(
        byte[] buffer, ushort defaultExifOrientation, ushort corruptedImageOrientation)
    {
        try
        {
            using MemoryStream stream = new(buffer);
            BitmapDecoder decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile,
                BitmapCacheOption.None);

            if (decoder.Frames[0].Metadata is BitmapMetadata bitmapMetadata)
            {
                object? orientation = bitmapMetadata.GetQuery("System.Photo.Orientation");

                if (orientation == null)
                {
                    return defaultExifOrientation;
                }

                return (ushort)orientation;
            }
        }
        catch (Exception ex) when (
            ex is not NotSupportedException { InnerException.HResult: -2003292351 })
        {
            // suppress — same intent as ExifHelper (avoid propagating decode errors)
        }

        return corruptedImageOrientation;
    }
}
