using ImageMagick;
using Microsoft.Extensions.Logging;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From AssetCreationService for CreateAsset() to get the originalImage
    public static BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation, ILogger logger)
    {
        try
        {
            BitmapImage image = new();

            using (MemoryStream stream = new(buffer))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = stream;
                image.Rotation = rotation;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From AssetCreationService for CreateAsset() to get the thumbnailImage
    public static BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height,
        ILogger logger)
    {
        try
        {
            BitmapImage image = new();

            using (MemoryStream stream = new(buffer))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = stream;
                image.Rotation = rotation;
                image.DecodePixelWidth = width;
                image.DecodePixelHeight = height;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            NotSupportedException exception =
                new("No imaging component suitable to complete this operation was found.");
            logger.LogError(exception, "{ExMessage}", exception.Message);
            throw exception;
        }
    }

    // From AssetCreationService for CreateAsset() to get the originalImage for HEIC
    public static BitmapImage LoadBitmapHeicOriginalImage(byte[] buffer, Rotation rotation, ILogger logger)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    // Apply Rotation because MagickImage does not rotate the image in-place
                    MagickImageApplyRotation(magickImage, rotation, true);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        // To keep the imageData after dispose of the using block
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmapImage.StreamSource = bitmapStream;
                        bitmapImage.Rotation = rotation; // Set the rotation value to save it into the BitmapImage
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        image = bitmapImage;

                        return image;
                    }
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return image;
    }

    // From AssetCreationService for CreateAsset() to get the thumbnailImage for HEIC
    public static BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height,
        ILogger logger)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    // Apply Rotation because MagickImage does not rotate the image in-place
                    MagickImageApplyRotation(magickImage, rotation, false);

                    // Resize the MagickImage
                    magickImage.Resize((uint)width, (uint)height);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        // To keep the imageData after dispose of the using block
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmapImage.StreamSource = bitmapStream;
                        bitmapImage.Rotation = rotation; // Set the rotation value to save it into the BitmapImage
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        image = bitmapImage;

                        return image;
                    }
                }
            }
        }
        catch (MagickException)
        {
            logger.LogError("The image is not valid or in an unsupported format");
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation)
    {
        BitmapImage image = new();

        if (File.Exists(imagePath))
        {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = new(imagePath);
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation, ILogger logger)
    {
        BitmapImage image = new();

        if (File.Exists(imagePath))
        {
            try
            {
                using (MagickImage magickImage = new(imagePath))
                {
                    // Apply Rotation because MagickImage does not rotate the image in-place
                    MagickImageApplyRotation(magickImage, rotation, false);

                    // Convert the MagickImage to a byte array (supported format: JPG)
                    byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                    // Create a BitmapImage from the byte array and set the rotation
                    image.BeginInit();
                    // To keep the imageData after dispose of the using block
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    image.StreamSource = new MemoryStream(imageData);
                    image.Rotation = rotation;
                    image.EndInit();
                    image.Freeze();
                }
            }
            catch (MagickException)
            {
                logger.LogError("Failed to load HEIC image from path: {imagePath}.", imagePath);
            }
        }

        return image;
    }

    // From AssetRepository
    public static BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height, ILogger logger)
    {
        try
        {
            BitmapImage thumbnailImage = new();

            using (MemoryStream stream = new(buffer))
            {
                thumbnailImage.BeginInit();
                thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                thumbnailImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                thumbnailImage.StreamSource = stream;
                thumbnailImage.DecodePixelWidth = width;
                thumbnailImage.DecodePixelHeight = height;
                thumbnailImage.EndInit();
                thumbnailImage.Freeze();
            }

            return thumbnailImage;
        }
        catch (Exception ex) when (ex is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            logger.LogError(ex, "No imaging component suitable to complete this operation was found.");
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    public static Bitmap? LoadBitmapFromPath(string imagePath)
    {
        Bitmap? image = null;

        if (File.Exists(imagePath))
        {
            using (MagickImage magickImage = new(imagePath))
            {
                // Convert the MagickImage to a byte array (supported format: JPG)
                byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                using (MemoryStream stream = new(imageData))
                {
                    using (Bitmap bitmap = new(stream))
                    {
                        // Create a copy of the Bitmap
                        // When the using block for the MemoryStream is exited, the stream is disposed of, which lead to have a default bitmap at the end and to lose all the data.
                        image = new(bitmap);
                    }
                }
            }
        }

        return image;
    }

    public static byte[] GetJpegBitmapImage(BitmapImage image)
    {
        return GetBitmapImage(image, new JpegBitmapEncoder());
    }

    public static byte[] GetPngBitmapImage(BitmapImage image)
    {
        return GetBitmapImage(image, new PngBitmapEncoder());
    }

    public static byte[] GetGifBitmapImage(BitmapImage image)
    {
        return GetBitmapImage(image, new GifBitmapEncoder());
    }

    public static (int width, int height) GetImageDimensions(byte[] buffer, Rotation rotation, ILogger logger)
    {
        (int rawWidth, int rawHeight) = TryReadDimensionsFromHeader(buffer);

        if (rawWidth <= 0 || rawHeight <= 0)
        {
            BitmapImage image = LoadBitmapOriginalImage(buffer, Rotation.Rotate0, logger);
            rawWidth = image.PixelWidth;
            rawHeight = image.PixelHeight;
        }

        return rotation is Rotation.Rotate90 or Rotation.Rotate270 ? (rawHeight, rawWidth) : (rawWidth, rawHeight);
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
                if (offset + 6 < buffer.Length)
                {
                    int height = (buffer[offset + 3] << 8) | buffer[offset + 4];
                    int width = (buffer[offset + 5] << 8) | buffer[offset + 6];
                    return (width, height);
                }

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

    private static byte[] GetBitmapImage(BitmapImage image, BitmapEncoder encoder)
    {
        byte[] imageBuffer;
        encoder.Frames.Add(BitmapFrame.Create(image));

        using (MemoryStream memoryStream = new())
        {
            encoder.Save(memoryStream);
            imageBuffer = memoryStream.ToArray();
        }

        return imageBuffer;
    }

    private static void MagickImageApplyRotation(MagickImage magickImage, Rotation rotation, bool isClockwise)
    {

        int rotationAngle = rotation switch
        {
            Rotation.Rotate90 => isClockwise ? 90 : -90,
            Rotation.Rotate180 => isClockwise ? 180 : -180,
            Rotation.Rotate270 => isClockwise ? 270 : -270,
            _ => 0
        };

        if (rotationAngle != 0)
        {
            magickImage.Rotate(rotationAngle);
        }
    }
}
