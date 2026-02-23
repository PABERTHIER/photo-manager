namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class FolderConfigsTests
{
    private readonly Guid _folderId = Guid.NewGuid();

    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = FolderConfigs.ConfigureDataTable();

        Assert.That(columns, Is.Not.Null);
        Assert.That(columns, Has.Length.EqualTo(2));

        Assert.That(columns[0].ColumnName, Is.EqualTo("Id"));
        Assert.That(columns[1].ColumnName, Is.EqualTo("Path"));
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

        Assert.That(folder, Is.Not.Null);
        Assert.That(folder.Id, Is.EqualTo(_folderId));
        Assert.That(folder.Path, Is.EqualTo("D:\\folder\\newFolder"));
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

        Assert.That(folder, Is.Not.Null);
        Assert.That(folder.Id, Is.EqualTo(_folderId));
        Assert.That(folder.Path, Is.EqualTo("D:\\folder\\newFolder"));
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[2];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => FolderConfigs.ReadFunc(nullValues));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'g')"));
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception =
            Assert.Throws<IndexOutOfRangeException>(() => FolderConfigs.ReadFunc(emptyArray));

        Assert.That(exception?.Message, Is.EqualTo("Index was outside the bounds of the array."));
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

        Assert.That(result[0], Is.EqualTo(_folderId));
        Assert.That(result[1], Is.EqualTo("D:\\folder\\newFolder"));
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        Folder folder = new()
        {
            Id = _folderId,
            Path = "D:\\folder\\newFolder"
        };

        ArgumentOutOfRangeException? exception =
            Assert.Throws<ArgumentOutOfRangeException>(() => FolderConfigs.WriteFunc(folder, 15));

        Assert.That(exception?.Message,
            Is.EqualTo("Specified argument was out of the range of valid values. (Parameter 'i')"));
    }

    [Test]
    public void WriteFunc_FolderIsNull_ThrowsNullReferenceException()
    {
        Folder? folder = null;

        for (int i = 0; i < 2; i++)
        {
            NullReferenceException? exception =
                Assert.Throws<NullReferenceException>(() => FolderConfigs.WriteFunc(folder!, i));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }
    }
}
