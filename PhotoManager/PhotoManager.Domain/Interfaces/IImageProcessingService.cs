namespace PhotoManager.Domain.Interfaces;

public interface IImageProcessingService
{
    BitmapImage LoadBitmapThumbnailImage(byte[] buffer, Rotation rotation, int width, int height);
    BitmapImage LoadBitmapThumbnailImage(byte[] buffer, int width, int height);
    BitmapImage LoadBitmapOriginalImage(byte[] buffer, Rotation rotation);
    BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation);
    BitmapImage LoadBitmapHeicOriginalImage(byte[] imageBytes, Rotation rotation);
    BitmapImage LoadBitmapHeicThumbnailImage(byte[] buffer, Rotation rotation, int width, int height);
    BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation);
    byte[] GetJpegBitmapImage(BitmapImage thumbnailImage);
    byte[] GetPngBitmapImage(BitmapImage thumbnailImage);
    byte[] GetGifBitmapImage(BitmapImage thumbnailImage);
    bool IsValidGDIPlusImage(byte[] imageData);
    bool IsValidHeic(byte[] imageData);
}
