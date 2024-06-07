namespace PhotoManager.Domain;

public class Folder
{
    // TODO: FolderId not null -> when a new -> new guid
    public Guid FolderId { get; set; } // TODO: Rename to Id
    public required string Path { get; set; }
    public string ThumbnailsFilename => FolderId + ".bin"; // TODO: BlobFileName instead -> rename all methods like this

    public string Name
    {
        get
        {
            string[] pathParts = Path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string result = pathParts[^1];

            return result;
        }
    }

    private Folder? Parent
    {
        get
        {
            string? parentPath = GetParentPath();
            return parentPath != null ? new Folder { Path = parentPath } : null;
        }
    }

    public bool IsParentOf(Folder folder)
    {
        return !string.IsNullOrWhiteSpace(Path)
            && !string.IsNullOrWhiteSpace(folder?.Parent?.Path)
            && string.Compare(Path, folder?.Parent?.Path, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private string? GetParentPath()
    {
        string[]? directoriesPath = Path?.Split(System.IO.Path.DirectorySeparatorChar);
        directoriesPath = directoriesPath?.SkipLast(1).ToArray();
        return directoriesPath?.Length > 0 ? System.IO.Path.Combine(directoriesPath) : null;
    }
}
