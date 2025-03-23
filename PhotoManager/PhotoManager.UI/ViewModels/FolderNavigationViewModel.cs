using PhotoManager.Application;
using PhotoManager.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class FolderNavigationViewModel : ApplicationViewModel
{
    private string? _targetPath;

    public FolderNavigationViewModel(
        IApplication application,
        Folder sourceFolder,
        Folder? lastSelectedFolder,
        List<string> recentTargetPaths) : base(application)
    {
        SourceFolder = sourceFolder;
        MoveAssetsLastSelectedFolder = lastSelectedFolder;
        RecentTargetPaths = [..recentTargetPaths];
    }

    public Folder SourceFolder { get; }

    public Folder? SelectedFolder => !string.IsNullOrWhiteSpace(TargetPath) ? new() { Id = Guid.NewGuid(), Path = TargetPath } : null;

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

    public ObservableCollection<string> RecentTargetPaths { get; private set; }

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
