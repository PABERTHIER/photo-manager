namespace PhotoManager.Domain.Models;

public class SyncAssetsDirectoriesDefinition
{
    private static readonly char[] DirectorySeparators = ['\\', '/'];

    public required string SourceDirectory { get; set; }
    public required string DestinationDirectory { get; set; }
    public bool IncludeSubFolders { get; init; }
    public bool DeleteAssetsNotInSource { get; init; }

    internal bool IsValid()
    {
        return (IsValidLocalPath(SourceDirectory) || IsValidRemotePath(SourceDirectory))
               && (IsValidLocalPath(DestinationDirectory) || IsValidRemotePath(DestinationDirectory));
    }

    internal void Normalize()
    {
        SourceDirectory = NormalizeDirectory(SourceDirectory);
        DestinationDirectory = NormalizeDirectory(DestinationDirectory);
    }

    private static string NormalizeDirectory(string directory)
    {
        return GetPathKind(directory) switch
        {
            PathKind.WindowsLocal => NormalizeWindowsLocalPath(directory),
            PathKind.WindowsRemote => NormalizeWindowsRemotePath(directory),
            PathKind.UnixAbsolute => NormalizeUnixPath(directory),
            _ => directory
        };
    }

    private static string NormalizeWindowsLocalPath(string directory)
    {
        string drive = directory[..2];
        string[] parts = directory[2..].Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);

        return parts.Length == 0 ? $"{drive}\\" : $"{drive}\\{string.Join('\\', parts)}";
    }

    private static string NormalizeWindowsRemotePath(string directory)
    {
        string[] parts = directory.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);

        return $"\\\\{string.Join('\\', parts)}";
    }

    private static string NormalizeUnixPath(string directory)
    {
        string[] parts = directory.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);

        return $"/{string.Join('/', parts)}";
    }

    private static bool IsValidLocalPath(string directory)
    {
        return GetPathKind(directory) is PathKind.WindowsLocal or PathKind.UnixAbsolute;
    }

    private static bool IsValidRemotePath(string directory)
    {
        return GetPathKind(directory) is PathKind.WindowsRemote;
    }

    private static PathKind GetPathKind(string directory)
    {
        if (directory == null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            return PathKind.Invalid;
        }

        if (directory.Length >= 3
            && char.IsAsciiLetter(directory[0])
            && directory[1] == ':'
            && IsDirectorySeparator(directory[2]))
        {
            return PathKind.WindowsLocal;
        }

        if ((directory.StartsWith(@"\\", StringComparison.Ordinal)
             || (OperatingSystem.IsWindows() && directory.StartsWith("//", StringComparison.Ordinal)))
            && directory.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries).Length >= 2)
        {
            return PathKind.WindowsRemote;
        }

        return directory[0] == '/' ? PathKind.UnixAbsolute : PathKind.Invalid;
    }

    private static bool IsDirectorySeparator(char character)
    {
        return character is '\\' or '/';
    }

    private enum PathKind
    {
        Invalid,
        WindowsLocal,
        WindowsRemote,
        UnixAbsolute
    }
}
