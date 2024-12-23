namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseInitializeTests
{
    private string? _dataDirectory;

    private TestableDatabase? _testableDatabase;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new (configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _testableDatabase = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void Initialize_ValidPath_InitializeData()
    {
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");

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

            Assert.That(_testableDatabase!.DataDirectory, Is.EqualTo(directoryPath));
            Assert.That(tablesDirectory, Is.EqualTo(Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables)));
            Assert.That(blobsDirectory, Is.EqualTo(Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs)));
            Assert.That(backupsDirectory, Is.EqualTo(directoryPath + "_Backups"));
            Assert.That(_testableDatabase!.Separator, Is.EqualTo(_userConfigurationService!.StorageSettings.Separator));
            Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
            Assert.That(dataTablePropertiesDictionary, Is.Empty);
            Assert.That(Directory.Exists(directoryPath), Is.True);
            Assert.That(Directory.Exists(tablesDirectory), Is.True);
            Assert.That(Directory.Exists(blobsDirectory), Is.True);
            Assert.That(Directory.Exists(backupsDirectory), Is.True);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
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

        Assert.That(exception?.Message, Is.EqualTo("Value cannot be null. (Parameter 'path1')"));

        string tablesDirectory = _testableDatabase!.GetTablesDirectory();
        string blobsDirectory = _testableDatabase!.GetBlobsDirectory();
        string backupsDirectory = _testableDatabase!.GetBackupsDirectory();
        Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

        Assert.That(_testableDatabase!.DataDirectory, Is.EqualTo(directoryPath));
        Assert.That(tablesDirectory, Is.EqualTo(string.Empty));
        Assert.That(blobsDirectory, Is.EqualTo(string.Empty));
        Assert.That(backupsDirectory, Is.EqualTo(string.Empty));
        Assert.That(_testableDatabase!.Separator, Is.EqualTo('\0'));
        Assert.That(dataTablePropertiesDictionary, Is.Not.Null);
        Assert.That(dataTablePropertiesDictionary, Is.Empty);
    }
}
