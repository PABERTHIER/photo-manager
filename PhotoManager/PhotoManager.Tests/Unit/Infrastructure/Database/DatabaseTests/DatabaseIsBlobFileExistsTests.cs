using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseIsBlobFileExistsTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;
    private TestLogger<PhotoManager.Infrastructure.Database.Database>? _testLogger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);

        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new(configurationRootMock);
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
    public void IsBlobFileExists_BlobExists_ReturnsTrue()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
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

            bool isBlobFileExists = _database!.IsBlobFileExists(blobName);

            Assert.That(isBlobFileExists, Is.True);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void IsBlobFileExists_BlobDoesNotExist_ReturnsFalse()
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

            bool isBlobFileExists = _database!.IsBlobFileExists(blobName);

            Assert.That(isBlobFileExists, Is.False);

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
