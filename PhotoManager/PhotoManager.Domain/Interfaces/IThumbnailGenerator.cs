namespace PhotoManager.Domain.Interfaces;

public interface IThumbnailGenerator
{
    byte[] GenerateThumbnail(byte[] imageBytes, ImageRotation rotation, int width, int height,
        ImageEncodingFormat encodingFormat);
}
