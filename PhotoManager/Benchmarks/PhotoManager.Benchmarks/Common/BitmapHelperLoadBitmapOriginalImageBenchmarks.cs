using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

namespace PhotoManager.Benchmarks.Common;

// LoadBitmapOriginalImage decodes full pixel data (OnLoad cache) into ~3.5 MB of unmanaged WIC memory per image.
// BenchmarkDotNet's default invocation count (~250× for ~2ms ops) × 4 images = ~3.5 GB unmanaged per iteration.
// Unmanaged WIC memory is invisible to GC, so it accumulates until Win32 OOM.
// Fix: invocationCount=1 (one benchmark call per measurement), IterationCleanup forces GC to release WIC COM objects.
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SimpleJob(warmupCount: 3, iterationCount: 10, invocationCount: 1)]
public class BitmapHelperLoadBitmapOriginalImageBenchmarks
{
    private byte[][] _imageBuffers = null!;
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _imageBuffers = Shared.LoadJpgImageBuffers();
        _logger = NullLogger.Instance;
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // Force finalization of managed BitmapImage/BitmapDecoder wrappers to release unmanaged WIC COM memory.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    // Current: fully decodes the image at original resolution only to read PixelWidth/PixelHeight,
    // which are then immediately discarded. The decoded pixels are never used.
    [Benchmark(Baseline = true)]
    public (int, int)[] Original()
    {
        return Shared.RunOnStaThread(() =>
        {
            (int, int)[] results = new (int, int)[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                BitmapImage image = BitmapHelper.LoadBitmapOriginalImage(_imageBuffers[i], Rotation.Rotate0, _logger);
                results[i] = (image.PixelWidth, image.PixelHeight);
            }

            return results;
        });
    }

    // Optimized: BitmapDecoder with DelayCreation reads only the image header/metadata,
    // skipping full pixel decode. Dimensions are extracted from the SOF segment.
    // BitmapCacheOption.None avoids caching decoded pixel data between benchmark iterations.
    [Benchmark]
    public (int, int)[] Optimized_BitmapDecoder()
    {
        return Shared.RunOnStaThread(() =>
        {
            (int, int)[] results = new (int, int)[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = GetImageDimensionsViaDecoder(_imageBuffers[i], Rotation.Rotate0);
            }

            return results;
        });
    }

    // Optimized: reads dimensions directly from raw format headers (zero WIC objects,
    // zero allocations beyond the return tuple). Supports JPEG and PNG.
    [Benchmark]
    public (int, int)[] Optimized_HeaderParsing()
    {
        return Shared.RunOnStaThread(() =>
        {
            (int, int)[] results = new (int, int)[_imageBuffers.Length];

            for (int i = 0; i < _imageBuffers.Length; i++)
            {
                results[i] = GetImageDimensionsFromHeader(_imageBuffers[i], Rotation.Rotate0);
            }

            return results;
        });
    }

    private static (int width, int height) GetImageDimensionsViaDecoder(byte[] buffer, Rotation rotation)
    {
        using MemoryStream stream = new(buffer);
        BitmapDecoder decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.None);
        int rawWidth = decoder.Frames[0].PixelWidth;
        int rawHeight = decoder.Frames[0].PixelHeight;

        return rotation is Rotation.Rotate90 or Rotation.Rotate270
            ? (rawHeight, rawWidth)
            : (rawWidth, rawHeight);
    }

    private static (int width, int height) GetImageDimensionsFromHeader(byte[] buffer, Rotation rotation)
    {
        (int rawWidth, int rawHeight) = TryReadDimensionsFromHeader(buffer);

        return rotation is Rotation.Rotate90 or Rotation.Rotate270
            ? (rawHeight, rawWidth)
            : (rawWidth, rawHeight);
    }

    // Reads JPEG or PNG image dimensions directly from the file header bytes.
    // Returns (-1, -1) for unsupported formats.
    private static (int width, int height) TryReadDimensionsFromHeader(ReadOnlySpan<byte> buffer)
    {
        // JPEG: starts with FF D8
        if (buffer.Length >= 4 && buffer[0] == 0xFF && buffer[1] == 0xD8)
        {
            return ReadJpegDimensions(buffer);
        }

        // PNG: starts with 89 50 4E 47 0D 0A 1A 0A (8-byte signature)
        // IHDR at offset 8: 4-byte length, 4-byte "IHDR", 4-byte width (big-endian), 4-byte height
        if (buffer.Length >= 24
            && buffer[0] == 0x89 && buffer[1] == 0x50
            && buffer[2] == 0x4E && buffer[3] == 0x47)
        {
            int width = (buffer[16] << 24) | (buffer[17] << 16) | (buffer[18] << 8) | buffer[19];
            int height = (buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23];
            return (width, height);
        }

        return (-1, -1);
    }

    private static (int width, int height) ReadJpegDimensions(ReadOnlySpan<byte> buffer)
    {
        int offset = 2; // skip SOI marker (FF D8)

        while (offset < buffer.Length - 3)
        {
            if (buffer[offset] != 0xFF)
            {
                break;
            }

            byte marker = buffer[offset + 1];
            offset += 2;

            // SOF markers encode image dimensions (exclude DHT=C4, DAC=CC, RST=D0-D7, SOI=D8, EOI=D9, SOS=DA)
            if (marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC)
            {
                // SOF layout: 2-byte segment length, 1-byte precision, 2-byte height, 2-byte width
                if (offset + 6 < buffer.Length)
                {
                    int height = (buffer[offset + 3] << 8) | buffer[offset + 4];
                    int width = (buffer[offset + 5] << 8) | buffer[offset + 6];
                    return (width, height);
                }

                break;
            }

            // Skip segment: next 2 bytes are segment length (includes the 2 length bytes)
            if (offset + 1 >= buffer.Length)
            {
                break;
            }

            int segmentLength = (buffer[offset] << 8) | buffer[offset + 1];

            if (segmentLength < 2)
            {
                break;
            }

            offset += segmentLength;
        }

        return (-1, -1);
    }
}
