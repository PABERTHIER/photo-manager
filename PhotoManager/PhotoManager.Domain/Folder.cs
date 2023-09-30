namespace PhotoManager.Domain;

public class Folder
{
    // TODO: FolderId not null -> when a new -> new guid
    public Guid FolderId { get; set; }
    public string Path { get; set; }
    public string ThumbnailsFilename => FolderId + ".bin"; // TODO: BlobFileName instead -> rename all methods like this

    public string Name
    {
        get
        {
            string[] pathParts = Path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string result = pathParts[pathParts.Length - 1];

            return result;
        }
    }

    // Only used here and BatchHelper which is not really used -> private ?
    public Folder? Parent
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

    // Usefull ?
    public override bool Equals(object? obj)
    {
        return obj is Folder folder && folder.Path == Path;
    }

    // Only for UT
    public override int GetHashCode()
    {
        return Path != null ? Path.GetHashCode() : base.GetHashCode();
    }

    // Only for UT
    public override string ToString()
    {
        return Path;
    }

    private string? GetParentPath()
    {
        string[]? directoriesPath = Path?.Split(System.IO.Path.DirectorySeparatorChar);
        directoriesPath = directoriesPath?.SkipLast(1).ToArray();
        return directoriesPath?.Length > 0 ? System.IO.Path.Combine(directoriesPath) : null;
    }
}
