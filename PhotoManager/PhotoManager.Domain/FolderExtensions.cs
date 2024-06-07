namespace PhotoManager.Domain;

public static class FolderExtensions
{
    public static bool IsSameDirectory(this Folder folder1, Folder folder2)
    {
        return folder1.Path.Equals(folder2.Path);
    }
}
