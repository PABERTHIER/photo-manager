﻿namespace PhotoManager.Tests.Integration.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteOldBackupsTests
{
    private string? dataDirectory;
    private PhotoManager.Infrastructure.Database.Database? _database;

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
    }

    [Test]
    public void DeleteOldBackups_DeletesExcessBackups_SuccessfullyDeletesBackups()
    {
        int backupsToKeep = 3;
        string path1 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230404.zip");
        string path2 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230304.zip");
        string path3 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230204.zip");
        string path4 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230104.zip");
        string path5 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230504.zip");

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            bool backupCreated1 = _database!.WriteBackup(new DateTime(2023, 04, 04));
            bool backupCreated2 = _database!.WriteBackup(new DateTime(2023, 03, 04));
            bool backupCreated3 = _database!.WriteBackup(new DateTime(2023, 02, 04));
            bool backupCreated4 = _database!.WriteBackup(new DateTime(2023, 01, 04));
            bool backupCreated5 = _database!.WriteBackup(new DateTime(2023, 05, 04));

            Assert.True(backupCreated1);
            Assert.True(backupCreated2);
            Assert.True(backupCreated3);
            Assert.True(backupCreated4);
            Assert.True(backupCreated5);

            _database!.DeleteOldBackups(backupsToKeep);

            Assert.IsTrue(File.Exists(path1));
            Assert.IsTrue(File.Exists(path2));
            Assert.IsFalse(File.Exists(path3));
            Assert.IsFalse(File.Exists(path4));
            Assert.IsTrue(File.Exists(path5));

            Assert.IsNotNull(_database!.Diagnostics.LastDeletedBackupFilePaths);
            Assert.AreEqual(2, _database!.Diagnostics.LastDeletedBackupFilePaths.Length);
            Assert.AreEqual(path4, _database!.Diagnostics.LastDeletedBackupFilePaths[0]);
            Assert.AreEqual(path3, _database!.Diagnostics.LastDeletedBackupFilePaths[1]);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void DeleteOldBackups_NoExcessBackups_NothingDeleted()
    {
        int backupsToKeep = 3;
        string path1 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230404.zip");
        string path2 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230304.zip");
        string path3 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230204.zip");

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(directoryPath, pipeSeparator);

            bool backupCreated1 = _database!.WriteBackup(new DateTime(2023, 04, 04));
            bool backupCreated2 = _database!.WriteBackup(new DateTime(2023, 03, 04));
            bool backupCreated3 = _database!.WriteBackup(new DateTime(2023, 02, 04));

            Assert.True(backupCreated1);
            Assert.True(backupCreated2);
            Assert.True(backupCreated3);

            _database!.DeleteOldBackups(backupsToKeep);

            Assert.IsTrue(File.Exists(path1));
            Assert.IsTrue(File.Exists(path2));
            Assert.IsTrue(File.Exists(path3));

            Assert.IsNotNull(_database!.Diagnostics.LastDeletedBackupFilePaths);
            Assert.AreEqual(0, _database!.Diagnostics.LastDeletedBackupFilePaths.Length);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}