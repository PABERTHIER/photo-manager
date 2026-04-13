namespace PhotoManager.Domain;

public class Folder
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public string BlobFileName => field ??= $"{Id}.bin";

    public string Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                return string.Empty;
            }

            ReadOnlySpan<char> pathAsSpan = Path.AsSpan().TrimEnd('\\');
            int lastSeparator = pathAsSpan.LastIndexOf('\\');

            return lastSeparator >= 0 ? pathAsSpan[(lastSeparator + 1)..].ToString() : pathAsSpan.ToString();
        }
    }

    public bool IsParentOf(Folder folder)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            return false;
        }

        string? parentPath = GetParentPath(folder.Path);

        return !string.IsNullOrWhiteSpace(parentPath)
               && string.Compare(Path, parentPath, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private static string? GetParentPath(string? path)
    {
        if (path == null)
        {
            return null;
        }

        ReadOnlySpan<char> pathAsSpan = path.AsSpan().TrimEnd(System.IO.Path.DirectorySeparatorChar);
        int lastSeparator = pathAsSpan.LastIndexOf(System.IO.Path.DirectorySeparatorChar);

        return lastSeparator >= 0 ? pathAsSpan[..lastSeparator].ToString() : null;
    }
}
