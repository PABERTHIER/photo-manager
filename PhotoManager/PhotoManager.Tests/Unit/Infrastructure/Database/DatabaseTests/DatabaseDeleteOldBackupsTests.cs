namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteOldBackupsTests
{
    private string? dataDirectory;

    private UserConfigurationService? _userConfigurationService;
    private Mock<IBackupStorage>? _backupStorageMock;

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
        _backupStorageMock = new Mock<IBackupStorage>();
    }

    [Test]
    public void DeleteOldBackups_ExcessBackups_SuccessfullyDeletesBackups()
    {
        ushort backupsToKeep = 3;
        string path1 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230404.zip");
        string path2 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230304.zip");
        string path3 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230204.zip");
        string path4 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230104.zip");
        string path5 = Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230504.zip");

        string[] filesPath = new string[]
        {
            path1,
            path2,
            path3,
            path4,
            path5
        };

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _backupStorageMock!.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns(filesPath);

            PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), _backupStorageMock.Object);
            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            database.DeleteOldBackups(backupsToKeep);

            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(It.IsAny<string>()), Times.Exactly(2));
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path1), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path2), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path5), Times.Never);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path3), Times.Once);
            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(path4), Times.Once);

            Assert.IsNotNull(database.Diagnostics.LastDeletedBackupFilePaths);
            Assert.AreEqual(2, database.Diagnostics.LastDeletedBackupFilePaths!.Length);
            Assert.AreEqual(path4, database.Diagnostics.LastDeletedBackupFilePaths[0]);
            Assert.AreEqual(path3, database.Diagnostics.LastDeletedBackupFilePaths[1]);
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
        ushort backupsToKeep = 3;
        string[] filesPath = new string[]
        {
            Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230404.zip"),
            Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230304.zip"),
            Path.Combine(dataDirectory!, "DatabaseTests_Backups", "20230204.zip")
        };

        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");

        try
        {
            _backupStorageMock!.Setup(x => x.GetBackupFilesPaths(It.IsAny<string>())).Returns(filesPath);

            PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), _backupStorageMock.Object);
            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
            database.DeleteOldBackups(backupsToKeep);

            _backupStorageMock.Verify(bs => bs.DeleteBackupFile(It.IsAny<string>()), Times.Never);

            Assert.IsNotNull(database.Diagnostics.LastDeletedBackupFilePaths);
            Assert.AreEqual(0, database.Diagnostics.LastDeletedBackupFilePaths!.Length);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
