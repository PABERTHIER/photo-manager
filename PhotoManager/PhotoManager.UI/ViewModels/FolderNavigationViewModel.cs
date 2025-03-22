using PhotoManager.Application;
using PhotoManager.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class FolderNavigationViewModel : ApplicationViewModel
{
    private string _targetPath;

    public FolderNavigationViewModel(IApplication application, Folder sourceFolder, Folder? lastSelectedFolder, List<string> recentTargetPaths) : base(application)
    {
        SourceFolder = sourceFolder;
        MoveAssetsLastSelectedFolder = lastSelectedFolder;
        RecentTargetPaths = [..recentTargetPaths];
    }

    public Folder SourceFolder { get; private set; }

    public Folder SelectedFolder
    {
        get
        {
            return !string.IsNullOrEmpty(TargetPath) ? new() { Id = Guid.NewGuid(), Path = TargetPath } : null;
        }
    }

    public bool CanConfirm
    {
        get
        {
            return SourceFolder != null
                && SelectedFolder != null
                && SourceFolder.Path != SelectedFolder.Path;
        }
    }

    public bool HasConfirmed { get; set; }

    public ObservableCollection<string> RecentTargetPaths { get; private set; }

    public string TargetPath
    {
        get => _targetPath;
        set
        {
            _targetPath = !string.IsNullOrEmpty(value) && value.EndsWith("\\") ? value[0..^1] : value;
            NotifyPropertyChanged(nameof(TargetPath), nameof(SelectedFolder), nameof(CanConfirm));
        }
    }
}
