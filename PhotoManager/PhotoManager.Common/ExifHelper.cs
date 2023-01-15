using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoManager.Common;

public static class ExifHelper
{
    public static ushort? GetExifOrientation(byte[] buffer)
    {
        ushort? result = null;

        using (MemoryStream stream = new(buffer))
        {
            BitmapFrame bitmapFrame = BitmapFrame.Create(stream);
            BitmapMetadata? bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;

            if (bitmapMetadata != null && bitmapMetadata.ContainsQuery("System.Photo.Orientation"))
            {
                object value = bitmapMetadata.GetQuery("System.Photo.Orientation");

                if (value != null)
                {
                    result = (ushort)value;
                }
            }
        }

        return result;
    }

    public static Rotation GetImageRotation(ushort exifOrientation)
    {
        Rotation rotation = exifOrientation switch
        {
            1 => Rotation.Rotate0,
            2 => Rotation.Rotate0, // FlipX
            3 => Rotation.Rotate180,
            4 => Rotation.Rotate180, // FlipX
            5 => Rotation.Rotate90, // FlipX
            6 => Rotation.Rotate90,
            7 => Rotation.Rotate270, // FlipX
            8 => Rotation.Rotate270,
            _ => Rotation.Rotate0,
        };

        return rotation;
    }

    public static bool IsValidGDIPlusImage(byte[] imageData)
    {
        try
        {
            using (var ms = new MemoryStream(imageData))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(ms);
                BitmapMetadata? bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
