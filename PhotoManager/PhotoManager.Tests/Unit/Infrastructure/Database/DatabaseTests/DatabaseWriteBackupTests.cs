namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBackupTests
{
    private string? dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
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
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void WriteBackup_BackupDoesNotExist_SuccessfullyWritesBackup()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(dataDirectory!, "DatabaseTests_Backups", backupName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated = _database!.WriteBackup(backupDate);

            Assert.IsTrue(backupCreated);
            Assert.AreEqual(filePath, _database!.Diagnostics.LastWriteFilePath);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void WriteBackup_BackupExists_OverwritesBackup()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(dataDirectory!, "DatabaseTests_Backups", backupName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool backupCreated1 = _database!.WriteBackup(backupDate);
            bool backupCreated2 = _database!.WriteBackup(backupDate);

            Assert.IsTrue(backupCreated1);
            Assert.IsTrue(backupCreated2);
            Assert.AreEqual(filePath, _database!.Diagnostics.LastWriteFilePath);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
