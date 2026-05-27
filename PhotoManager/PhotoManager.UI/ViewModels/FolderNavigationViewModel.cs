using PhotoManager.Domain;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class FolderNavigationViewModel(
    ApplicationViewModel applicationViewModel,
    Folder sourceFolder,
    List<string> recentTargetPaths)
    : BaseViewModel
{
    public ApplicationViewModel ApplicationViewModel { get; } = applicationViewModel;

    public Folder SourceFolder { get; } = sourceFolder;

    public Folder? SelectedFolder { get; private set; }

    public Folder? LastSelectedFolder => ApplicationViewModel.MoveAssetsLastSelectedFolder;

    public bool CanConfirm
    {
        get
        {
            if (SelectedFolder == null)
            {
                return false;
            }

            string sourceFolderPathFormatted =
                SourceFolder.Path.EndsWith('\\') ? SourceFolder.Path[..^1] : SourceFolder.Path;

            return sourceFolderPathFormatted != SelectedFolder.Path;
        }
    }

    public bool HasConfirmed { get; set; }

    public ObservableCollection<string> RecentTargetPaths { get; private set; } = [.. recentTargetPaths];

    public string? TargetPath
    {
        get;
        set
        {
            string? targetPath = !string.IsNullOrWhiteSpace(value) && value.EndsWith('\\') ? value[..^1] : value;

            if (field == targetPath)
            {
                return;
            }

            field = targetPath;
            SelectedFolder = CreateSelectedFolder(targetPath);
            NotifyPropertyChanged(nameof(TargetPath), nameof(SelectedFolder), nameof(CanConfirm));
        }
    }

    private Folder? CreateSelectedFolder(string? targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return null;
        }

        if (string.Equals(SourceFolder.Path, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            return SourceFolder;
        }

        if (LastSelectedFolder is { } lastSelectedFolder
            && string.Equals(lastSelectedFolder.Path, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            return lastSelectedFolder;
        }

        return new() { Id = Guid.NewGuid(), Path = targetPath };
    }
}
