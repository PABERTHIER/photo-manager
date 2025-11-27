using ImageMagick;
using log4net;
using System.Reflection;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public static BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation)
    {
        try
        {
            BitmapImage image = new();

            using (MemoryStream stream = new (buffer))
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
        catch (Exception e) when (e is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public static BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        try
        {
            BitmapImage image = new();

            using (MemoryStream stream = new (buffer))
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
        catch (Exception e) when (e is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public static BitmapImage LoadBitmapHeicOriginalImage(byte[] buffer, Rotation rotation)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new (buffer))
            {
                using (MagickImage magickImage = new (stream))
                {
                    MagickImageApplyRotation(magickImage, rotation, true); // Apply Rotation because MagickImage does not rotate the image in-place

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
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
            Log.Error("The image is not valid or in an unsupported format");
        }

        return image;
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public static BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new (buffer))
            {
                using (MagickImage magickImage = new (stream))
                {
                    MagickImageApplyRotation(magickImage, rotation, false); // Apply Rotation because MagickImage does not rotate the image in-place

                    // Resize the MagickImage
                    magickImage.Resize((uint)width, (uint)height);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
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
            Log.Error("The image is not valid or in an unsupported format");
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
            image.UriSource = new Uri(imagePath);
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation)
    {
        BitmapImage image = new();

        if (File.Exists(imagePath))
        {
            using (MagickImage magickImage = new (imagePath))
            {
                MagickImageApplyRotation(magickImage, rotation, false); // Apply Rotation because MagickImage does not rotate the image in-place

                // Convert the MagickImage to a byte array (supported format: JPG)
                byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                // Create a BitmapImage from the byte array and set the rotation
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after dispose of the using block
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = new MemoryStream(imageData);
                image.Rotation = rotation;
                image.EndInit();
                image.Freeze();
            }
        }

        return image;
    }

    // From AssetRepository
    public static BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height)
    {
        try
        {
            BitmapImage thumbnailImage = new();

            using (MemoryStream stream = new (buffer))
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
        catch (Exception e) when (e is not ArgumentException and not ArgumentNullException and not OverflowException)
        {
            throw new NotSupportedException("No imaging component suitable to complete this operation was found.");
        }
    }

    public static Bitmap? LoadBitmapFromPath(string imagePath)
    {
        Bitmap? image = null;

        if (File.Exists(imagePath))
        {
            using (MagickImage magickImage = new (imagePath))
            {
                // Convert the MagickImage to a byte array (supported format: JPG)
                byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                using (MemoryStream stream = new (imageData))
                {
                    using (Bitmap bitmap = new (stream))
                    {
                        // Create a copy of the Bitmap
                        // When the using block for the MemoryStream is exited, the stream is disposed of, which lead to have a default bitmap at the end and to lose all the data.
                        image = new Bitmap(bitmap);
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
        int rotationAngle = 0;

        switch (rotation)
        {
            case Rotation.Rotate90:
                rotationAngle = isClockwise ? 90 : -90;
                break;
            case Rotation.Rotate180:
                rotationAngle = isClockwise ? 180 : -180;
                break;
            case Rotation.Rotate270:
                rotationAngle = isClockwise ? 270 : -270;
                break;
        }

        if (rotationAngle != 0)
        {
            magickImage.Rotate(rotationAngle);
        }
    }
}
