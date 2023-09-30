namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseSetDataTablePropertiesTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;
    private TestableDatabase? _testableDatabase;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _testableDatabase = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void SetDataTableProperties_FolderType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = AssetConstants.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(AssetConstants.FoldersTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[AssetConstants.FoldersTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[AssetConstants.FoldersTableName].ColumnProperties);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void SetDataTableProperties_AssetType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = AssetConstants.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(AssetConstants.AssetsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[AssetConstants.AssetsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[AssetConstants.AssetsTableName].ColumnProperties);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void SetDataTableProperties_SyncAssetsDirectoriesDefinitionType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = AssetConstants.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(AssetConstants.SyncAssetsDirectoriesDefinitionsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[AssetConstants.SyncAssetsDirectoriesDefinitionsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[AssetConstants.SyncAssetsDirectoriesDefinitionsTableName].ColumnProperties);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void SetDataTableProperties_RecentTargetPathsType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = AssetConstants.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(AssetConstants.RecentTargetPathsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[AssetConstants.RecentTargetPathsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[AssetConstants.RecentTargetPathsTableName].ColumnProperties);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void SetDataTableProperties_WithNullProperties_ThrowsArgumentNullException()
    {
        DataTableProperties? dataTableProperties = null;

        var exception = Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(dataTableProperties!));
        Assert.AreEqual("dataTableProperties", exception?.ParamName);
    }

    [Test]
    public void SetDataTableProperties_WithNullColumnProperties_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = null!
        };

        var exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.AreEqual("Column properties must not be empty.", exception?.Message);
    }

    [Test]
    public void SetDataTableProperties_WithEmptyColumnProperties_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = Array.Empty<ColumnProperties>()
        };

        var exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.AreEqual("Column properties must not be empty.", exception?.Message);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void SetDataTableProperties_WithInvalidColumnName_ThrowsArgumentNullException(string columnName)
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Column1" },
                new ColumnProperties { ColumnName = columnName }
            }
        };

        var exception = Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(properties));
        Assert.AreEqual("All column properties should have a ColumnName (Parameter 'ColumnName')", exception?.Message);
        Assert.AreEqual(nameof(ColumnProperties.ColumnName), exception?.ParamName);
    }

    [Test]
    public void SetDataTableProperties_WithDuplicateColumnNames_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Column1" },
                new ColumnProperties { ColumnName = "Column1" }
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.AreEqual("Duplicated column properties. (Parameter 'Column1')", exception?.Message);
        Assert.AreEqual("Column1", exception?.ParamName);
    }
}
