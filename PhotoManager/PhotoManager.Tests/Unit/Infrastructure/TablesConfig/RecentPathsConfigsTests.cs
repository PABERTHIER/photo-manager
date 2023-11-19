namespace PhotoManager.Tests.Unit.Infrastructure.TablesConfig;

[TestFixture]
public class RecentPathsConfigsTests
{
    [Test]
    public void ConfigureDataTable_ReturnsCorrectColumnNames()
    {
        ColumnProperties[] columns = RecentPathsConfigs.ConfigureDataTable();

        Assert.IsNotNull(columns);
        Assert.AreEqual(1, columns.Length);

        Assert.AreEqual("Path", columns[0].ColumnName);
    }

    [Test]
    public void ReadFunc_ValidValues_ParsesStringArrayIntoRecentPath()
    {
        string[] validValues = new string[]
        {
            "D:\\folder\\newFolder"
        };

        string recentPath = RecentPathsConfigs.ReadFunc(validValues);

        Assert.IsNotNull(recentPath);
        Assert.AreEqual("D:\\folder\\newFolder", recentPath);
    }

    [Test]
    public void ReadFunc_TooManyValues_ParsesStringArrayIntoRecentPath()
    {
        string[] tooManyValues = new string[]
        {
            "D:\\folder\\newFolder1",
            "D:\\folder\\newFolder2"
        };

        string recentPath = RecentPathsConfigs.ReadFunc(tooManyValues);

        Assert.IsNotNull(recentPath);
        Assert.AreEqual("D:\\folder\\newFolder1", recentPath);
    }

    [Test]
    public void ReadFunc_NullValues_ParsesStringArrayIntoNullValue()
    {
        string[] nullValues = new string[1];

        string recentPath = RecentPathsConfigs.ReadFunc(nullValues);

        Assert.IsNull(recentPath);
    }

    [Test]
    public void ReadFunc_EmptyArray_ThrowsIndexOutOfRangeException()
    {
        string[] emptyArrray = Array.Empty<string>();

        Assert.Throws<IndexOutOfRangeException>(() => RecentPathsConfigs.ReadFunc(emptyArrray));
    }

    [Test]
    public void WriteFunc_RecentPathWithValidValues_ConvertsRecentPathPropertiesToIndexInArray()
    {
        string recentPath = "D:\\folder\\newFolder";

        object result = RecentPathsConfigs.WriteFunc(recentPath, 0);

        Assert.AreEqual(recentPath, result);
    }

    [Test]
    public void WriteFunc_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        string recentPath = "D:\\folder\\newFolder";

        ArgumentOutOfRangeException? exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecentPathsConfigs.WriteFunc(recentPath, 2));

        Assert.AreEqual("Specified argument was out of the range of valid values. (Parameter 'i')", exception?.Message);
    }

    [Test]
    public void WriteFunc_RecentPathIsNull_ConvertsNullValuePropertiesToIndexInArray()
    {
        string? recentPath = null;

        object result = RecentPathsConfigs.WriteFunc(recentPath!, 0);

        Assert.IsNull(result);
    }
}
