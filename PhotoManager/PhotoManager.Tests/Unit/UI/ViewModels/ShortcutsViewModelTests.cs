using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class ShortcutsViewModelTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    [Test]
    public void Constructor_DefaultShortcuts_ReturnsUserFriendlyShortcutList()
    {
        string expectedModifier = OperatingSystem.IsMacOS() ? "Cmd" : "Ctrl";

        ShortcutsViewModel shortcutsViewModel = new();

        Assert.That(shortcutsViewModel.Shortcuts, Has.Count.EqualTo(6));

        Assert.That(shortcutsViewModel.Shortcuts[0].Section, Is.EqualTo("Navigation"));
        Assert.That(shortcutsViewModel.Shortcuts[0].Shortcut, Is.EqualTo("F1"));
        Assert.That(shortcutsViewModel.Shortcuts[0].Action, Is.EqualTo("Switch view"));
        Assert.That(shortcutsViewModel.Shortcuts[0].Details,
            Is.EqualTo("Switches between the thumbnails view and the picture viewer."));

        Assert.That(shortcutsViewModel.Shortcuts[1].Section, Is.EqualTo("Navigation"));
        Assert.That(shortcutsViewModel.Shortcuts[1].Shortcut, Is.EqualTo("Left / Page Up"));
        Assert.That(shortcutsViewModel.Shortcuts[1].Action, Is.EqualTo("Previous picture"));
        Assert.That(shortcutsViewModel.Shortcuts[1].Details,
            Is.EqualTo("Moves to the previous picture in viewer mode."));

        Assert.That(shortcutsViewModel.Shortcuts[2].Section, Is.EqualTo("Navigation"));
        Assert.That(shortcutsViewModel.Shortcuts[2].Shortcut, Is.EqualTo("Right / Page Down"));
        Assert.That(shortcutsViewModel.Shortcuts[2].Action, Is.EqualTo("Next picture"));
        Assert.That(shortcutsViewModel.Shortcuts[2].Details, Is.EqualTo("Moves to the next picture in viewer mode."));

        Assert.That(shortcutsViewModel.Shortcuts[3].Section, Is.EqualTo("File"));
        Assert.That(shortcutsViewModel.Shortcuts[3].Shortcut, Is.EqualTo($"{expectedModifier}+C"));
        Assert.That(shortcutsViewModel.Shortcuts[3].Action, Is.EqualTo("Copy selected assets"));
        Assert.That(shortcutsViewModel.Shortcuts[3].Details,
            Is.EqualTo("Select one or more pictures first, then choose the destination folder."));

        Assert.That(shortcutsViewModel.Shortcuts[4].Section, Is.EqualTo("File"));
        Assert.That(shortcutsViewModel.Shortcuts[4].Shortcut, Is.EqualTo($"{expectedModifier}+M"));
        Assert.That(shortcutsViewModel.Shortcuts[4].Action, Is.EqualTo("Move selected assets"));
        Assert.That(shortcutsViewModel.Shortcuts[4].Details,
            Is.EqualTo("Select one or more pictures first, then choose the destination folder."));

        Assert.That(shortcutsViewModel.Shortcuts[5].Section, Is.EqualTo("File"));
        Assert.That(shortcutsViewModel.Shortcuts[5].Shortcut, Is.EqualTo("Delete"));
        Assert.That(shortcutsViewModel.Shortcuts[5].Action, Is.EqualTo("Delete selected assets"));
        Assert.That(shortcutsViewModel.Shortcuts[5].Details,
            Is.EqualTo("Deletes the selected pictures from the catalog and disk."));
    }
}
