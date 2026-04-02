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
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        if (extension.IsEmpty)
        {
            return false;
        }

        return extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".dng", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".heic", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".ico", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jfif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".tif", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
    }
}
