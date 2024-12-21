namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class FolderConfigsTests
{
    private readonly Guid _folderId = Guid.NewGuid();

    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = FolderConfigs.ConfigureDataTable();

        Assert.IsNotNull(columns);
        Assert.AreEqual(2, columns.Length);

        Assert.AreEqual("Id", columns[0].ColumnName);
        Assert.AreEqual("Path", columns[1].ColumnName);
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoFolder()
    {
        string[] validValues =
        [
            _folderId.ToString(),
            "D:\\folder\\newFolder"
        ];

        Folder folder = FolderConfigs.ReadFunc(validValues);

        Assert.IsNotNull(folder);
        Assert.AreEqual(_folderId, folder.Id);
        Assert.AreEqual("D:\\folder\\newFolder", folder.Path);
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoFolder()
    {
        string[] tooManyValues =
        [
            _folderId.ToString(),
            "D:\\folder\\newFolder",
            "toto",
            "15"
        ];

        Folder folder = FolderConfigs.ReadFunc(tooManyValues);

        Assert.IsNotNull(folder);
        Assert.AreEqual(_folderId, folder.Id);
        Assert.AreEqual("D:\\folder\\newFolder", folder.Path);
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[2];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => FolderConfigs.ReadFunc(nullValues));

        Assert.AreEqual("Value cannot be null. (Parameter 'g')", exception?.Message);
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() => FolderConfigs.ReadFunc(emptyArray));

        Assert.AreEqual("Index was outside the bounds of the array.", exception?.Message);
    }

    [Test]
    public void WriteFunc_FolderWithValidValues_ConvertsFolderPropertiesToIndexInArray()
    {
        Folder folder = new()
        {
            Id = _folderId,
            Path = "D:\\folder\\newFolder"
        };

        object[] result = new object[2];
        for (int i = 0; i < 2; i++)
        {
            result[i] = FolderConfigs.WriteFunc(folder, i);
        }

        Assert.AreEqual(_folderId, result[0]);
        Assert.AreEqual("D:\\folder\\newFolder", result[1]);
    }

    [Test]
    public void WriteFunc_FolderWithPartialValues_ConvertsFolderPropertiesToIndexInArrayWithSomeDefaultValues()
    {
        Folder folder = new()
        {
            Path = "Path\\SubPath"
        };

        object[] result = new object[2];
        for (int i = 0; i < 2; i++)
        {
            result[i] = FolderConfigs.WriteFunc(folder, i);
        }

        Assert.AreEqual(Guid.Empty, result[0]);
        Assert.AreEqual("Path\\SubPath", result[1]);
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Folder folder = new()
        {
            Id = _folderId,
            Path = "D:\\folder\\newFolder"
        };

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => FolderConfigs.WriteFunc(folder, 15));

        Assert.AreEqual("Specified argument was out of the range of valid values. (Parameter 'i')", exception?.Message);
    }

    [Test]
    public void WriteFunc_FolderIsNull_ThrowsNullReferenceException()
    {
        Folder? folder = null;

        for (int i = 0; i < 2; i++)
        {
            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => FolderConfigs.WriteFunc(folder!, i));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        }
    }
}
