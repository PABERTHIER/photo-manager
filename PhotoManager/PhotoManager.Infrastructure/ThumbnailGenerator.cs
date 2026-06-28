namespace PhotoManager.Infrastructure;

public class ThumbnailGenerator(IImageProcessingService imageProcessingService) : IThumbnailGenerator
{
    public byte[] GenerateThumbnail(byte[] imageBytes, ImageRotation rotation, int width, int height,
        ImageEncodingFormat encodingFormat)
    {
        using (IImageData thumbnailImage =
               imageProcessingService.LoadBitmapThumbnailImage(imageBytes, rotation, width, height))
        {
            return encodingFormat switch
            {
                ImageEncodingFormat.Png => imageProcessingService.GetPngBitmapImage(thumbnailImage),
                ImageEncodingFormat.Gif => imageProcessingService.GetGifBitmapImage(thumbnailImage),
                ImageEncodingFormat.Jpeg => imageProcessingService.GetJpegBitmapImage(thumbnailImage),
                ImageEncodingFormat.Bmp => thumbnailImage.ToByteArray(ImageEncodingFormat.Bmp),
                _ => throw new ArgumentOutOfRangeException(nameof(encodingFormat), encodingFormat, null)
            };
        }
    }
}
