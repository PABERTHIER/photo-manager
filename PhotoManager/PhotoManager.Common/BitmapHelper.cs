using ImageMagick;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From AssetCreationService for CreateAsset() to get the thumbnailImage
    public static SkiaImageData LoadBitmapThumbnailImage(byte[] buffer, ImageRotation rotation, int width,
        int height, ILogger logger)
    {
        ValidateBuffer(buffer);

        try
        {
            if (IsHeicFormat(buffer))
            {
                return LoadHeicThumbnailImage(buffer, rotation, width, height, logger);
            }

            return SkiaImageData.FromEncodedBytesWithRotation(buffer, rotation, width, height, logger);
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static SkiaImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation, ILogger logger)
    {
        if (!File.Exists(imagePath))
        {
            return SkiaImageData.Empty();
        }

        return LoadBitmapOriginalImage(imagePath, rotation, logger);
    }

    // From HashingHelper.CalculateDHash for loading any image file to SKBitmap for pixel access
    public static SkiaImageData? LoadBitmapFromPath(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            return null;
        }

        using (FileStream fileStream = File.OpenRead(imagePath))
        {
            if (IsHeicFormat(fileStream))
            {
                try
                {
                    return LoadHeicImageFromFile(imagePath);
                }
                catch (MagickException)
                {
                    return null;
                }
            }

            fileStream.Position = 0;

            try
            {
                return SkiaImageData.FromEncodedStreamWithRotation(fileStream, ImageRotation.Rotate0);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public static byte[] GetJpegBitmapImage(IImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return image.ToByteArray(ImageEncodingFormat.Jpeg);
    }

    public static byte[] GetPngBitmapImage(IImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return image.ToByteArray(ImageEncodingFormat.Png);
    }

    public static byte[] GetGifBitmapImage(IImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return image.ToByteArray(ImageEncodingFormat.Gif);
    }

    public static (int width, int height) GetImageDimensions(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        (int rawWidth, int rawHeight) = TryReadDimensionsFromHeader(buffer);

        if (rawWidth <= 0 || rawHeight <= 0)
        {
            try
            {
                if (IsHeicFormat(buffer))
                {
                    using (MemoryStream stream = new(buffer))
                    {
                        MagickReadSettings settings = new();
                        settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

                        using (MagickImage magickImage = new(stream, settings))
                        {
                            rawWidth = (int)magickImage.Width;
                            rawHeight = (int)magickImage.Height;
                        }
                    }
                }
                else
                {
                    using (SkiaImageData image = LoadBitmapOriginalImage(buffer, ImageRotation.Rotate0, logger))
                    {
                        rawWidth = image.Width;
                        rawHeight = image.Height;
                    }
                }
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OverflowException)
            {
                NotSupportedException notSupportedException =
                    new("No imaging component suitable to complete this operation was found.", ex);
                logger.LogError(notSupportedException, "{ExMessage}", notSupportedException.Message);
                throw notSupportedException;
            }
        }

        return rotation is ImageRotation.Rotate90 or ImageRotation.Rotate270
            ? (rawHeight, rawWidth)
            : (rawWidth, rawHeight);
    }

    private static SkiaImageData LoadBitmapOriginalImage(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ValidateBuffer(buffer);

        try
        {
            return SkiaImageData.FromEncodedBytesWithRotation(buffer, rotation, logger);
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    private static SkiaImageData LoadBitmapOriginalImage(string imagePath, ImageRotation rotation, ILogger logger)
    {
        try
        {
            using (FileStream fileStream = File.OpenRead(imagePath))
            {
                if (IsHeicFormat(fileStream))
                {
                    return LoadHeicOriginalImageFromFile(imagePath, rotation);
                }

                fileStream.Position = 0;
                return SkiaImageData.FromEncodedStreamWithRotation(fileStream, rotation);
            }
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    private static SkiaImageData LoadHeicOriginalImageFromFile(string imagePath, ImageRotation rotation)
    {
        MagickReadSettings settings = new();
        settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

        using (MagickImage magickImage = new(imagePath, settings))
        {
            SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
            return SkiaImageData.FromBitmapWithRotation(bitmap, rotation);
        }
    }

    private static SkiaImageData LoadHeicImageFromFile(string imagePath)
    {
        using (MagickImage magickImage = new(imagePath))
        {
            return new(MagickImageToSkBitmap(magickImage), ImageRotation.Rotate0);
        }
    }

    private static SkiaImageData LoadHeicThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height,
        ILogger logger)
    {
        try
        {
            int targetWidth = Math.Abs(width);
            int targetHeight = Math.Abs(height);

            using (MemoryStream stream = new(buffer))
            {
                MagickReadSettings settings = new();
                settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

                using (MagickImage magickImage = new(stream, settings))
                {
                    SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
                    SKBitmap? rotated = null;

                    try
                    {
                        SKBitmap source = bitmap;

                        if (rotation != ImageRotation.Rotate0)
                        {
                            rotated = SkiaImageData.ApplyRotation(bitmap, rotation);
                            source = rotated;
                        }

                        (int finalWidth, int finalHeight) = CalculateFitDimensions(targetWidth, targetHeight,
                            source.Width, source.Height);

                        if (finalWidth == source.Width && finalHeight == source.Height)
                        {
                            if (source == rotated)
                            {
                                rotated = null;
                                return new(source, ImageRotation.Rotate0);
                            }

                            return new(CloneBitmap(source), ImageRotation.Rotate0);
                        }

                        SKBitmap resized = ResizeBitmapInternal(source, finalWidth, finalHeight);
                        return new(resized, ImageRotation.Rotate0);
                    }
                    finally
                    {
                        rotated?.Dispose();
                        bitmap.Dispose();
                    }
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return SkiaImageData.Empty();
    }

    private static (int width, int height) TryReadDimensionsFromHeader(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 4)
        {
            return (-1, -1);
        }

        // JPEG: starts with FF D8
        if (buffer[0] == 0xFF && buffer[1] == 0xD8)
        {
            return ReadJpegDimensions(buffer);
        }

        // PNG: 89 50 4E 47 signature. IHDR at offset 8: length(4) + "IHDR"(4) + width(4 BE) + height(4 BE)
        if (buffer.Length >= 24 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
        {
            int width = (buffer[16] << 24) | (buffer[17] << 16) | (buffer[18] << 8) | buffer[19];
            int height = (buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23];
            return (width, height);
        }

        // GIF: "GIF8" prefix (GIF87a or GIF89a). Width and height are LE 16-bit at offsets 6 and 8.
        if (buffer.Length >= 10 && buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
        {
            int width = buffer[6] | (buffer[7] << 8);
            int height = buffer[8] | (buffer[9] << 8);
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
                if (offset + 6 >= buffer.Length)
                {
                    return (-1, -1);
                }

                int height = (buffer[offset + 3] << 8) | buffer[offset + 4];
                int width = (buffer[offset + 5] << 8) | buffer[offset + 6];
                return (width, height);
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

    private static void ValidateBuffer(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(buffer));
        }
    }

    private static SKBitmap MagickImageToSkBitmap(MagickImage magickImage)
    {
        byte[] bmpData = magickImage.ToByteArray(MagickFormat.Bmp);
        return SKBitmap.Decode(bmpData);
    }

    private static bool IsHeicFormat(ReadOnlySpan<byte> buffer)
    {
        // HEIC/HEIF container: bytes 4-7 are "ftyp" (ISO base media file format box)
        if (buffer.Length < 12)
        {
            return false;
        }

        return buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70;
    }

    private static bool IsHeicFormat(Stream stream)
    {
        Span<byte> header = stackalloc byte[12];
        int bytesRead = stream.Read(header);
        return bytesRead >= 12 && IsHeicFormat(header);
    }

    private static readonly SKSamplingOptions ResizeSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    private static SKBitmap ResizeBitmapInternal(SKBitmap source, int targetWidth, int targetHeight)
    {
        return source.Resize(new SKImageInfo(targetWidth, targetHeight), ResizeSamplingOptions);
    }

    private static SKBitmap CloneBitmap(SKBitmap source)
    {
        SKBitmap clone = new(source.Width, source.Height, source.ColorType, source.AlphaType);
        source.CopyTo(clone);
        return clone;
    }

    // Fits within bounding box preserving aspect ratio without upscaling (matches MagickGeometry)
    private static (int Width, int Height) CalculateFitDimensions(int requestedWidth, int requestedHeight,
        int sourceWidth, int sourceHeight)
    {
        if (requestedWidth == 0 && requestedHeight == 0)
        {
            return (sourceWidth, sourceHeight);
        }

        double scaleX = requestedWidth > 0 ? (double)requestedWidth / sourceWidth : double.MaxValue;
        double scaleY = requestedHeight > 0 ? (double)requestedHeight / sourceHeight : double.MaxValue;
        double scale = Math.Min(scaleX, scaleY);

        if (scale >= 1.0)
        {
            return (sourceWidth, sourceHeight);
        }

        int finalWidth = Math.Max(1, (int)Math.Round(sourceWidth * scale, MidpointRounding.AwayFromZero));
        int finalHeight = Math.Max(1, (int)Math.Round(sourceHeight * scale, MidpointRounding.AwayFromZero));
        return (finalWidth, finalHeight);
    }
}
