using System.Text.RegularExpressions;

namespace PhotoManager.Domain;

public partial class SyncAssetsDirectoriesDefinition
{
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
        bool isRemote = IsValidRemotePath(directory);
        string[] parts = directory.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        string normalizedDirectory = Path.Combine(parts);
        normalizedDirectory = isRemote ? "\\" + normalizedDirectory : normalizedDirectory;

        return normalizedDirectory;
    }

    private static bool IsValidLocalPath(string directory)
    {
        Regex regex = LocalPathRegex();
        return regex.IsMatch(directory);
    }

    private static bool IsValidRemotePath(string directory)
    {
        Regex regex = RemotePathRegex();
        return regex.IsMatch(directory);
    }

    [GeneratedRegex("^([A-Za-z])(:)(\\[A-Za-z0-9]*)*")]
    private static partial Regex LocalPathRegex();

    [GeneratedRegex(@"^(\\)(\[A-Za-z0-9]*)*")]
    private static partial Regex RemotePathRegex();
}
