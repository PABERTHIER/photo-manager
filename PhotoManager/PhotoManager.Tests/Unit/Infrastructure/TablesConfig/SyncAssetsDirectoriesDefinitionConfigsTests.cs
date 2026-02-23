namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class SyncAssetsDirectoriesDefinitionConfigsTests
{
    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable();

        Assert.That(columns, Is.Not.Null);
        Assert.That(columns, Has.Length.EqualTo(4));

        Assert.That(columns[0].ColumnName, Is.EqualTo("SourceDirectory"));
        Assert.That(columns[1].ColumnName, Is.EqualTo("DestinationDirectory"));
        Assert.That(columns[2].ColumnName, Is.EqualTo("IncludeSubFolders"));
        Assert.That(columns[3].ColumnName, Is.EqualTo("DeleteAssetsNotInSource"));
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoSyncAssetsDirectoriesDefinition()
    {
        string[] validValues =
        [
            "D:\\source\\newFolder",
            "D:\\destination\\newFolder",
            "False",
            "True"
        ];

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition =
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(validValues);

        Assert.That(syncAssetsDirectoriesDefinition, Is.Not.Null);
        Assert.That(syncAssetsDirectoriesDefinition.SourceDirectory, Is.EqualTo("D:\\source\\newFolder"));
        Assert.That(syncAssetsDirectoriesDefinition.DestinationDirectory, Is.EqualTo("D:\\destination\\newFolder"));
        Assert.That(syncAssetsDirectoriesDefinition.IncludeSubFolders, Is.False);
        Assert.That(syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource, Is.True);
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoSyncAssetsDirectoriesDefinition()
    {
        string[] tooManyValues =
        [
            "D:\\source\\newFolder",
            "D:\\destination\\newFolder",
            "False",
            "True",
            "toto",
            "15"
        ];

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition =
            SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(tooManyValues);

        Assert.That(syncAssetsDirectoriesDefinition, Is.Not.Null);
        Assert.That(syncAssetsDirectoriesDefinition.SourceDirectory, Is.EqualTo("D:\\source\\newFolder"));
        Assert.That(syncAssetsDirectoriesDefinition.DestinationDirectory, Is.EqualTo("D:\\destination\\newFolder"));
        Assert.That(syncAssetsDirectoriesDefinition.IncludeSubFolders, Is.False);
        Assert.That(syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource, Is.True);
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[4];

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(nullValues));

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'value')"));
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception =
            Assert.Throws<IndexOutOfRangeException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(emptyArray));

        Assert.That(exception?.Message, Is.EqualTo("Index was outside the bounds of the array."));
    }

    [Test]
    public void ReadFunc_InvalidValues_ThrowsFormatException()
    {
        string[] invalidValues =
        [
            "D:\\source\\newFolder",
            "D:\\destination\\newFolder",
            "15",
            "False"
        ];

        FormatException? exception =
            Assert.Throws<FormatException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(invalidValues));

        Assert.That(exception?.Message, Is.EqualTo("String '15' was not recognized as a valid Boolean."));
    }

    [Test]
    public void
        WriteFunc_SyncAssetsDirectoriesDefinitionWithValidValues_ConvertsSyncAssetsDirectoriesDefinitionPropertiesToIndexInArray()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new()
        {
            SourceDirectory = "D:\\source\\newFolder",
            DestinationDirectory = "D:\\destination\\newFolder",
            IncludeSubFolders = true,
            DeleteAssetsNotInSource = false
        };

        object[] result = new object[4];
        for (int i = 0; i < 4; i++)
        {
            result[i] = SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition, i);
        }

        Assert.That(result[0], Is.EqualTo("D:\\source\\newFolder"));
        Assert.That(result[1], Is.EqualTo("D:\\destination\\newFolder"));
        Assert.That(result[2], Is.True);
        Assert.That(result[3], Is.False);
    }

    [Test]
    public void
        WriteFunc_SyncAssetsDirectoriesDefinitionWithPartialValues_ConvertsSyncAssetsDirectoriesDefinitionPropertiesToIndexInArrayWithSomeDefaultValues()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new()
        {
            SourceDirectory = "D:\\source\\newFolder",
            DestinationDirectory = "D:\\destination\\newFolder",
            DeleteAssetsNotInSource = false
        };

        object[] result = new object[4];
        for (int i = 0; i < 4; i++)
        {
            result[i] = SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition, i);
        }

        Assert.That(result[0], Is.EqualTo("D:\\source\\newFolder"));
        Assert.That(result[1], Is.EqualTo("D:\\destination\\newFolder"));
        Assert.That(result[2], Is.False);
        Assert.That(result[3], Is.False);
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = new()
        {
            SourceDirectory = "D:\\source\\newFolder",
            DestinationDirectory = "D:\\destination\\newFolder",
            IncludeSubFolders = true,
            DeleteAssetsNotInSource = false
        };

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition, 5));

        Assert.That(exception?.Message,
            Is.EqualTo("Specified argument was out of the range of valid values. (Parameter 'i')"));
    }

    [Test]
    public void WriteFunc_SyncAssetsDirectoriesDefinitionIsNull_ThrowsNullReferenceException()
    {
        SyncAssetsDirectoriesDefinition? syncAssetsDirectoriesDefinition = null;

        for (int i = 0; i < 4; i++)
        {
            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() =>
                SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition!, i));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }
    }
}
