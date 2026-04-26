using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class FolderTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Name_PathIsNullOrWhiteSpace_ReturnsEmpty(string? path)
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = path! };

        string name = folder.Name;

        Assert.That(name, Is.EqualTo(string.Empty));
    }

    [Test]
    [TestCase("FolderName", "FolderName")]
    [TestCase("C:\\TestFolder\\SubFolder", "SubFolder")]
    [TestCase("C:\\TestFolder\\SubFolder\\", "SubFolder")]
    public void Name_PathIsValid_ReturnsExpected(string path, string expected)
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = path };

        string name = folder.Name;

        Assert.That(name, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("TestFolder", "TestFolder", false)] // WhenParentAndChildPathsAreTheSame
    [TestCase("TestFolder", "TestFolder\\TestSubFolder1", true)] // ChildIsImmediateChild
    [TestCase("TestFolder", "TestFolder\\TestSubFolder1\\TestSubFolder2", false)] // WhenChildIsNotImmediateChild
    [TestCase("TestFolder\\TestSubFolder1", "TestFolder", false)]
    [TestCase("TestFolder\\TestSubFolder1\\TestSubFolder2", "TestFolder", false)]
    [TestCase("TestFolder\\TestSubFolder1", "TestFolder\\TestSubFolder2", false)]
    public void IsParentOf_PathIsValid_ReturnsExpected(string parentFolderSubPath, string childFolderSubPath,
        bool expected)
    {
        Folder parentFolder = new()
        {
            Id = Guid.NewGuid(),
            Path = Path.Combine(_dataDirectory!, parentFolderSubPath)
        };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = Path.Combine(_dataDirectory!, childFolderSubPath) };

        bool isFolderParentOfChild = parentFolder.IsParentOf(childFolder);

        Assert.That(isFolderParentOfChild, Is.EqualTo(expected));
    }

    [Test]
    public void IsParentOf_PathMatchesCaseInsensitively_ReturnsTrue()
    {
        Folder parentFolder = new() { Id = Guid.NewGuid(), Path = "C:\\TestFolder" };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = "C:\\TESTFOLDER\\SubFolder" };

        bool isFolderParentOfChild = parentFolder.IsParentOf(childFolder);

        Assert.That(isFolderParentOfChild, Is.True);
    }

    [Test]
    [TestCase("", "TestFolder", false)]
    [TestCase("TestFolder", "", false)]
    [TestCase(" ", "TestFolder", false)]
    [TestCase("TestFolder", " ", false)]
    public void IsParentOf_PathIsInvalid_ReturnsExpected(string parentFolderSubPath, string childFolderSubPath,
        bool expected)
    {
        Folder parentFolder = new() { Id = Guid.NewGuid(), Path = parentFolderSubPath };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = childFolderSubPath };

        bool isFolderParentOfChild = parentFolder.IsParentOf(childFolder);

        Assert.That(isFolderParentOfChild, Is.EqualTo(expected));
    }

    [Test]
    public void IsParentOf_PathIsNull_ReturnsFalse()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = null! };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = Path.Combine(_dataDirectory!, "TestFolder") };

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.That(isFolderParentOfChild, Is.False);
    }

    [Test]
    public void IsParentOf_ChildPathIsNull_ReturnsFalse()
    {
        Folder folder = new() { Id = Guid.NewGuid(), Path = _dataDirectory! };
        Folder childFolder = new() { Id = Guid.NewGuid(), Path = null! };

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.That(isFolderParentOfChild, Is.False);
    }
}
