using Microsoft.Extensions.Logging;

namespace PhotoManager.Benchmarks.Common;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
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

    [Benchmark(Baseline = true)]
    public (int, int)[] Current_SkiaSharp()
    {
        (int, int)[] results = new (int, int)[_imageBuffers.Length];

        for (int i = 0; i < _imageBuffers.Length; i++)
        {
            SkiaImageData image =
                SkiaImageData.FromEncodedBytesWithRotation(_imageBuffers[i], ImageRotation.Rotate0, _logger);
            results[i] = (image.Width, image.Height);
        }

        return results;
    }

    // Reads dimensions directly from raw format headers (zero codec objects,
    // zero allocations beyond the return tuple). Supports JPEG and PNG.
    [Benchmark]
    public (int, int)[] Optimized_HeaderParsing()
    {
        (int, int)[] results = new (int, int)[_imageBuffers.Length];

        for (int i = 0; i < _imageBuffers.Length; i++)
        {
            results[i] = GetImageDimensionsFromHeader(_imageBuffers[i], ImageRotation.Rotate0);
        }

        return results;
    }

    private static (int width, int height) GetImageDimensionsFromHeader(byte[] buffer, ImageRotation rotation)
    {
        (int rawWidth, int rawHeight) = TryReadDimensionsFromHeader(buffer);

        return rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270
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
