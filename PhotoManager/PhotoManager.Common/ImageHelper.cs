namespace PhotoManager.Common;

public static class ImageHelper
{
    //.bmp - Windows bitmap
    //.gif - Graphics Interchange Format
    //.ico - Icon file
    //.jpeg, .jpg - Joint Photographic Experts Group
    //.png - Portable Network Graphics
    //.tiff, .tif - Tagged Image File Format
    //.webp - WebP image
    //.dng - RAW Format
    public static bool IsImageFile(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            string s when s.EndsWith(".jpg") => true,
            string s when s.EndsWith(".jpeg") => true,
            string s when s.EndsWith(".jfif") => true,
            string s when s.EndsWith(".png") => true,
            string s when s.EndsWith(".gif") => true,
            string s when s.EndsWith(".dng") => true,
            string s when s.EndsWith(".bmp") => true,
            string s when s.EndsWith(".tiff") => true,
            string s when s.EndsWith(".tif") => true,
            _ => false
        };
    }
}
