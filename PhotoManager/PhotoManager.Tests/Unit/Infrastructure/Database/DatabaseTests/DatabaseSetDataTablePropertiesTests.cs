namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseSetDataTablePropertiesTests
{
    private string? dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private TestableDatabase? _testableDatabase;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _testableDatabase = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void SetDataTableProperties_FolderType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName,
            ColumnProperties = FolderConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName].ColumnProperties);
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
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName,
            ColumnProperties = AssetConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName].ColumnProperties);
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
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName,
            ColumnProperties = SyncAssetsDirectoriesDefinitionConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName].ColumnProperties);
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
            TableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName,
            ColumnProperties = RecentPathsConfigs.ConfigureDataTable()
        };

        try
        {
            _testableDatabase!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName].ColumnProperties);
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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(dataTableProperties!));
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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
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

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(properties));
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

        ArgumentException? exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.AreEqual("Duplicated column properties. (Parameter 'Column1')", exception?.Message);
        Assert.AreEqual("Column1", exception?.ParamName);
    }
}
