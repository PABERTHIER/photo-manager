using Directories = PhotoManager.Tests.Unit.Constants.Directories;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseSetDataTablePropertiesTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private TestableDatabase? _testableDatabase;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database>? _testLogger;

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
        _testLogger = new();
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
        _testableDatabase = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
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

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
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

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
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

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
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

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
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

        _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
    }

    [Test]
    public void SetDataTableProperties_WithNullColumnProperties_ThrowsNullReferenceException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = null!
        };

        NullReferenceException? exception =
            Assert.Throws<NullReferenceException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));

        _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
    }

    [Test]
    public void SetDataTableProperties_WithEmptyColumnProperties_LogsItAndThrowsArgumentException()
    {
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties = []
        };

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo("Column properties must not be empty."));

        ArgumentException expectedException = new("Column properties must not be empty.");
        _testLogger!.AssertLogExceptions([expectedException], typeof(PhotoManager.Infrastructure.Database.Database));
    }

    [Test]
    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public void SetDataTableProperties_WithInvalidColumnName_LogsItAndThrowsArgumentNullException(string columnName)
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

        ArgumentNullException expectedException = new(nameof(ColumnProperties.ColumnName),
            "All column properties should have a ColumnName");
        _testLogger!.AssertLogExceptions([expectedException], typeof(PhotoManager.Infrastructure.Database.Database));
    }

    [Test]
    public void SetDataTableProperties_WithDuplicateColumnNames_LogsItAndThrowsArgumentException()
    {
        const string duplicateColumnName = "Column1";
        DataTableProperties properties = new()
        {
            TableName = "TestTable",
            ColumnProperties =
            [
                new() { ColumnName = duplicateColumnName },
                new() { ColumnName = duplicateColumnName }
            ]
        };

        ArgumentException? exception =
            Assert.Throws<ArgumentException>(() => _database!.SetDataTableProperties(properties));
        Assert.That(exception?.Message, Is.EqualTo($"Duplicated column properties. (Parameter '{duplicateColumnName}')"));
        Assert.That(exception?.ParamName, Is.EqualTo(duplicateColumnName));

        ArgumentException expectedException = new("Duplicated column properties.", duplicateColumnName);
        _testLogger!.AssertLogExceptions([expectedException], typeof(PhotoManager.Infrastructure.Database.Database));
    }
}
