using FFMpegCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.Common;

public static class VideoHelper
{
    private const string PhotoManagerFfmpegBinaryFolderEnvironmentVariable = "PHOTOMANAGER_FFMPEG_BINARY_FOLDER";
    private const string FfmpegBinaryFolderEnvironmentVariable = "FFMPEG_BINARY_FOLDER";
    private const string FfmpegFolderName = "Ffmpeg";
    private const string FfmpegBinFolderName = "Bin";

    private static readonly string[] AppLocalCandidateFolders =
    [
        string.Empty,
        FfmpegFolderName,
        Path.Combine(FfmpegFolderName, FfmpegBinFolderName),
        Path.Combine("runtimes", "win-x64", "native"),
        Path.Combine("runtimes", "linux-x64", "native"),
        Path.Combine("runtimes", "linux-arm64", "native"),
        Path.Combine("runtimes", "osx-x64", "native"),
        Path.Combine("runtimes", "osx-arm64", "native")
    ];

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
        ReadOnlySpan<char> extension = Path.GetExtension(fileName.AsSpan());

        if (extension.IsEmpty)
        {
            return false;
        }

        return extension.Equals(".3g2", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".3gp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".asf", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".av1", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".avi", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".flv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".m4v", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mkv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mov", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".mpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".ogv", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webm", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".wmv", StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetFirstFramePath(string directoryName, string fileName, string destinationPath,
        ILogger logger)
    {
        string videoPath = Path.Combine(directoryName, fileName);

        // Create the output directory if it doesn't exist
        Directory.CreateDirectory(destinationPath);

        // Set the output file name based on the input video file name
        string firstFrameVideoName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";

        try
        {
            string firstFrameVideoPath = Path.Combine(destinationPath, firstFrameVideoName);

            string? ffmpegBinPath = GetFfmpegBinaryFolder();
            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegBinPath!);

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

            logger.LogInformation("First frame extracted successfully for: {videoPath}", videoPath);
            logger.LogInformation("First frame saved at: {firstFrameVideoPath}", firstFrameVideoPath);

            return firstFrameVideoPath;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to extract the first frame for: {videoPath}, Message: {ex.Message}",
                videoPath,
                ex.Message);

            return null;
        }
    }

    public static string? GetFfmpegBinaryFolder()
    {
        string? configuredBinaryFolder =
            Environment.GetEnvironmentVariable(PhotoManagerFfmpegBinaryFolderEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(configuredBinaryFolder))
        {
            configuredBinaryFolder = Environment.GetEnvironmentVariable(FfmpegBinaryFolderEnvironmentVariable);
        }

        return GetFfmpegBinaryFolder(AppContext.BaseDirectory, configuredBinaryFolder);
    }

    public static string? GetFfmpegBinaryFolder(string baseDirectory, string? configuredBinaryFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        if (ContainsFfmpegExecutable(configuredBinaryFolder))
        {
            return configuredBinaryFolder;
        }

        foreach (string appLocalCandidateFolder in AppLocalCandidateFolders)
        {
            string candidateFolder = Path.GetFullPath(Path.Combine(baseDirectory, appLocalCandidateFolder));

            if (ContainsFfmpegExecutable(candidateFolder))
            {
                return candidateFolder;
            }
        }

        return GetLegacyProjectBinaryFolder(baseDirectory);
    }

    private static string? GetLegacyProjectBinaryFolder(string baseDirectory)
    {
        DirectoryInfo? directoryInfo = new(baseDirectory);

        while (directoryInfo is not null)
        {
            string candidateFolder = Path.Combine(directoryInfo.FullName, "PhotoManager.Common", FfmpegFolderName,
                FfmpegBinFolderName);

            if (ContainsFfmpegExecutable(candidateFolder))
            {
                return candidateFolder;
            }

            directoryInfo = directoryInfo.Parent;
        }

        return null;
    }

    [ExcludeFromCodeCoverage(Justification = "Platform-dependent")]
    private static bool ContainsFfmpegExecutable(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return false;
        }

        string executableName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

        return File.Exists(Path.Combine(directory, executableName));
    }
}
