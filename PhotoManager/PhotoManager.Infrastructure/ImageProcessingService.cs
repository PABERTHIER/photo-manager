namespace PhotoManager.Infrastructure;

public class ImageProcessingService : IImageProcessingService
{
    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage
    public BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        return BitmapHelper.LoadBitmapThumbnailImage(buffer, rotation, width, height);
    }

    // From AssetRepository
    public BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height)
    {
        return BitmapHelper.LoadBitmapThumbnailImage(buffer, width, height);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage
    public BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapOriginalImage(buffer, rotation);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode
    public BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapImageFromPath(imagePath, rotation);
    }

    // From CatalogAssetsService for CreateAsset() to get the originalImage for HEIC
    public BitmapImage LoadBitmapHeicOriginalImage(byte[] imageBytes, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapHeicOriginalImage(imageBytes, rotation);
    }

    // From CatalogAssetsService for CreateAsset() to get the thumbnailImage for HEIC
    public BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height)
    {
        return BitmapHelper.LoadBitmapHeicThumbnailImage(buffer, rotation, width, height);
    }

    // From ShowImage() in ViewerUserControl to open the image in fullscreen mode for Heic
    public BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation)
    {
        return BitmapHelper.LoadBitmapHeicImageFromPath(imagePath, rotation);
    }

    public byte[] GetJpegBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetJpegBitmapImage(thumbnailImage);
    }

    public byte[] GetPngBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetPngBitmapImage(thumbnailImage);
    }

    public byte[] GetGifBitmapImage(BitmapImage thumbnailImage)
    {
        return BitmapHelper.GetGifBitmapImage(thumbnailImage);
    }

    public bool IsValidGDIPlusImage(byte[] imageData)
    {
        return ExifHelper.IsValidGDIPlusImage(imageData);
    }

    public bool IsValidHeic(byte[] imageData)
    {
        return ExifHelper.IsValidHeic(imageData);
    }
}
