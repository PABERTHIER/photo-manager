namespace PhotoManager.Domain;

public class Folder
{
    public Guid Id { get; init; }
    public required string Path { get; init; }
    public string ThumbnailsFilename => Id + ".bin"; // TODO: BlobFileName instead -> rename all methods like this

    public string Name
    {
        get
        {
            string[] pathParts = !string.IsNullOrWhiteSpace(Path) ? Path.Split(['\\'], StringSplitOptions.RemoveEmptyEntries) : [];
            string result = pathParts.Length > 0 ? pathParts[^1] : string.Empty;

            return result;
        }
    }

    private Folder? Parent
    {
        get
        {
            string? parentPath = GetParentPath(Path);
            return parentPath != null ? new Folder { Path = parentPath } : null;
        }
    }

    public bool IsParentOf(Folder folder)
    {
        return !string.IsNullOrWhiteSpace(Path)
            && !string.IsNullOrWhiteSpace(folder.Parent?.Path)
            && string.Compare(Path, folder.Parent?.Path, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private static string? GetParentPath(string? path)
    {
        string[]? directoriesPath = path?.Split(System.IO.Path.DirectorySeparatorChar);
        directoriesPath = directoriesPath?.SkipLast(1).ToArray();
        return directoriesPath?.Length > 0 ? System.IO.Path.Combine(directoriesPath) : null;
    }
}
