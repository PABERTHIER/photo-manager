namespace PhotoManager.Tests.Unit.Domain;

[TestFixture]
public class FolderExtensionsTests
{
    [Test]
    public void IsSameDirectory_DirectoriesAreTheSame_ReturnsTrue()
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.That(isSameDirectory, Is.True);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.That(isSameDirectory, Is.True);

        Assert.That(folder2.Path, Is.EqualTo(folder1.Path));
    }

    [Test]
    public void IsSameDirectory_DirectoriesAreNotTheSame_ReturnsFalse()
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Tutu" };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.That(isSameDirectory, Is.False);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.That(isSameDirectory, Is.False);

        Assert.That(folder2.Path, Is.Not.EqualTo(folder1.Path));
    }

    [Test]
    [TestCase("D:\\Workspace\\PhotoManager\\Toto", "")]
    [TestCase("", "D:\\Workspace\\PhotoManager\\Tutu")]

    public void IsSameDirectory_OneDirectoryIsEmpty_ReturnsFalse(string path1, string path2)
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = path1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = path2 };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.That(isSameDirectory, Is.False);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.That(isSameDirectory, Is.False);

        Assert.That(folder2.Path, Is.Not.EqualTo(folder1.Path));
    }

    [Test]
    public void IsSameDirectory_DirectoryAreEmpty_ReturnsTrue()
    {
        Folder folder1 = new() { Id = Guid.Empty, Path = string.Empty };
        Folder folder2 = new() { Id = Guid.Empty, Path = string.Empty };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.That(isSameDirectory, Is.True);

        isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.That(isSameDirectory, Is.True);

        Assert.That(folder2.Path, Is.EqualTo(folder1.Path));
    }

    [TestCase(null!, "D:\\Workspace\\PhotoManager\\Tutu")]
    public void IsSameDirectory_FirstDirectoryIsNull_ThrowsNullReferenceException(string path1, string path2)
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = path1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = path2 };

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        bool isSameDirectory = folder2.IsSameDirectory(folder1);

        Assert.That(isSameDirectory, Is.False);

        Assert.That(folder2.Path, Is.Not.EqualTo(folder1.Path));
    }

    [TestCase("D:\\Workspace\\PhotoManager\\Toto", null!)]
    public void IsSameDirectory_SecondDirectoryIsNull_ThrowsNullReferenceException(string path1, string path2)
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = path1 };
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = path2 };

        bool isSameDirectory = folder1.IsSameDirectory(folder2);

        Assert.That(isSameDirectory, Is.False);

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        Assert.That(folder2.Path, Is.Not.EqualTo(folder1.Path));
    }

    [Test]
    public void IsSameDirectory_DirectoryAreNull_ThrowsNullReferenceException()
    {
        string? nullPath1 = null;
        string? nullPath2 = null;

        Folder folder1 = new() { Id = Guid.Empty, Path = nullPath1! };
        Folder folder2 = new() { Id = Guid.Empty, Path = nullPath2! };

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2));

        Assert.That(exception1?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1));

        Assert.That(exception2?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        Assert.That(folder2.Path, Is.EqualTo(folder1.Path));
    }

    [Test]
    public void IsSameDirectory_FirstFolderIsNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder folder2 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2));

        Assert.That(exception1?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2.IsSameDirectory(folder1!));

        Assert.That(exception2?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void IsSameDirectory_SecondFolderIsNull_ThrowsNullReferenceException()
    {
        Folder folder1 = new() { Id = Guid.NewGuid(), Path = "D:\\Workspace\\PhotoManager\\Toto" };
        Folder? folder2 = null;

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1.IsSameDirectory(folder2!));

        Assert.That(exception1?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1));

        Assert.That(exception2?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void IsSameDirectory_FoldersAreNull_ThrowsNullReferenceException()
    {
        Folder? folder1 = null;
        Folder? folder2 = null;

        NullReferenceException? exception1 = Assert.Throws<NullReferenceException>(() => folder1!.IsSameDirectory(folder2!));

        Assert.That(exception1?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        NullReferenceException? exception2 = Assert.Throws<NullReferenceException>(() => folder2!.IsSameDirectory(folder1!));

        Assert.That(exception2?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }
}
