using ImageMagick;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace PhotoManager.Common;

/// <summary>
/// Cross-platform implementation of <see cref="IImageData"/> backed by SkiaSharp's <see cref="SKBitmap"/>.
/// Holds decoded pixel data in memory for fast access; encodes on demand for serialization.
/// </summary>
public sealed class SkiaImageData : IImageData
{
    private static readonly SKSamplingOptions ResizeSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    // Rotations are exact multiples of 90°, so every pixel maps 1:1 — nearest sampling keeps it lossless.
    private static readonly SKSamplingOptions RotationSamplingOptions = new(SKFilterMode.Nearest, SKMipmapMode.None);

    private bool _disposed;

    public SkiaImageData(SKBitmap bitmap, ImageRotation rotation)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        Bitmap = bitmap;
        Rotation = rotation;
    }

    public int Width => Bitmap.Width;
    public int Height => Bitmap.Height;
    public ImageRotation Rotation { get; }
    public SKBitmap Bitmap { get; }

    public byte[] ToByteArray(ImageEncodingFormat format)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Bitmap.IsEmpty)
        {
            return [];
        }

        return format switch
        {
            ImageEncodingFormat.Jpeg => EncodeJpeg(),
            ImageEncodingFormat.Png => EncodePng(),
            ImageEncodingFormat.Gif => EncodeGif(),
            ImageEncodingFormat.Bmp => EncodeBmp(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public Stream ToStream(ImageEncodingFormat format)
    {
        byte[] bytes = ToByteArray(format);
        return new MemoryStream(bytes, writable: false);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Bitmap.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Creates a <see cref="SkiaImageData"/> from raw image bytes (JPEG, PNG, GIF, BMP, etc.).
    /// </summary>
    public static SkiaImageData FromEncodedBytes(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ValidateBuffer(buffer, logger);

        SKBitmap bitmap = SKBitmap.Decode(buffer);
        return new(bitmap, rotation);
    }

    /// <summary>
    /// Creates a <see cref="SkiaImageData"/> from raw image bytes with resize.
    /// </summary>
    public static SkiaImageData FromEncodedBytes(byte[] buffer, ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        ValidateBuffer(buffer, logger);

        SKBitmap original = SKBitmap.Decode(buffer);

        try
        {
            (int targetWidth, int targetHeight) =
                CalculateTargetDimensions(width, height, original.Width, original.Height);
            SKBitmap resized = ResizeBitmap(original, targetWidth, targetHeight);
            return new(resized, rotation);
        }
        finally
        {
            original.Dispose();
        }
    }

    /// <summary>
    /// Creates a rotated <see cref="SkiaImageData"/> from raw image bytes.
    /// </summary>
    public static SkiaImageData FromEncodedBytesWithRotation(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ValidateBuffer(buffer, logger);

        SKBitmap decoded = SKBitmap.Decode(buffer);

        if (rotation == ImageRotation.Rotate0)
        {
            return new(decoded, ImageRotation.Rotate0);
        }

        try
        {
            SKBitmap rotated = ApplyRotation(decoded, rotation);
            return new(rotated, ImageRotation.Rotate0);
        }
        finally
        {
            decoded.Dispose();
        }
    }

    /// <summary>
    /// Creates a rotated and resized <see cref="SkiaImageData"/> from raw image bytes.
    /// Width/height targets are applied after rotation, so they describe the final displayed orientation.
    /// </summary>
    public static SkiaImageData FromEncodedBytesWithRotation(byte[] buffer, ImageRotation rotation, int width,
        int height, ILogger logger)
    {
        ValidateBuffer(buffer, logger);

        SKBitmap decoded = SKBitmap.Decode(buffer);

        SKBitmap? rotated = null;

        try
        {
            SKBitmap source = decoded;

            if (rotation != ImageRotation.Rotate0)
            {
                rotated = ApplyRotation(decoded, rotation);
                source = rotated;
            }

            (int targetWidth, int targetHeight) = CalculateTargetDimensions(width, height, source.Width, source.Height);
            SKBitmap resized = ResizeBitmap(source, targetWidth, targetHeight);
            return new(resized, ImageRotation.Rotate0);
        }
        finally
        {
            rotated?.Dispose();
            decoded.Dispose();
        }
    }

    /// <summary>
    /// Creates a <see cref="SkiaImageData"/> from an <see cref="SKBitmap"/> with rotation applied.
    /// Used for HEIC images that are decoded via MagickImage then converted to SKBitmap.
    /// </summary>
    public static SkiaImageData FromBitmapWithRotation(SKBitmap bitmap, ImageRotation rotation)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        if (rotation == ImageRotation.Rotate0)
        {
            return new(bitmap, ImageRotation.Rotate0);
        }

        try
        {
            SKBitmap rotated = ApplyRotation(bitmap, rotation);
            return new(rotated, ImageRotation.Rotate0);
        }
        finally
        {
            bitmap.Dispose();
        }
    }

    /// <summary>
    /// Creates a rotated <see cref="SkiaImageData"/> from an encoded image stream.
    /// </summary>
    public static SkiaImageData FromEncodedStreamWithRotation(Stream stream, ImageRotation rotation)
    {
        ArgumentNullException.ThrowIfNull(stream);

        SKBitmap decoded = SKBitmap.Decode(stream);

        if (rotation == ImageRotation.Rotate0)
        {
            return new(decoded, ImageRotation.Rotate0);
        }

        try
        {
            SKBitmap rotated = ApplyRotation(decoded, rotation);
            return new(rotated, ImageRotation.Rotate0);
        }
        finally
        {
            decoded.Dispose();
        }
    }

    /// <summary>
    /// Creates an empty <see cref="SkiaImageData"/> representing a failed or missing image.
    /// </summary>
    public static SkiaImageData Empty()
    {
        return new(new(1, 1), ImageRotation.Rotate0);
    }

    /// <summary>
    /// Gets the brightness of a specific pixel using the HSL lightness formula.
    /// </summary>
    public float GetPixelBrightness(int x, int y)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (x < 0 || x >= Bitmap.Width || y < 0 || y >= Bitmap.Height)
        {
            return 0f;
        }

        SKColor color = Bitmap.GetPixel(x, y);
        float red = color.Red / 255f;
        float green = color.Green / 255f;
        float blue = color.Blue / 255f;
        float max = Math.Max(red, Math.Max(green, blue));
        float min = Math.Min(red, Math.Min(green, blue));
        return (max + min) / 2f;
    }

    /// <summary>
    /// Calculates target dimensions matching Bitmap DecodePixelWidth/Height semantics:
    /// - Negative values are treated as their absolute value
    /// - Zero means "auto-calculate preserving aspect ratio"
    /// - Both zero means "use original dimensions"
    /// </summary>
    private static (int Width, int Height) CalculateTargetDimensions(int requestedWidth, int requestedHeight,
        int originalWidth, int originalHeight)
    {
        int width = Math.Abs(requestedWidth);
        int height = Math.Abs(requestedHeight);

        if (width == 0 && height == 0)
        {
            return (originalWidth, originalHeight);
        }

        if (width == 0)
        {
            width = originalWidth * height / originalHeight;
        }
        else if (height == 0)
        {
            height = originalHeight * width / originalWidth;
        }

        return (width, height);
    }

    private static SKBitmap ResizeBitmap(SKBitmap source, int targetWidth, int targetHeight)
    {
        return source.Resize(new SKImageInfo(targetWidth, targetHeight), ResizeSamplingOptions)
               ?? throw new NotSupportedException(
                   "No imaging component suitable to complete this operation was found.");
    }

    private static void ValidateBuffer(byte[] buffer, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            ArgumentException exception = new("Value cannot be empty.", nameof(buffer));
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    internal static SKBitmap ApplyRotation(SKBitmap source, ImageRotation rotation)
    {
        float degrees = rotation switch
        {
            ImageRotation.Rotate90 => 90f,
            ImageRotation.Rotate180 => 180f,
            ImageRotation.Rotate270 => 270f,
            _ => 0f
        };

        bool swapDimensions = rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270;
        int newWidth = swapDimensions ? source.Height : source.Width;
        int newHeight = swapDimensions ? source.Width : source.Height;

        SKBitmap rotated = new(newWidth, newHeight, source.ColorType, source.AlphaType);

        using (SKCanvas canvas = new(rotated))
        {
            canvas.Translate(newWidth / 2f, newHeight / 2f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-source.Width / 2f, -source.Height / 2f);
            canvas.DrawBitmap(source, 0, 0, RotationSamplingOptions);
        }

        return rotated;
    }

    private byte[] EncodeJpeg()
    {
        using (SKImage image = SKImage.FromBitmap(Bitmap))
        {
            using (SKData? data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
            {
                return data?.ToArray() ?? [];
            }
        }
    }

    private byte[] EncodePng()
    {
        using (SKImage image = SKImage.FromBitmap(Bitmap))
        {
            using (SKData? data = image.Encode(SKEncodedImageFormat.Png, 0))
            {
                return data?.ToArray() ?? [];
            }
        }
    }

    private byte[] EncodeGif()
    {
        using (MagickImage magickImage = CreateMagickImageFromBitmap())
        {
            return magickImage.ToByteArray(MagickFormat.Gif);
        }
    }

    private byte[] EncodeBmp()
    {
        using (MagickImage magickImage = CreateMagickImageFromBitmap())
        {
            return magickImage.ToByteArray(MagickFormat.Bmp);
        }
    }

    private MagickImage CreateMagickImageFromBitmap()
    {
        if (Bitmap.ColorType == SKColorType.Bgra8888 && Bitmap.RowBytes == Bitmap.Width * 4)
        {
            return CreateMagickImage(Bitmap.Bytes, Bitmap.Width, Bitmap.Height);
        }

        using (SKBitmap bgraBitmap = Bitmap.Copy(SKColorType.Bgra8888))
        {
            return CreateMagickImage(bgraBitmap.Bytes, bgraBitmap.Width, bgraBitmap.Height);
        }
    }

    private static MagickImage CreateMagickImage(byte[] pixelData, int width, int height)
    {
        MagickReadSettings settings = new()
        {
            Width = (uint)width,
            Height = (uint)height,
            Format = MagickFormat.Bgra,
            Depth = 8
        };

        return new(pixelData, settings);
    }
}
