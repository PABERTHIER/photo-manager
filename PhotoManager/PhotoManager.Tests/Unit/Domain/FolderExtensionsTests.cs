namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class FolderExtensionsTests
{
    [Test]
    public void IsSameDirectory_DirectoriesAreTheSame_ReturnsTrue()
    {
        Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.IsTrue(isSameDirectory);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.IsTrue(isSameDirectory);

        Assert.AreEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_DirectoriesAreNotTheSame_ReturnsFalse()
    {
        Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.IsFalse(isSameDirectory);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.IsFalse(isSameDirectory);

        Assert.AreNotEqual(folder1.Path, folder2.Path);
    }

    [Test]
    [TestCase("D:\\Workspace\\PhotoManager\\Toto", "")]
    [TestCase("", "D:\\Workspace\\PhotoManager\\Tutu")]

    public void IsSameDirectory_OneDirectoryIsEmpty_ReturnsFalse(string path1, string path2)
    {
        Folder folder1 = new() { Path = path1 };
        Folder folder2 = new() { Path = path2 };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.IsFalse(isSameDirectory);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.IsFalse(isSameDirectory);

        Assert.AreNotEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_DirectoryAreEmpty_ReturnsTrue()
    {
        Folder folder1 = new() { Path = string.Empty };
        Folder folder2 = new() { Path = string.Empty };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.IsTrue(isSameDirectory);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.IsTrue(isSameDirectory);

        Assert.AreEqual(folder1.Path, folder2.Path);
    }

    [TestCase(null, "D:\\Workspace\\PhotoManager\\Tutu")]
    public void IsSameDirectory_FirstDirectoryIsNull_ThrowsNullReferenceException(string path1, string path2)
    {
        Folder folder1 = new() { Path = path1 };
        Folder folder2 = new() { Path = path2 };

        Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        bool isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.IsFalse(isSameDirectory);

        Assert.AreNotEqual(folder1.Path, folder2.Path);
    }

    [TestCase("D:\\Workspace\\PhotoManager\\Toto", null)]
    public void IsSameDirectory_SecondDirectoryIsNull_ThrowsNullReferenceException(string path1, string path2)
    {
        Folder folder1 = new() { Path = path1 };
        Folder folder2 = new() { Path = path2 };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.IsFalse(isSameDirectory);

        Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.AreNotEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_DirectoryAreNull_ThrowsNullReferenceException()
    {
        string? nullPath1 = null;
        string? nullPath2 = null;

        Folder folder1 = new() { Path = nullPath1! };
        Folder folder2 = new() { Path = nullPath2! };

        Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.AreEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_FirstFolderIsNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };

        Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2));
        Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1!));
    }

    [Test]
    public void IsSameDirectory_SecondFolderIsNull_ThrowsNullReferenceException()
    {
        Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder? folder2 = null;

        Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2!));
        Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1));
    }

    [Test]
    public void IsSameDirectory_FoldersAreNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder? folder2 = null;

        Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2!));
        Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1!));
    }
}
