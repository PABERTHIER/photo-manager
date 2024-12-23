namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class RecentPathsConfigsTests
{
    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = RecentPathsConfigs.ConfigureDataTable();

        Assert.That(columns, Is.Not.Null);
        Assert.That(columns, Has.Length.EqualTo(1));

        Assert.That(columns[0].ColumnName, Is.EqualTo("Path"));
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoRecentPath()
    {
        string[] validValues =
        [
            "D:\\folder\\newFolder"
        ];

        string recentPath = RecentPathsConfigs.ReadFunc(validValues);

        Assert.That(recentPath, Is.Not.Null);
        Assert.That(recentPath, Is.EqualTo("D:\\folder\\newFolder"));
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoRecentPath()
    {
        string[] tooManyValues =
        [
            "D:\\folder\\newFolder1",
            "D:\\folder\\newFolder2"
        ];

        string recentPath = RecentPathsConfigs.ReadFunc(tooManyValues);

        Assert.That(recentPath, Is.Not.Null);
        Assert.That(recentPath, Is.EqualTo("D:\\folder\\newFolder1"));
    }

    [Test]
    public void ReadFunc_NullValues_ParsesStringArrayIntoNullValue()
    {
        string[] nullValues = new string[1];

        string recentPath = RecentPathsConfigs.ReadFunc(nullValues);

        Assert.That(recentPath, Is.Null);
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArray = [];

        IndexOutOfRangeException? exception = Assert.Throws<IndexOutOfRangeException>(() => RecentPathsConfigs.ReadFunc(emptyArray));

        Assert.That(exception?.Message, Is.EqualTo("Index was outside the bounds of the array."));
    }

    [Test]
    public void WriteFunc_RecentPathWithValidValues_ConvertsRecentPathPropertiesToIndexInArray()
    {
        const string recentPath = "D:\\folder\\newFolder";

        object result = RecentPathsConfigs.WriteFunc(recentPath, 0);

        Assert.That(result, Is.EqualTo(recentPath));
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        const string recentPath = "D:\\folder\\newFolder";

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecentPathsConfigs.WriteFunc(recentPath, 2));

        Assert.That(exception?.Message, Is.EqualTo("Specified argument was out of the range of valid values. (Parameter 'i')"));
    }

    [Test]
    public void WriteFunc_RecentPathIsNull_ConvertsNullValuePropertiesToIndexInArray()
    {
        string? recentPath = null;

        object result = RecentPathsConfigs.WriteFunc(recentPath!, 0);

        Assert.That(result, Is.Null);
    }
}
