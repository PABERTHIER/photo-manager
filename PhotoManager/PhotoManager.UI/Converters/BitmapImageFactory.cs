using PhotoManager.Common.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.UI.Converters;

internal static class BitmapImageFactory
{
    public static BitmapImage? Create(IImageData imageData, ImageEncodingFormat format)
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
            bitmapImage.Rotation = Rotation.Rotate0;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
