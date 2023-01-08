namespace PhotoManager.Common;

public static class VideoHelper
{
    // Todo: Add other formats
    public static bool IsVideoFile(string fileName)
    {
        return fileName.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".avi", StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase);
    }
}
