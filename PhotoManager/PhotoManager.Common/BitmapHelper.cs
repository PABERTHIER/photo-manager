using ImageMagick;
using log4net;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public static BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation)
    {
        BitmapImage image = new();

        using (MemoryStream stream = new(buffer))
        {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = stream;
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public static BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        BitmapImage image = new();

        using (MemoryStream stream = new(buffer))
        {
            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
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
                    MagickImageApplyRotation(magickImage, rotation);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new ())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new ();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmapImage.StreamSource = bitmapStream;
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
            log.Error("The image is not valid or in an unsupported format");
        }

        return image;
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public static BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    MagickImageApplyRotation(magickImage, rotation);

                    // Resize the MagickImage
                    magickImage.Resize(width, height);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
                        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                        bitmapImage.StreamSource = bitmapStream;
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
            log.Error("The image is not valid or in an unsupported format");
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation)
    {
        BitmapImage image = new();

        if (File.Exists(imagePath))
        {
            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
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
                MagickImageApplyRotation(magickImage, rotation);

                // Convert the MagickImage to a byte array (supported format: JPG)
                byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                // Create a BitmapImage from the byte array and set the rotation
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // To keep the imageData after the dispose of the using block
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
    public static BitmapImage LoadBitmapImage(byte[] buffer, int width, int height)
    {
        BitmapImage thumbnailImage = new();

        // TODO: If the stream is disposed by a using block, the thumbnail is not shown. Find a way to dispose of the stream.
        // When the using block for the MemoryStream is exited, the stream is disposed of, which lead to have a default bitmap at the end and to lose all the data.
        MemoryStream stream = new(buffer);
        thumbnailImage.BeginInit();
        thumbnailImage.CacheOption = BitmapCacheOption.None;
        thumbnailImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        thumbnailImage.StreamSource = stream;
        thumbnailImage.DecodePixelWidth = width;
        thumbnailImage.DecodePixelHeight = height;
        thumbnailImage.EndInit();
        thumbnailImage.Freeze();

        return thumbnailImage;
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

        using (var memoryStream = new MemoryStream())
        {
            encoder.Save(memoryStream);
            imageBuffer = memoryStream.ToArray();
        }

        return imageBuffer;
    }

    private static void MagickImageApplyRotation(MagickImage magickImage, Rotation rotation)
    {
        // Apply rotation to the MagickImage based on the Rotation value
        switch (rotation)
        {
            // Apply rotation to the MagickImage
            // Case Rotation.Rotate0: No rotation needed for Rotate0
            case Rotation.Rotate90:
                magickImage.Rotate(90);
                break;
            case Rotation.Rotate180:
                magickImage.Rotate(180);
                break;
            case Rotation.Rotate270:
                magickImage.Rotate(270);
                break;
        }
    }
}
