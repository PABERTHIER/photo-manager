using NUnit.Framework;
using PhotoManager.Constants;
using PhotoManager.Infrastructure.Database.Storage;
using System.IO;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBackupTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private readonly char pipeSeparator = AssetConstants.Separator.ToCharArray().First();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        string directoryName = Path.GetDirectoryName(typeof(DatabaseWriteBackupTests).Assembly.Location) ?? "";
        dataDirectory = Path.Combine(directoryName, "TestFiles");
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
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
            _database!.Initialize(directoryPath, pipeSeparator);
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
    public void WriteBackup_BackupExists_DoesNotWriteBackup()
    {
        DateTime backupDate = DateTime.Now;
        string backupName = backupDate.ToString("yyyyMMdd") + ".zip";

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string filePath = Path.Combine(dataDirectory!, "DatabaseTests_Backups", backupName);

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);
            bool backupCreated1 = _database!.WriteBackup(backupDate);
            bool backupCreated2 = _database!.WriteBackup(backupDate);

            Assert.IsFalse(backupCreated2);
            Assert.AreEqual(filePath, _database!.Diagnostics.LastWriteFilePath);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
