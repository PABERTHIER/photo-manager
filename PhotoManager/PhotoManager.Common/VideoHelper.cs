using FFMpegCore;
using log4net;
using System.Reflection;

namespace PhotoManager.Common;

public static class VideoHelper
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

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
            { } s when s.EndsWith(".3g2") => true,
            { } s when s.EndsWith(".3gp") => true,
            { } s when s.EndsWith(".asf") => true,
            { } s when s.EndsWith(".av1") => true,
            { } s when s.EndsWith(".avi") => true,
            { } s when s.EndsWith(".flv") => true,
            { } s when s.EndsWith(".m4v") => true,
            { } s when s.EndsWith(".mkv") => true,
            { } s when s.EndsWith(".mov") => true,
            { } s when s.EndsWith(".mp4") => true,
            { } s when s.EndsWith(".mpeg") => true,
            { } s when s.EndsWith(".mpg") => true,
            { } s when s.EndsWith(".ogv") => true,
            { } s when s.EndsWith(".webm") => true,
            { } s when s.EndsWith(".wmv") => true,
            _ => false
        };
    }

    public static string? GetFirstFramePath(string directoryName, string fileName, string destinationPath)
    {
        string videoPath = Path.Combine(directoryName, fileName);

        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(destinationPath);

        // Set the output file name based on the input video file name
        string firstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";

        try
        {
            string firstFrameVideoPath = Path.Combine(destinationPath, firstFrameVideoName);

            // Set the path to the extracted ffmpeg.exe, ffplay.exe, and ffprobe.exe files
            string ffmpegBinPath = GetCommonProjectPath();

            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegBinPath);

            // Use FFMpegCore to extract the first frame
            FFMpegArguments
                .FromFileInput(videoPath)
                .OutputToFile(firstFrameVideoPath, false, options => options
                    .Seek(TimeSpan.FromSeconds(1))
                    .WithFrameOutputCount(1))
                .ProcessSynchronously();

            if (!File.Exists(firstFrameVideoPath))
            {
                throw new FileFormatException(
                    "FFmpeg failed to generate the first frame file due to its format or content.");
            }

            Log.Info($"First frame extracted successfully for: {videoPath}");
            Log.Info($"First frame saved at: {firstFrameVideoPath}");

            return firstFrameVideoPath;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to extract the first frame for: {videoPath}, Message: {ex.Message}");

            return null;
        }
    }

    /// <summary>
    /// Gets the path where are stored the Ffmpeg binaries, resolving correctly for both test and runtime contexts.
    /// </summary>
    /// <returns>The path to the Ffmpeg binaries.</returns>
    private static string GetCommonProjectPath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Traverse up the directory structure until you find the "PhotoManager.Common" folder
        string commonProjectPath = FindProjectDirectory(baseDirectory, "PhotoManager.Common");

        return Path.Combine(commonProjectPath, "Ffmpeg", "Bin");
    }

    private static string FindProjectDirectory(string startPath, string projectFolderName)
    {
        DirectoryInfo directoryInfo = new(startPath);

        // Traverse up the directory structure and return as soon as the project folder is found
        while (directoryInfo.GetDirectories(projectFolderName).Length == 0)
        {
            directoryInfo = directoryInfo.Parent!;
        }

        // Since the project structure is fixed, we can assume this point will always find the directory
        return Path.Combine(directoryInfo.FullName, projectFolderName);
    }
}
