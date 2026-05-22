namespace PhotoManager.Common.Imaging;

public interface IImageData : IDisposable
{
    int Width { get; }
    int Height { get; }
    ImageRotation Rotation { get; }
    byte[] ToByteArray(ImageEncodingFormat format);
    Stream ToStream(ImageEncodingFormat format);
}
