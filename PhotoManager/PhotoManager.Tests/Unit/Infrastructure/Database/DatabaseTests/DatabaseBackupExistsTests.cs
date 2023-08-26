namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseBackupExistsTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        string directoryName = Path.GetDirectoryName(typeof(DatabaseBackupExistsTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void BackupExists_BackupExists_ReturnsTrue()
    {
        DateTime backupDate = DateTime.Now;

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);
            bool backupCreated = _database!.WriteBackup(backupDate);
            bool backupExists = _database!.BackupExists(backupDate);

            Assert.IsTrue(backupExists);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void BackupExists_BackupDoesNotExist_ReturnsFalse()
    {
        DateTime backupDate = DateTime.Now;

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);
            bool backupExists = _database!.BackupExists(backupDate);

            Assert.IsFalse(backupExists);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
