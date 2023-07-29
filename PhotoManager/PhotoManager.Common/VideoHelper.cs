using PhotoManager.Constants;
using System.Diagnostics;
using System.IO;

namespace PhotoManager.Common;

public static class VideoHelper
{
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

    public static string GetFirstFrame(string directoryName, string fileName)
    {
        string videoPath = Path.Combine(directoryName, fileName);
        string destinationPath = Path.Combine(PathConstants.PathLocation, "outputVideoThumbnails");

        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(destinationPath);

        // Set the FFmpeg executable path
        string ffmpegPath = PathConstants.FfmpegPath;

        // Set the output file name based on the input video file name
        string thumbnailName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
        string thumbnailPath = Path.Combine(destinationPath, thumbnailName);

        try
        {
            // Execute FFmpeg command to extract a thumbnail
            string arguments = $"-i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{thumbnailPath}\"";
            ExecuteFFmpegCommand(ffmpegPath, arguments);

            Console.WriteLine($"Thumbnail extracted successfully for: {videoPath}");
            Console.WriteLine($"Thumbnail saved at: {thumbnailPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract thumbnail for: {videoPath}");
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Thumbnail extraction completed.");

        return thumbnailPath;
    }

    private static void ExecuteFFmpegCommand(string ffmpegPath, string arguments)
    {
        using (Process process = new ())
        {
            process.StartInfo.FileName = ffmpegPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // Event handler for output and error data
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit with the specified timeout
            bool processExited = process.WaitForExit(1000);

            // If the process has not exited, it means it has timed out; terminate the process forcefully
            if (!processExited)
            {
                process.Kill();
                Console.WriteLine($"FFmpeg process terminated forcefully due to timeout.");
            }

            // Dispose the process
            process.Dispose();
        }
    }
}
