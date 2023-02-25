namespace PhotoManager.Common;

public static class VideoHelper
{
    //.3g2 - Mobile video
    //.3gp - Mobile video
    //.asf - Advanced Systems Format
    //.avi - Audio Video Interleave
    //.flv - Flash video
    //.m4v - MP4 video
    //.mkv - Matroska video
    //.mov - QuickTime movie
    //.mp4 - MP4 video
    //.mpeg - Moving Picture Experts Group
    //.mpg - Moving Picture Experts Group
    //.ogv - Ogg Vorbis video
    //.webm / av1 - WebM video
    //.wmv - Windows Media Video
    public static bool IsVideoFile(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            string s when s.EndsWith(".mov") => true,
            string s when s.EndsWith(".avi") => true,
            string s when s.EndsWith(".av1") => true,
            string s when s.EndsWith(".mp4") => true,
            string s when s.EndsWith(".3g2") => true,
            string s when s.EndsWith(".3gp") => true,
            string s when s.EndsWith(".asf") => true,
            string s when s.EndsWith(".flv") => true,
            string s when s.EndsWith(".m4v") => true,
            string s when s.EndsWith(".mkv") => true,
            string s when s.EndsWith(".mpeg") => true,
            string s when s.EndsWith(".mpg") => true,
            string s when s.EndsWith(".ogv") => true,
            string s when s.EndsWith(".webm") => true,
            string s when s.EndsWith(".wmv") => true,
            _ => false
        };
    }
}


