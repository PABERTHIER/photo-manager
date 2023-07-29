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

    // From ShowImage() in ViewerUserControl
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
