using ImageMagick;
using log4net;
using PhotoManager.Constants;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Common;

public static class ExifHelper
{

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static ushort GetExifOrientation(byte[] buffer)
    {
        using (MemoryStream stream = new(buffer))
        {
            BitmapFrame bitmapFrame = BitmapFrame.Create(stream);

            if (bitmapFrame.Metadata is BitmapMetadata bitmapMetadata)
            {
                try
                {
                    var orientation = bitmapMetadata.GetQuery("System.Photo.Orientation");

                    if (orientation == null)
                    {
                        return (ushort)Rotation.Rotate0;
                    }

                    return (ushort)orientation;
                }
                catch (Exception e)
                {
                    if (e is NotSupportedException && e.InnerException?.HResult == -2003292351)
                    {
                        Console.WriteLine("The image is corrupted");
                        return AssetConstants.OrientationCorruptedImage;
                    }

                    log.Error(e);
                }
            }

            return AssetConstants.OrientationCorruptedImage;
        }
    }

    public static ushort GetHeicExifOrientation(byte[] buffer)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                using (MagickImage image = new(stream))
                {
                    var orientation = image.GetAttribute("exif:Orientation");
                    if (orientation != null && ushort.TryParse(orientation, out ushort orientationValue))
                    {
                        return orientationValue;
                    }
                }
            }
        }
        catch (MagickException)
        {
            // Image is not valid or unsupported format
            Console.WriteLine("The image is corrupted or in an unsupported format");
        }

        return AssetConstants.OrientationCorruptedImage;
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
            using (MemoryStream ms = new (imageData))
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

    public static bool IsValidHeic(byte[] imageData)
    {
        try
        {
            using (MemoryStream ms = new(imageData))
            {
                using (MagickImage image = new(ms))
                {
                    // Image is valid
                }
            }

            return true;
        }
        catch (MagickException)
        {
            // Image is not valid or unsupported format
            return false;
        }
    }
}
