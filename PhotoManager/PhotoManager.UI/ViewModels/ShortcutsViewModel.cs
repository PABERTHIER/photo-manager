using PhotoManager.UI.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PhotoManager.UI.ViewModels;

public class ShortcutsViewModel : BaseViewModel
{
    public ObservableCollection<ShortcutItem> Shortcuts { get; }

    [ExcludeFromCodeCoverage(Justification = "Platform-dependent")]
    public ShortcutsViewModel()
    {
        // macOS users expect the Command key; every other platform uses Control.
        // The MainWindow key handler accepts Cmd (Meta) and Ctrl on macOS, so both wordings stay accurate.
        string commandModifier = OperatingSystem.IsMacOS() ? "Cmd" : "Ctrl";

        Shortcuts =
        [
            new("Navigation", "F1", "Switch view", "Switches between the thumbnails view and the picture viewer."),
            new("Navigation", "Left / Page Up", "Previous picture", "Moves to the previous picture in viewer mode."),
            new("Navigation", "Right / Page Down", "Next picture", "Moves to the next picture in viewer mode."),
            new("File", $"{commandModifier}+C", "Copy selected assets",
                "Select one or more pictures first, then choose the destination folder."),
            new("File", $"{commandModifier}+M", "Move selected assets",
                "Select one or more pictures first, then choose the destination folder."),
            new("File", "Delete", "Delete selected assets", "Deletes the selected pictures from the catalog and disk.")
        ];
    }
}
