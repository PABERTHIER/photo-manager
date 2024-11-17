namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteThumbnailsTests
{
    private string? dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");

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
    public void DeleteThumbnails_BlobExists_DeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
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

            Assert.IsTrue(File.Exists(blobFilePath));

            _database!.DeleteThumbnails(blobName);

            Assert.IsFalse(File.Exists(blobFilePath));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void DeleteThumbnails_BlobDoesNotExist_DoesNotDeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this FolderId + ".bin"
        string directoryPath = Path.Combine(dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            Assert.IsFalse(File.Exists(blobFilePath));

            _database!.DeleteThumbnails(blobName);

            Assert.IsFalse(File.Exists(blobFilePath));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
