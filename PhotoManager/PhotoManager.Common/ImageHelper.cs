namespace PhotoManager.Common;

public static class ImageHelper
{
    // Todo: Add other formats like dng...
    public static bool IsImageFile(string fileName)
    {
        return fileName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".jfif", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase);
    }

    //public void Toto()
    //{
    //    string filePath = @"C:\myfile.jpg";
    //    string fileExtension = Path.GetExtension(filePath);
    //    if (fileExtension == ".jpg"
    //        || fileExtension == ".jpeg"
    //        || fileExtension == ".png"
    //        || fileExtension == ".dng"
    //        || fileExtension == ".bmp"
    //        || fileExtension == ".tiff"
    //        || fileExtension == ".tif"
    //        || fileExtension == ".gif")
    //    {
    //        // It's an image file
    //    }
    //    else if (fileExtension == ".mov"
    //        || fileExtension == ".mp4"
    //        || fileExtension == ".avi")
    //    {
    //        // It's an video file
    //    }
    //}
}
