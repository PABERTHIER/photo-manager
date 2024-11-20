namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class FolderTests
{
    private string? _dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
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
        Folder parentFolder1 = new() { Path = Path.Combine(_dataDirectory!, parentFolderSubPath1) };
        Folder childFolder2 = new() { Path = Path.Combine(_dataDirectory!, childFolderSubPath2) };

        bool isFolderParentOfChild = parentFolder1.IsParentOf(childFolder2);

        Assert.AreEqual(expected, isFolderParentOfChild);
    }

    [Test]
    [TestCase("", "TestFolder", false)]
    [TestCase("TestFolder", "", false)]
    [TestCase(" ", "TestFolder", false)]
    [TestCase("TestFolder", " ", false)]
    public void IsParentOf_PathIsInvalid_ReturnsExpected(string parentFolderSubPath1, string childFolderSubPath2, bool expected)
    {
        Folder parentFolder1 = new() { Path = parentFolderSubPath1 };
        Folder childFolder2 = new() { Path = childFolderSubPath2 };

        bool isFolderParentOfChild = parentFolder1.IsParentOf(childFolder2);

        Assert.AreEqual(expected, isFolderParentOfChild);
    }

    [Test]
    public void IsParentOf_PathIsNull_ReturnsFalse()
    {
        Folder folder = new() { Path = null! };
        Folder childFolder = new() { Path = Path.Combine(_dataDirectory!, "TestFolder") };

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.IsFalse(isFolderParentOfChild);
    }

    [Test]
    public void IsParentOf_ChildPathIsNull_ReturnsFalse()
    {
        Folder folder = new() { Path = _dataDirectory! };
        Folder childFolder = new() { Path = null! };

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.IsFalse(isFolderParentOfChild);
    }
}
