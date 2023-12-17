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

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);

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

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);

        Assert.AreNotEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_DirectoryAreNull_ThrowsNullReferenceException()
    {
        string? nullPath1 = null;
        string? nullPath2 = null;

        Folder folder1 = new() { Path = nullPath1! };
        Folder folder2 = new() { Path = nullPath2! };

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception1?.Message);

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception2?.Message);
        Assert.AreEqual(folder1.Path, folder2.Path);
    }

    [Test]
    public void IsSameDirectory_FirstFolderIsNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception1?.Message);

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception2?.Message);
    }

    [Test]
    public void IsSameDirectory_SecondFolderIsNull_ThrowsNullReferenceException()
    {
        Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder? folder2 = null;

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception1?.Message);

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception2?.Message);
    }

    [Test]
    public void IsSameDirectory_FoldersAreNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder? folder2 = null;

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception1?.Message);

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1!));

        Assert.AreEqual("Object reference not set to an instance of an object.", exception2?.Message);
    }
}
