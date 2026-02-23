using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseSetDataTablePropertiesTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private TestableDatabase? _testableDatabase;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        _testableDatabase = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void SetDataTableProperties_FolderType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
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
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary =
                _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
            Assert.That(dataTablePropertiesDictionary, Is.Not.Empty);
            Assert.That(dataTablePropertiesDictionary, Has.Count.EqualTo(1));
            Assert.That(
                dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings
                    .FoldersTableName), Is.True);
            Assert.That(
                dataTablePropertiesDictionary[
                    _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName].TableName,
                Is.EqualTo(properties.TableName));
            Assert.That(
                dataTablePropertiesDictionary[
                    _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName].ColumnProperties,
                Is.EqualTo(properties.ColumnProperties).AsCollection);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void SetDataTableProperties_AssetType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
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
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary =
                _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
            Assert.That(dataTablePropertiesDictionary, Is.Not.Empty);
            Assert.That(dataTablePropertiesDictionary, Has.Count.EqualTo(1));
            Assert.That(
                dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings
                    .AssetsTableName), Is.True);
            Assert.That(
                dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName]
                    .TableName, Is.EqualTo(properties.TableName));
            Assert.That(
                dataTablePropertiesDictionary[_userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName]
                    .ColumnProperties, Is.EqualTo(properties.ColumnProperties).AsCollection);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void SetDataTableProperties_SyncAssetsDirectoriesDefinitionType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        DataTableProperties properties = new()
        {
            TableName = _userConfigurationService!.StorageSettings.TablesSettings
                .SyncAssetsDirectoriesDefinitionsTableName,
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
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary =
                _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
            Assert.That(dataTablePropertiesDictionary, Is.Not.Empty);
            Assert.That(dataTablePropertiesDictionary, Has.Count.EqualTo(1));
            Assert.That(
                dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings
                    .SyncAssetsDirectoriesDefinitionsTableName), Is.True);
            Assert.That(
                dataTablePropertiesDictionary[
                        _userConfigurationService!.StorageSettings.TablesSettings
                            .SyncAssetsDirectoriesDefinitionsTableName]
                    .TableName, Is.EqualTo(properties.TableName));
            Assert.That(
                dataTablePropertiesDictionary[
                        _userConfigurationService!.StorageSettings.TablesSettings
                            .SyncAssetsDirectoriesDefinitionsTableName]
                    .ColumnProperties, Is.EqualTo(properties.ColumnProperties).AsCollection);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void SetDataTableProperties_RecentTargetPathsType_SetsDataTableProperties()
    {
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
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
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary =
                _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
            Assert.That(dataTablePropertiesDictionary, Is.Not.Empty);
            Assert.That(dataTablePropertiesDictionary, Has.Count.EqualTo(1));
            Assert.That(
                dataTablePropertiesDictionary.ContainsKey(_userConfigurationService!.StorageSettings.TablesSettings
                    .RecentTargetPathsTableName), Is.True);
            Assert.That(
                dataTablePropertiesDictionary[
                    _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName].TableName,
                Is.EqualTo(properties.TableName));
            Assert.That(
                dataTablePropertiesDictionary[
                        _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName]
                    .ColumnProperties, Is.EqualTo(properties.ColumnProperties).AsCollection);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void SetDataTableProperties_WithNullProperties_ThrowsArgumentNullException()
    {
        DataTableProperties? dataTableProperties = null;

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(dataTableProperties!));
        Assert.That(exception?.ParamName, Is.EqualTo("dataTableProperties"));
    }

    [Test]
    public void SetDataTableProperties_WithNullColumnProperties_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = null!
        };

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo("Column properties must not be empty."));
    }

    [Test]
    public void SetDataTableProperties_WithEmptyColumnProperties_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = []
        };

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo("Column properties must not be empty."));
    }

    [Test]
    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public void SetDataTableProperties_WithInvalidColumnName_ThrowsArgumentNullException(string columnName)
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties =
            [
                new() { ColumnName = "Column1" },
                new() { ColumnName = columnName }
            ]
        };

        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message,
            Is.EqualTo("All column properties should have a ColumnName (Parameter 'ColumnName')"));
        Assert.That(exception?.ParamName, Is.EqualTo(nameof(ColumnProperties.ColumnName)));
    }

    [Test]
    public void SetDataTableProperties_WithDuplicateColumnNames_ThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties =
            [
                new() { ColumnName = "Column1" },
                new() { ColumnName = "Column1" }
            ]
        };

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo("Duplicated column properties. (Parameter 'Column1')"));
        Assert.That(exception?.ParamName, Is.EqualTo("Column1"));
    }
}
