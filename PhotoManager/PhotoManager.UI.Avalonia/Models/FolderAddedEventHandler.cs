using PhotoManager.Domain;

namespace PhotoManager.UI.Models;

public delegate void FolderAddedEventHandler(object sender, FolderAddedEventArgs e);

public class FolderAddedEventArgs
{
    public required Folder Folder { get; init; }
}
