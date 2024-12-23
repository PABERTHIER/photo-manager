﻿namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class SyncAssetsDirectoriesDefinitionConfigsTests
{
    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable();

        Assert.IsNotNull(columns);
        Assert.AreEqual(4, columns.Length);

        Assert.AreEqual("SourceDirectory", columns[0].ColumnName);
        Assert.AreEqual("DestinationDirectory", columns[1].ColumnName);
        Assert.AreEqual("IncludeSubFolders", columns[2].ColumnName);
        Assert.AreEqual("DeleteAssetsNotInSource", columns[3].ColumnName);
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

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(validValues);

        Assert.IsNotNull(syncAssetsDirectoriesDefinition);
        Assert.AreEqual("D:\\source\\newFolder", syncAssetsDirectoriesDefinition.SourceDirectory);
        Assert.AreEqual("D:\\destination\\newFolder", syncAssetsDirectoriesDefinition.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition.IncludeSubFolders);
        Assert.AreEqual(true, syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource);
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

        SyncAssetsDirectoriesDefinition syncAssetsDirectoriesDefinition = SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(tooManyValues);

        Assert.IsNotNull(syncAssetsDirectoriesDefinition);
        Assert.AreEqual("D:\\source\\newFolder", syncAssetsDirectoriesDefinition.SourceDirectory);
        Assert.AreEqual("D:\\destination\\newFolder", syncAssetsDirectoriesDefinition.DestinationDirectory);
        Assert.AreEqual(false, syncAssetsDirectoriesDefinition.IncludeSubFolders);
        Assert.AreEqual(true, syncAssetsDirectoriesDefinition.DeleteAssetsNotInSource);
    }

    [Test]
    public void ReadFunc_NullValues_ThrowsArgumentNullException()
    {
        string[] nullValues = new string[4];

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(nullValues));

        Assert.AreEqual("Value cannot be null. (Parameter 'value')", exception?.Message);
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(emptyArray));

        Assert.AreEqual("Index was outside the bounds of the array.", exception?.Message);
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

        FormatException? exception = Assert.Throws<FormatException>(() => SyncAssetsDirectoriesDefinitionConfigs.ReadFunc(invalidValues));

        Assert.AreEqual("String '15' was not recognized as a valid Boolean.", exception?.Message);
    }

    [Test]
    public void WriteFunc_SyncAssetsDirectoriesDefinitionWithValidValues_ConvertsSyncAssetsDirectoriesDefinitionPropertiesToIndexInArray()
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

        Assert.AreEqual("D:\\source\\newFolder", result[0]);
        Assert.AreEqual("D:\\destination\\newFolder", result[1]);
        Assert.AreEqual(true, result[2]);
        Assert.AreEqual(false, result[3]);
    }

    [Test]
    public void WriteFunc_SyncAssetsDirectoriesDefinitionWithPartialValues_ConvertsSyncAssetsDirectoriesDefinitionPropertiesToIndexInArrayWithSomeDefaultValues()
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

        Assert.AreEqual("D:\\source\\newFolder", result[0]);
        Assert.AreEqual("D:\\destination\\newFolder", result[1]);
        Assert.AreEqual(false, result[2]);
        Assert.AreEqual(false, result[3]);
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

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition, 5));

        Assert.AreEqual("Specified argument was out of the range of valid values. (Parameter 'i')", exception?.Message);
    }

    [Test]
    public void WriteFunc_SyncAssetsDirectoriesDefinitionIsNull_ThrowsNullReferenceException()
    {
        SyncAssetsDirectoriesDefinition? syncAssetsDirectoriesDefinition = null;

        for (int i = 0; i < 4; i++)
        {
            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => SyncAssetsDirectoriesDefinitionConfigs.WriteFunc(syncAssetsDirectoriesDefinition!, i));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        }
    }
}
