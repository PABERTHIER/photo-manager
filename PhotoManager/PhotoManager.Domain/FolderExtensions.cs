namespace PhotoManager.Domain;

public static class FolderExtensions
{
    extension(Folder folder)
    {
        public bool IsSameDirectory(Folder folder2)
        {
            return folder.Path.Equals(folder2.Path);
        }
    }
}
