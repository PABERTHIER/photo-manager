using PhotoManager.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class FolderNavigationViewModel(
    ApplicationViewModel applicationViewModel,
    Folder sourceFolder,
    List<string> recentTargetPaths)
    : BaseViewModel
{
    private string? _targetPath;

    public ApplicationViewModel ApplicationViewModel { get; } = applicationViewModel;

    public Folder SourceFolder { get; } = sourceFolder;

    // TODO: Not great having a new guid each time
    public Folder? SelectedFolder => !string.IsNullOrWhiteSpace(TargetPath) ? new() { Id = Guid.NewGuid(), Path = TargetPath } : null;

    public Folder? LastSelectedFolder => ApplicationViewModel.MoveAssetsLastSelectedFolder;

    public bool CanConfirm
    {
        get
        {
            if (SelectedFolder == null)
            {
                return false;
            }

            string sourceFolderPathFormatted = SourceFolder.Path.EndsWith('\\') ? SourceFolder.Path[..^1] : SourceFolder.Path;

            return sourceFolderPathFormatted != SelectedFolder.Path;
        }
    }

    public bool HasConfirmed { get; set; }

    public ObservableCollection<string> RecentTargetPaths { get; private set; } = [.. recentTargetPaths];

    public string? TargetPath
    {
        get => _targetPath;
        set
        {
            _targetPath = !string.IsNullOrWhiteSpace(value) && value.EndsWith('\\') ? value[..^1] : value;
            NotifyPropertyChanged(nameof(TargetPath), nameof(SelectedFolder), nameof(CanConfirm));
        }
    }
}
