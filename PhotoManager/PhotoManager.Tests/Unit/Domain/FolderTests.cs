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
    [TestCase("TestFolder", "TestFolder", false)] // WhenParentAndChildPathsAreTheSame
    [TestCase("TestFolder", "TestFolder\\TestSubFolder1", true)] // ChildIsImmediateChild
    [TestCase("TestFolder", "TestFolder\\TestSubFolder1\\TestSubFolder2", false)] // WhenChildIsNotImmediateChild
    [TestCase("TestFolder\\TestSubFolder1", "TestFolder", false)]
    [TestCase("TestFolder\\TestSubFolder1\\TestSubFolder2", "TestFolder", false)]
    [TestCase("TestFolder\\TestSubFolder1", "TestFolder\\TestSubFolder2", false)]
    public void IsParentOf_PathIsValid_ReturnsExpected(string parentFolderSubPath1, string childFolderSubPath2, bool expected)
    {
        Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = Path.Combine(_dataDirectory!, parentFolderSubPath1) };
        Folder childFolder2 = new() { Id = Guid.NewGuid(), Path = Path.Combine(_dataDirectory!, childFolderSubPath2) };

        bool isFolderParentOfChild = parentFolder1.IsParentOf(childFolder2);

        Assert.That(isFolderParentOfChild, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", "TestFolder", false)]
    [TestCase("TestFolder", "", false)]
    [TestCase(" ", "TestFolder", false)]
    [TestCase("TestFolder", " ", false)]
    public void IsParentOf_PathIsInvalid_ReturnsExpected(string parentFolderSubPath1, string childFolderSubPath2, bool expected)
    {
        Folder parentFolder1 = new() { Id = Guid.NewGuid(), Path = parentFolderSubPath1 };
        Folder childFolder2 = new() { Id = Guid.NewGuid(), Path = childFolderSubPath2 };

        bool isFolderParentOfChild = parentFolder1.IsParentOf(childFolder2);

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
