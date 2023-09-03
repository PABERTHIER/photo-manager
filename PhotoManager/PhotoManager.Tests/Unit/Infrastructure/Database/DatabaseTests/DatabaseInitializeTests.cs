namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseInitializeTests
{
    private string? dataDirectory;
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
        _testableDatabase = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void Initialize_ValidPath_InitializeData()
    {
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _testableDatabase!.Initialize(directoryPath, pipeSeparator);

            string tablesDirectory = _testableDatabase!.GetTablesDirectory();
            string blobsDirectory = _testableDatabase!.GetBlobsDirectory();
            string backupsDirectory = _testableDatabase!.GetBackupsDirectory();
            Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

            Assert.AreEqual(directoryPath, _testableDatabase!.DataDirectory);
            Assert.AreEqual(Path.Combine(directoryPath, AssetConstants.Tables), tablesDirectory);
            Assert.AreEqual(Path.Combine(directoryPath, AssetConstants.Blobs), blobsDirectory);
            Assert.AreEqual(directoryPath + "_Backups", backupsDirectory);
            Assert.AreEqual(pipeSeparator, _testableDatabase!.Separator);
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

        Assert.Throws<ArgumentNullException>(() => _testableDatabase!.Initialize(directoryPath!, pipeSeparator));

        string tablesDirectory = _testableDatabase!.GetTablesDirectory();
        string blobsDirectory = _testableDatabase!.GetBlobsDirectory();
        string backupsDirectory = _testableDatabase!.GetBackupsDirectory();
        Dictionary<string, DataTableProperties> dataTablePropertiesDictionary = _testableDatabase!.GetDataTablePropertiesDictionary();

        Assert.AreEqual(directoryPath, _testableDatabase!.DataDirectory);
        Assert.AreEqual(directoryPath, tablesDirectory);
        Assert.AreEqual(directoryPath, blobsDirectory);
        Assert.AreEqual(directoryPath, backupsDirectory);
        Assert.AreEqual('\0', _testableDatabase!.Separator);
        Assert.IsNull(dataTablePropertiesDictionary);
    }
}
