namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseFolderHasThumbnailsTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
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
        _database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void FolderHasThumbnails_BlobExists_ReturnsTrue()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", new byte[] { 1, 2, 3 } },
            { "Image2.png", new byte[] { 4, 5, 6 } }
        };

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);

            bool folderHasThumbnails = _database!.FolderHasThumbnails(blobName);

            Assert.IsTrue(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void FolderHasThumbnails_BlobDoesNotExist_ReturnsFalse()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool folderHasThumbnails = _database!.FolderHasThumbnails(blobName);

            Assert.IsFalse(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
