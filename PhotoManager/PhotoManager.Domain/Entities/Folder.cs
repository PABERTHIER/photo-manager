namespace PhotoManager.Domain.Entities;

public class Folder
{
    private static readonly char[] PathSeparators = ['\\', '/'];

    public required Guid Id { get; init; }
    public required string Path { get; init; }

    public string Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                return string.Empty;
            }

            ReadOnlySpan<char> pathAsSpan = Path.AsSpan().TrimEnd(PathSeparators);
            int lastSeparator = pathAsSpan.LastIndexOfAny(PathSeparators);

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

        ReadOnlySpan<char> pathAsSpan = path.AsSpan().TrimEnd(PathSeparators);
        int lastSeparator = pathAsSpan.LastIndexOfAny(PathSeparators);

        return lastSeparator >= 0 ? pathAsSpan[..lastSeparator].ToString() : null;
    }
}
