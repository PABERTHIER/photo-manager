namespace PhotoManager.Common;

public static class VideoHelper
{
    public static IProcessExecutor ProcessExecutor { get; set; } = new ProcessExecutor();

    //.3g2 - Mobile video
    //.3gp - Mobile video
    //.asf - Advanced Systems Format
    //.av1 - Video coding format for videos transmissions
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

    public static string GetFirstFramePath(string directoryName, string fileName, string destinationPath)
    {
        string videoPath = Path.Combine(directoryName, fileName);

        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(destinationPath);

        // Set the FFmpeg executable path
        string ffmpegPath = PathConstants.FfmpegPath;

        // Set the output file name based on the input video file name
        string firstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string firstFrameVideoPath = Path.Combine(destinationPath, firstFrameVideoName);

        try
        {
            // Execute FFmpeg command to extract the first frame
            string arguments = $"-i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{firstFrameVideoPath}\"";
            ProcessExecutor.ExecuteFFmpegCommand(ffmpegPath, arguments);

            Console.WriteLine($"First frame extracted successfully for: {videoPath}");
            Console.WriteLine($"First frame saved at: {firstFrameVideoPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract the first frame for: {videoPath}");
            Console.WriteLine($"Error: {ex.Message}");
        }

        return firstFrameVideoPath;
    }
}
