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
            TableName = "Folder",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "Path" }
            }
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey("Folder"));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary["Folder"].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary["Folder"].ColumnProperties);
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
            TableName = "Asset",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "FolderId" },
                new ColumnProperties { ColumnName = "FileName" },
                new ColumnProperties { ColumnName = "FileSize" },
                new ColumnProperties { ColumnName = "ImageRotation" },
                new ColumnProperties { ColumnName = "PixelWidth" },
                new ColumnProperties { ColumnName = "PixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailPixelWidth" },
                new ColumnProperties { ColumnName = "ThumbnailPixelHeight" },
                new ColumnProperties { ColumnName = "ThumbnailCreationDateTime" },
                new ColumnProperties { ColumnName = "Hash" },
                new ColumnProperties { ColumnName = "AssetCorruptedMessage" },
                new ColumnProperties { ColumnName = "IsAssetCorrupted" },
                new ColumnProperties { ColumnName = "AssetRotatedMessage" },
                new ColumnProperties { ColumnName = "IsAssetRotated" }
            }
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey("Asset"));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary["Asset"].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary["Asset"].ColumnProperties);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void SetDataTableProperties_ImportType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        DataTableProperties properties = new()
        {
            TableName = "Import",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "SourceDirectory" },
                new ColumnProperties { ColumnName = "DestinationDirectory" },
                new ColumnProperties { ColumnName = "IncludeSubFolders" },
                new ColumnProperties { ColumnName = "DeleteAssetsNotInSource" }
            }
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey("Import"));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary["Import"].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary["Import"].ColumnProperties);
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
            TableName = "RecentTargetPaths",
            ColumnProperties = new ColumnProperties[]
            {
                new ColumnProperties { ColumnName = "Path" }
            }
        };

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);
            _testableDatabase!.SetDataTableProperties(properties);
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsNotEmpty(dataTablePropertiesDictionary);
            Assert.AreEqual(1, dataTablePropertiesDictionary.Count);
            Assert.IsTrue(dataTablePropertiesDictionary.ContainsKey("RecentTargetPaths"));
            CollectionAssert.AreEqual(properties.TableName, dataTablePropertiesDictionary["RecentTargetPaths"].TableName);
            CollectionAssert.AreEqual(properties.ColumnProperties, dataTablePropertiesDictionary["RecentTargetPaths"].ColumnProperties);
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
            ColumnProperties = null
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
    public void SetDataTableProperties_WithInvalidColumnName_ThrowsArgumentException(string columnName)
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

        var exception = Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
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
