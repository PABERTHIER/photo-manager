using Microsoft.Extensions.Logging;

namespace PhotoManager.Infrastructure;

public class ImageProcessingService(ILogger<ImageProcessingService> logger) : IImageProcessingService
{
    // From AssetCreationService for CreateAsset() to get the thumbnailImage
    public IImageData LoadBitmapThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height)
    {
        return BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height, logger);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public IImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation)
    {
        return BitmapHelper.LoadBitmapImageFromPath(imagePath, rotation, logger);
    }

    public byte[] GetJpegBitmapImage(IImageData thumbnailImage)
    {
        return BitmapHelper.GetJpegBitmapImage(thumbnailImage);
    }

    public byte[] GetPngBitmapImage(IImageData thumbnailImage)
    {
        return BitmapHelper.GetPngBitmapImage(thumbnailImage);
    }

    public byte[] GetGifBitmapImage(IImageData thumbnailImage)
    {
        return BitmapHelper.GetGifBitmapImage(thumbnailImage);
    }

    public byte[] ConvertImage(string imagePath, ImageEncodingFormat targetFormat)
    {
        return BitmapHelper.ConvertImage(imagePath, targetFormat);
    }

    public (int width, int height) GetImageDimensions(byte[] buffer, ImageRotation rotation)
    {
        return BitmapHelper.GetImageDimensions(buffer, rotation, logger);
    }

    public bool IsValidImage(byte[] imageData)
    {
        return ExifHelper.IsValidImage(imageData, logger);
    }

    public bool IsValidHeic(byte[] imageData)
    {
        return ExifHelper.IsValidHeic(imageData, logger);
    }
}
