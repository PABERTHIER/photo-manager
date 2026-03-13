using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseDeleteBlobFileTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage(), _testLogger);
    }

    [TearDown]
    public void TearDown()
    {
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void DeleteBlobFile_BlobExists_DeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { FileNames.IMAGE1_JPG, [1, 2, 3] },
            { FileNames.IMAGE_2_PNG, [4, 5, 6] }
        };

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);

            Assert.That(File.Exists(blobFilePath), Is.True);

            _database!.DeleteBlobFile(blobName);

            Assert.That(File.Exists(blobFilePath), Is.False);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void DeleteBlobFile_BlobDoesNotExist_DoesNotDeleteFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            Assert.That(File.Exists(blobFilePath), Is.False);

            _database!.DeleteBlobFile(blobName);

            Assert.That(File.Exists(blobFilePath), Is.False);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void DeleteBlobFile_FileIsLocked_LogsItAndThrowsIOException()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobsDirectory = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);
        string blobFilePath = Path.Combine(blobsDirectory, blobName);

        string logMessage = $"Error while trying to delete blob {blobName}.";
        string exceptionMessage =
            $"The process cannot access the file '{blobFilePath}' because it is being used by another process.";

        try
        {
            Directory.CreateDirectory(blobsDirectory);
            File.WriteAllText(blobFilePath, "test content");

            // Open a file stream to lock the file
            using (new FileStream(blobFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                _database!.Initialize(
                    directoryPath,
                    _userConfigurationService!.StorageSettings.Separator,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                    _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

                IOException? exception = Assert.Throws<IOException>(() => _database!.DeleteBlobFile(blobName));
                Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

                _testLogger!.AssertLogErrors([logMessage], typeof(PhotoManager.Infrastructure.Database.Database));
            }
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            string backupsPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS);

            if (Directory.Exists(backupsPath))
            {
                Directory.Delete(backupsPath, true);
            }
        }
    }
}
