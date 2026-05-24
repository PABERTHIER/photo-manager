using ImageMagick;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From AssetCreationService for CreateAsset() to get the originalImage
    public static SkiaImageData LoadBitmapOriginalImage(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        ValidateBuffer(buffer);

        try
        {
            return SkiaImageData.FromEncodedBytesWithRotation(buffer, rotation);
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From AssetCreationService for CreateAsset() to get the thumbnailImage
    public static SkiaImageData LoadBitmapThumbnailImage(byte[] buffer, ImageRotation rotation, int width,
        int height, ILogger logger)
    {
        ValidateBuffer(buffer);

        try
        {
            return SkiaImageData.FromEncodedBytesWithRotation(buffer, rotation, width, height);
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From AssetCreationService for CreateAsset() to get the originalImage for HEIC
    public static SkiaImageData LoadBitmapHeicOriginalImage(byte[] buffer, ImageRotation rotation, ILogger logger)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    // HEIC codec (libheif) auto-orients based on EXIF, no manual rotation needed
                    SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
                    return new(bitmap, rotation);
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return SkiaImageData.Empty();
    }

    // From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC
    public static SkiaImageData LoadBitmapHeicThumbnailImage(byte[] buffer, ImageRotation rotation, int width,
        int height, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty.", nameof(buffer));
        }

        try
        {
            int targetWidth = Math.Abs(width);
            int targetHeight = Math.Abs(height);

            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    // HEIC codec (libheif) auto-orients based on EXIF, no manual rotation needed

                    if (targetWidth > 0 || targetHeight > 0)
                    {
                        if (targetWidth == 0)
                        {
                            targetWidth = (int)magickImage.Width;
                        }

                        if (targetHeight == 0)
                        {
                            targetHeight = (int)magickImage.Height;
                        }

                        magickImage.Resize((uint)targetWidth, (uint)targetHeight);
                    }

                    SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
                    return new(bitmap, rotation);
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return SkiaImageData.Empty();
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static SkiaImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation)
    {
        if (File.Exists(imagePath))
        {
            byte[] buffer = File.ReadAllBytes(imagePath);

            if (IsHeicFormat(buffer))
            {
                using (MemoryStream stream = new(buffer))
                {
                    using (MagickImage magickImage = new(stream))
                    {
                        SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
                        return SkiaImageData.FromBitmapWithRotation(bitmap, rotation);
                    }
                }
            }

            return SkiaImageData.FromEncodedBytesWithRotation(buffer, rotation);
        }

        return SkiaImageData.Empty();
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static SkiaImageData LoadBitmapHeicImageFromPath(string imagePath, ImageRotation rotation, ILogger logger)
    {
        if (File.Exists(imagePath))
        {
            try
            {
                using (MagickImage magickImage = new(imagePath))
                {
                    SKBitmap bitmap = MagickImageToSkBitmap(magickImage);
                    return SkiaImageData.FromBitmapWithRotation(bitmap, rotation);
                }
            }
            catch (MagickException)
            {
                logger.LogError("Failed to load HEIC image from path: {imagePath}.", imagePath);
            }
        }

        return SkiaImageData.Empty();
    }

    // From AssetRepository
    // TODO: Because the rotation is not used, it impacts the ImageByteSizes for the rotated images
    public static SkiaImageData LoadBitmapThumbnailImage(byte[] buffer, int width, int height, ILogger logger)
    {
        ValidateBuffer(buffer);

        try
        {
            if (IsHeicFormat(buffer))
            {
                // SkiaSharp cannot decode HEIC — decode with MagickImage, then resize with SkiaSharp
                byte[] decodedBuffer = DecodeHeicToBmp(buffer);
                return SkiaImageData.FromEncodedBytes(decodedBuffer, ImageRotation.Rotate0, width, height);
            }

            return SkiaImageData.FromEncodedBytes(buffer, ImageRotation.Rotate0, width, height);
        }
        catch (Exception ex) when (ex is not OverflowException)
        {
            logger.LogError(ex, "No imaging component suitable to complete this operation was found.");
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    // From HashingHelper.CalculateDHash for loading any image file to SKBitmap for pixel access
    public static SkiaImageData? LoadBitmapFromPath(string imagePath)
    {
        if (File.Exists(imagePath))
        {
            byte[] buffer = File.ReadAllBytes(imagePath);

            if (IsHeicFormat(buffer))
            {
                try
                {
                    using (MemoryStream stream = new(buffer))
                    {
                        using (MagickImage magickImage = new(stream))
                        {
                            return new(MagickImageToSkBitmap(magickImage), ImageRotation.Rotate0);
                        }
                    }
                }
                catch (MagickException)
                {
                    return null;
                }
            }

            try
            {
                SKBitmap? decoded = SKBitmap.Decode(buffer);

                if (decoded != null)
                {
                    return new(decoded, ImageRotation.Rotate0);
                }
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        return null;
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
                        using (MagickImage magickImage = new(stream))
                        {
                            rawWidth = (int)magickImage.Width;
                            rawHeight = (int)magickImage.Height;
                        }
                    }
                }
                else
                {
                    SkiaImageData image = LoadBitmapOriginalImage(buffer, ImageRotation.Rotate0, logger);
                    rawWidth = image.Width;
                    rawHeight = image.Height;
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
        SKBitmap bitmap = SKBitmap.Decode(bmpData)
            ?? throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        return bitmap;
    }

    private static byte[] DecodeHeicToBmp(byte[] buffer)
    {
        using (MemoryStream stream = new(buffer))
        {
            using (MagickImage magickImage = new(stream))
            {
                return magickImage.ToByteArray(MagickFormat.Bmp);
            }
        }
    }

    private static bool IsHeicFormat(byte[] buffer)
    {
        // HEIC/HEIF container: bytes 4-7 are "ftyp" (ISO base media file format box)
        if (buffer.Length < 12)
        {
            return false;
        }

        return buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70;
    }
}
