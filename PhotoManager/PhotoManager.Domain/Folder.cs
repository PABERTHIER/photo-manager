﻿namespace PhotoManager.Domain;

public class Folder
{
    public string FolderId { get; set; }
    public string Path { get; set; }
    public string ThumbnailsFilename => FolderId + ".bin";

    public string Name
    {
        get
        {
            string[] pathParts = Path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string result = pathParts[pathParts.Length - 1];

            return result;
        }
    }

    public Folder? Parent
    {
        get
        {
            string? parentPath = GetParentPath();
            return parentPath != null ? new Folder { Path = parentPath } : null;
        }
    }

    private string? GetParentPath()
    {
        string[] thisPathDirectories = Path.Split(System.IO.Path.DirectorySeparatorChar);
        thisPathDirectories = thisPathDirectories.SkipLast(1).ToArray();
        return thisPathDirectories.Length > 0 ? System.IO.Path.Combine(thisPathDirectories) : null;
    }

    public bool IsParentOf(Folder otherFolder)
    {
        return !string.IsNullOrWhiteSpace(Path)
            && !string.IsNullOrWhiteSpace(otherFolder?.Parent?.Path)
            && string.Compare(Path, otherFolder?.Parent?.Path, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is Folder folder && folder.Path == Path;
    }

    public override int GetHashCode()
    {
        return Path != null ? Path.GetHashCode() : base.GetHashCode();
    }

    public override string ToString()
    {
        return Path;
    }
}