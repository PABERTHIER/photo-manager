using PhotoManager.Common.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Converters;

internal static class BitmapImageFactory
{
    public static BitmapImage? Create(IImageData imageData, ImageEncodingFormat format, bool applyRotation = true)
    {
        byte[] bytes = imageData.ToByteArray(format);

        if (bytes.Length == 0)
        {
            return null;
        }

        using (MemoryStream stream = new(bytes))
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.Rotation = applyRotation ? ToWpfRotationOrDefault(imageData.Rotation) : Rotation.Rotate0;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }

    private static Rotation ToWpfRotationOrDefault(ImageRotation rotation)
    {
        return rotation switch
        {
            ImageRotation.Rotate0 => Rotation.Rotate0,
            ImageRotation.Rotate90 => Rotation.Rotate90,
            ImageRotation.Rotate180 => Rotation.Rotate180,
            ImageRotation.Rotate270 => Rotation.Rotate270,
            _ => Rotation.Rotate0
        };
    }
}
