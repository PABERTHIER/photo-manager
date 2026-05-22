namespace PhotoManager.Common;

/// <summary>
/// Temporary WPF-backed implementation of <see cref="IImageData"/>.
/// Wraps a <see cref="BitmapImage"/> for the transition period until BitmapHelper
/// is rewritten with SkiaSharp (Phase 1.3).
/// </summary>
public sealed class BitmapImageData(BitmapImage bitmapImage, ImageRotation rotation) : IImageData
{
    public BitmapImageData(BitmapImage bitmapImage) : this(bitmapImage, MapFromWpfRotation(bitmapImage.Rotation))
    {
    }

    public int Width => BitmapImage.PixelWidth;
    public int Height => BitmapImage.PixelHeight;
    public ImageRotation Rotation { get; } = rotation;
    public BitmapImage BitmapImage { get; } = bitmapImage;

    public byte[] ToByteArray(ImageEncodingFormat format)
    {
        BitmapEncoder encoder = format switch
        {
            ImageEncodingFormat.Jpeg => new JpegBitmapEncoder(),
            ImageEncodingFormat.Png => new PngBitmapEncoder(),
            ImageEncodingFormat.Gif => new GifBitmapEncoder(),
            ImageEncodingFormat.Bmp => new BmpBitmapEncoder(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        encoder.Frames.Add(BitmapFrame.Create(BitmapImage));

        using (MemoryStream stream = new())
        {
            encoder.Save(stream);
            return stream.ToArray();
        }
    }

    public Stream ToStream(ImageEncodingFormat format)
    {
        MemoryStream stream = new();
        BitmapEncoder encoder = format switch
        {
            ImageEncodingFormat.Jpeg => new JpegBitmapEncoder(),
            ImageEncodingFormat.Png => new PngBitmapEncoder(),
            ImageEncodingFormat.Gif => new GifBitmapEncoder(),
            ImageEncodingFormat.Bmp => new BmpBitmapEncoder(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        encoder.Frames.Add(BitmapFrame.Create(BitmapImage));
        encoder.Save(stream);
        stream.Position = 0;

        return stream;
    }

    public void Dispose()
    {
        // BitmapImage is frozen (immutable) in WPF and does not require explicit disposal
    }

    public static Rotation ToWpfRotation(ImageRotation rotation)
    {
        return rotation switch
        {
            ImageRotation.Rotate0 => System.Windows.Media.Imaging.Rotation.Rotate0,
            ImageRotation.Rotate90 => System.Windows.Media.Imaging.Rotation.Rotate90,
            ImageRotation.Rotate180 => System.Windows.Media.Imaging.Rotation.Rotate180,
            ImageRotation.Rotate270 => System.Windows.Media.Imaging.Rotation.Rotate270,
            _ => throw new ArgumentException(
                $"'{(int)rotation}' is not a valid value for property 'Rotation'.")
        };
    }

    public static ImageRotation MapFromWpfRotation(Rotation wpfRotation)
    {
        return wpfRotation switch
        {
            System.Windows.Media.Imaging.Rotation.Rotate0 => ImageRotation.Rotate0,
            System.Windows.Media.Imaging.Rotation.Rotate90 => ImageRotation.Rotate90,
            System.Windows.Media.Imaging.Rotation.Rotate180 => ImageRotation.Rotate180,
            System.Windows.Media.Imaging.Rotation.Rotate270 => ImageRotation.Rotate270,
            _ => ImageRotation.Rotate0
        };
    }
}
