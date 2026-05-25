namespace PhotoManager.Domain.Interfaces;

public interface IImageProcessingService
{
    IImageData LoadBitmapThumbnailImage(byte[] buffer, ImageRotation rotation, int width, int height);
    IImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation);
    byte[] GetJpegBitmapImage(IImageData thumbnailImage);
    byte[] GetPngBitmapImage(IImageData thumbnailImage);
    byte[] GetGifBitmapImage(IImageData thumbnailImage);
    (int width, int height) GetImageDimensions(byte[] buffer, ImageRotation rotation);
    bool IsValidGdiPlusImage(byte[] imageData);
    bool IsValidHeic(byte[] imageData);
}
