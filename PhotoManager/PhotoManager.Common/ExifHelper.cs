using ImageMagick;
using log4net;
using System.Reflection;

namespace PhotoManager.Common;

public static class ExifHelper
{

    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg counterclockwise (270 deg clockwise)
    // 8: Rotated 90 deg clockwise (270 deg counterclockwise)
    public static ushort GetExifOrientation(byte[] buffer)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(stream);

                if (bitmapFrame.Metadata is BitmapMetadata bitmapMetadata)
                {
                
                    object orientation = bitmapMetadata.GetQuery("System.Photo.Orientation");

                    if (orientation == null)
                    {
                        return AssetConstants.DefaultExifOrientation;
                    }

                    return (ushort)orientation;
                
                }
            }
        }
        catch (Exception e)
        {
            if (e is NotSupportedException && e.InnerException?.HResult == -2003292351)
            {
                log.Error("The image is corrupted");
            }
            else
            {
                log.Error(e);
            }
        }

        return AssetConstants.OrientationCorruptedImage;
    }

    public static ushort GetHeicExifOrientation(byte[] buffer)
    {
        try
        {
            using (MemoryStream stream = new(buffer))
            {
                // https://github.com/dlemstra/Magick.NET/issues/836
                //MagickReadSettings settings = new();
                //settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

                //using (MagickImage image = new(stream, settings))
                using (MagickImage image = new(stream))
                {
                    string? orientation = image.GetAttribute("exif:Orientation");
                    //image.AutoOrient();
                    //image.GetExifProfile();
                    if (orientation != null && ushort.TryParse(orientation, out ushort orientationValue))
                    {
                        return orientationValue;
                    }
                    else
                    {
                        return GetMagickHeicOrientation(image.Orientation);
                    }
                }
            }
        }
        catch (MagickException)
        {
            log.Error("The image is not valid or in an unsupported format");
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
        catch (Exception e)
        {
            log.Error(e);
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
            log.Error("The image is not valid or in an unsupported format");
            return false;
        }
    }

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg counterclockwise (270 deg clockwise)
    // 8: Rotated 90 deg clockwise (270 deg counterclockwise)
    private static ushort GetMagickHeicOrientation(OrientationType orientationType)
    {
        ushort result = orientationType
        switch
        {
            (OrientationType.TopLeft or OrientationType.LeftTop) => 1,
            (OrientationType.BottomLeft or OrientationType.LeftBotom) => 8,
            (OrientationType.BottomRight or OrientationType.RightBottom) => 3,
            (OrientationType.TopRight or OrientationType.RightTop) => 6,
            OrientationType.Undefined => AssetConstants.OrientationCorruptedImage,
            _ => AssetConstants.OrientationCorruptedImage
        };

        return result;
    }
}
