namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class FolderTests
{
    private string? dataDirectory;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
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
        Folder parentFolder1 = new() { Path = Path.Combine(dataDirectory!, parentFolderSubPath1) };
        Folder childFolder2 = new() { Path = Path.Combine(dataDirectory!, childFolderSubPath2) };

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
        Folder folder = new();
        Folder childFolder = new() { Path = Path.Combine(dataDirectory!, "TestFolder") };

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.IsFalse(isFolderParentOfChild);
    }

    [Test]
    public void IsParentOf_ParentPathIsNull_ReturnsFalse()
    {
        Folder folder = new() { Path = dataDirectory! };
        Folder childFolder = new();

        bool isFolderParentOfChild = folder.IsParentOf(childFolder);

        Assert.IsFalse(isFolderParentOfChild);
    }
}

//    [Fact]
//    public void SameFileAndFolderEqualTest()
//    {
//        Folder folder1 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        Folder folder2 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        folder1.Should().Be(folder2);
//        folder1.GetHashCode().Should().Be(folder2.GetHashCode());
//    }

//    [Fact]
//    public void DifferentPathNonEqualTest()
//    {
//        Folder folder1 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        Folder folder2 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures\MyAlbum"
//        };

//        folder1.Should().NotBe(folder2);
//        folder1.GetHashCode().Should().NotBe(folder2.GetHashCode());
//    }

//    [Fact]
//    public void OnlyOnePathNonEqualTest()
//    {
//        Folder folder1 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        Folder folder2 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = null
//        };

//        folder1.Should().NotBe(folder2);
//        folder1.GetHashCode().Should().NotBe(folder2.GetHashCode());
//    }

//    [Fact]
//    public void OnlyOneObjectNonEqualTest()
//    {
//        Folder folder = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        folder.Should().NotBeNull();
//    }

//    [Fact]
//    public void ToStringWithPathTest()
//    {
//        Folder folder = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = @"C:\Users\TestUser\Pictures"
//        };

//        folder.ToString().Should().Be(@"C:\Users\TestUser\Pictures");
//    }

//    [Fact]
//    public void ToStringWithoutPathTest()
//    {
//        Folder folder1 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = null
//        };

//        folder1.ToString().Should().BeNull();

//        Folder folder2 = new()
//        {
//            FolderId = "599e3dec-1da6-4e1d-b18d-e2e6cb417292",
//            Path = ""
//        };

//        folder2.ToString().Should().BeEmpty();
//    }
//}
