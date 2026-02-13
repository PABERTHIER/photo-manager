namespace PhotoManager.Common;

public static class ImageHelper
{
    //.bmp - Windows bitmap
    //.dng - RAW Format
    //.gif - Graphics Interchange Format
    //.heic - Apple Format
    //.ico - Icon file
    //.jfif - JPEG File Interchange Format
    //.jpeg, .jpg - Joint Photographic Experts Group
    //.png - Portable Network Graphics
    //.tiff, .tif - Tagged Image File Format
    //.webp - WebP image
    public static bool IsImageFile(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            { } s when s.EndsWith(".bmp") => true,
            { } s when s.EndsWith(".dng") => true,
            { } s when s.EndsWith(".gif") => true,
            { } s when s.EndsWith(".heic") => true,
            { } s when s.EndsWith(".ico") => true,
            { } s when s.EndsWith(".jfif") => true,
            { } s when s.EndsWith(".jpeg") => true,
            { } s when s.EndsWith(".jpg") => true,
            { } s when s.EndsWith(".png") => true,
            { } s when s.EndsWith(".tiff") => true,
            { } s when s.EndsWith(".tif") => true,
            { } s when s.EndsWith(".webp") => true,
            _ => false
        };
    }
}
