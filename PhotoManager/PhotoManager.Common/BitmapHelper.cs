using ImageMagick;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.Common;

public static class BitmapHelper
{
    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public static BitmapImage LoadBitmapImage(byte[] buffer, Rotation rotation)
    {
        BitmapImage image = new();

        using (MemoryStream stream = new(buffer))
        {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = stream;
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public static BitmapImage LoadBitmapImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        BitmapImage image = null;

        using (MemoryStream stream = new(buffer))
        {
            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
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
    public static BitmapImage LoadBitmapHeicImage(byte[] buffer, Rotation rotation)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new (buffer))
            {
                using (MagickImage magickImage = new (stream))
                {
                    // Apply rotation to the MagickImage
                    if (rotation == Rotation.Rotate90)
                    {
                        magickImage.Rotate(90);
                    }
                    else if (rotation == Rotation.Rotate180)
                    {
                        magickImage.Rotate(180);
                    }
                    else if (rotation == Rotation.Rotate270)
                    {
                        magickImage.Rotate(270);
                    }

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new ())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new ();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
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
            // Image is not valid or unsupported format
            Console.WriteLine("The image is corrupted or in an unsupported format");
        }

        return image;
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public static BitmapImage LoadBitmapHeicImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        BitmapImage image = new();

        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage magickImage = new(stream))
                {
                    // Apply rotation to the MagickImage
                    if (rotation == Rotation.Rotate90)
                    {
                        magickImage.Rotate(90);
                    }
                    else if (rotation == Rotation.Rotate180)
                    {
                        magickImage.Rotate(180);
                    }
                    else if (rotation == Rotation.Rotate270)
                    {
                        magickImage.Rotate(270);
                    }

                    // Resize the MagickImage
                    magickImage.Resize(width, height);

                    // Convert the MagickImage to a BitmapImage
                    using (MemoryStream bitmapStream = new())
                    {
                        magickImage.Write(bitmapStream, MagickFormat.Bmp);
                        bitmapStream.Position = 0;

                        BitmapImage bitmapImage = new();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
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
            // Image is not valid or unsupported format
            Console.WriteLine("The image is corrupted or in an unsupported format");
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public static BitmapImage LoadBitmapImage(string imagePath, Rotation rotation)
    {
        BitmapImage image = null;

        if (File.Exists(imagePath))
        {
            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = new Uri(imagePath);
            image.Rotation = rotation;
            image.EndInit();
            image.Freeze();
        }

        return image;
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public static BitmapImage LoadBitmapHeicImage(string imagePath, Rotation rotation)
    {
        BitmapImage image = null;

        if (File.Exists(imagePath))
        {
            using (MagickImage magickImage = new (imagePath))
            {
                // Apply rotation to the MagickImage based on the Rotation value
                switch (rotation)
                {
                    case Rotation.Rotate90:
                        magickImage.Rotate(90);
                        break;
                    case Rotation.Rotate180:
                        magickImage.Rotate(180);
                        break;
                    case Rotation.Rotate270:
                        magickImage.Rotate(270);
                        break;
                        // case Rotation.Rotate0: No rotation needed for Rotate0
                }

                // Convert the MagickImage to a byte array (supported format: JPG)
                byte[] imageData = magickImage.ToByteArray(MagickFormat.Jpg);

                // Create a BitmapImage from the byte array and set the rotation
                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
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
        // TODO: If the stream is disposed by a using block, the thumbnail is not shown. Find a way to dispose of the stream.
        MemoryStream stream = new(buffer);
        BitmapImage thumbnailImage = new();
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
}
