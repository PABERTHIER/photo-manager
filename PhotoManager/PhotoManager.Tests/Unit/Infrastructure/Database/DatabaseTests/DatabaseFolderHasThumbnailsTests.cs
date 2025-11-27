using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

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
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

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
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { FileNames.IMAGE1_JPG, [1, 2, 3]},
            { FileNames.IMAGE_2_PNG, [4, 5, 6]}
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

            Assert.That(folderHasThumbnails, Is.True);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void FolderHasThumbnails_BlobDoesNotExist_ReturnsFalse()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            bool folderHasThumbnails = _database!.FolderHasThumbnails(blobName);

            Assert.That(folderHasThumbnails, Is.False);
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
