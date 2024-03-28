using ImageMagick;
using log4net;
using System.Reflection;

namespace PhotoManager.Common;

public static class ExifHelper
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    public static ushort GetExifOrientation(byte[] buffer, ushort defaultExifOrientation, ushort corruptedImageOrientation)
    {
        try
        {
            using (MemoryStream stream = new (buffer))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(stream);

                if (bitmapFrame.Metadata is BitmapMetadata bitmapMetadata)
                {
                    object? orientation = bitmapMetadata.GetQuery("System.Photo.Orientation");

                    if (orientation == null)
                    {
                        return defaultExifOrientation;
                    }

                    return (ushort)orientation;
                }
            }
        }
        catch (Exception e)
        {
            if (e is NotSupportedException && e.InnerException?.HResult == -2003292351)
            {
                Log.Error("The image is corrupted");
            }
            else
            {
                Log.Error(e);
            }
        }

        return corruptedImageOrientation;
    }

    public static ushort GetHeicExifOrientation(byte[] buffer, ushort corruptedImageOrientation)
    {
        try
        {
            using (MemoryStream stream = new (buffer))
            {
                MagickReadSettings settings = new();
                settings.SetDefine(MagickFormat.Heic, "preserve-orientation", true);

                using (MagickImage image = new (stream, settings))
                {
                    // image.Orientation contain the value from the exif data -> image.GetAttribute("exif:Orientation")
                    return GetMagickHeicOrientation(image.Orientation, corruptedImageOrientation);
                }
            }
        }
        catch (MagickException)
        {
            Log.Error("The image is not valid or in an unsupported format");
        }

        return corruptedImageOrientation;
    }

    // (ushort)1 <=> "Horizontal (normal)"
    // (ushort)2 <=> "Mirror horizontal"
    // (ushort)3 <=> "Rotate 180"
    // (ushort)4 <=> "Mirror vertical"
    // ushort)5 <=> "Mirror horizontal and rotate 270 CW"
    // (ushort)6 <=> "Rotate 90 CW"
    // (ushort)7 <=> "Mirror horizontal and rotate 90 CW"
    // (ushort)8 <=> "Rotate 270 CW"
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
                BitmapFrame.Create(ms);
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return false;
        }
    }

    public static bool IsValidHeic(byte[] imageData)
    {
        try
        {
            using (MemoryStream ms = new (imageData))
            {
                using (new MagickImage(ms))
                {
                    // Image is valid
                }
            }

            return true;
        }
        catch (MagickException)
        {
            Log.Error("The image is not valid or in an unsupported format");
            return false;
        }
    }

    // 1: Normal (0 deg rotation)
    // 3: Upside-down (180 deg rotation)
    // 6: Rotated 90 deg clockwise (270 deg counterclockwise)
    // 8: Rotated 90 deg counterclockwise (270 deg clockwise)
    private static ushort GetMagickHeicOrientation(OrientationType orientationType, ushort corruptedImageOrientation)
    {
        ushort result = orientationType
        switch
        {
            (OrientationType.TopLeft or OrientationType.LeftTop) => 1,
            (OrientationType.BottomLeft or OrientationType.LeftBottom) => 8,
            (OrientationType.BottomRight or OrientationType.RightBottom) => 3,
            (OrientationType.TopRight or OrientationType.RightTop) => 6,
            _ => corruptedImageOrientation
        };

        return result;
    }
}
