namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseInitializeTests
{
    private string? dataDirectory;

    private TestableDatabase? _testableDatabase;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void Setup()
    {
        _testableDatabase = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void Initialize_ValidPath_InitializeData()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _testableDatabase!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            string tablesDirectory = _testableDatabase!.GetTablesDirectory();
            string blobsDirectory = _testableDatabase!.GetBlobsDirectory();
            string backupsDirectory = _testableDatabase!.GetBackupsDirectory();
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.AreEqual(directoryPath, _testableDatabase!.DataDirectory);
            Assert.AreEqual(Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables), tablesDirectory);
            Assert.AreEqual(Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs), blobsDirectory);
            Assert.AreEqual(directoryPath + "_Backups", backupsDirectory);
            Assert.AreEqual(_userConfigurationService!.StorageSettings.Separator, _testableDatabase!.Separator);
            Assert.IsNotNull(dataTablePropertiesDictionary);
            Assert.IsEmpty(dataTablePropertiesDictionary);
            Assert.IsTrue(Directory.Exists(directoryPath));
            Assert.IsTrue(Directory.Exists(tablesDirectory));
            Assert.IsTrue(Directory.Exists(blobsDirectory));
            Assert.IsTrue(Directory.Exists(backupsDirectory));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void Initialize_InvalidPath_InitializeData()
    {
        string? directoryPath = null;

        ArgumentNullException? exception = Assert.Throws<ArgumentNullException>(() =>
        {
            _testableDatabase!.Initialize(
                directoryPath!,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        });

        Assert.AreEqual("Value cannot be null. (Parameter 'path1')", exception?.Message);

        string tablesDirectory = _testableDatabase!.GetTablesDirectory();
        string blobsDirectory = _testableDatabase!.GetBlobsDirectory();
        string backupsDirectory = _testableDatabase!.GetBackupsDirectory();
        Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

        Assert.AreEqual(directoryPath, _testableDatabase!.DataDirectory);
        Assert.AreEqual(string.Empty, tablesDirectory);
        Assert.AreEqual(string.Empty, blobsDirectory);
        Assert.AreEqual(string.Empty, backupsDirectory);
        Assert.AreEqual('\0', _testableDatabase!.Separator);
        Assert.IsNotNull(dataTablePropertiesDictionary);
        Assert.IsEmpty(dataTablePropertiesDictionary);
    }
}
