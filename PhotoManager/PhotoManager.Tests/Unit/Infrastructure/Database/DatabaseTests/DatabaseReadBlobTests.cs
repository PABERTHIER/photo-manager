using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseReadBlobTests
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
    public void ReadBlob_FileExistsAndSemicolonSeparator_ReturnsBlob()
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
                Constants.SEMICOLON_SEPARATOR,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);
            Assert.That(File.Exists(blobFilePath), Is.True);

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(_database!.Diagnostics.LastReadFilePath, Is.EqualTo(blobFilePath));
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
            Assert.That(blob.ContainsKey(FileNames.IMAGE1_JPG), Is.True);
            Assert.That(blob[FileNames.IMAGE1_JPG], Is.EqualTo(blobToWrite[FileNames.IMAGE1_JPG]).AsCollection);
            Assert.That(blob[FileNames.IMAGE_2_PNG], Is.EqualTo(blobToWrite[FileNames.IMAGE_2_PNG]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadBlob_FileExistsAndPipeSeparator_ReturnsBlob()
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

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(_database!.Diagnostics.LastReadFilePath, Is.EqualTo(blobFilePath));
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
            Assert.That(blob.ContainsKey(FileNames.IMAGE1_JPG), Is.True);
            Assert.That(blob[FileNames.IMAGE1_JPG], Is.EqualTo(blobToWrite[FileNames.IMAGE1_JPG]).AsCollection);
            Assert.That(blob[FileNames.IMAGE_2_PNG], Is.EqualTo(blobToWrite[FileNames.IMAGE_2_PNG]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadBlob_FileDoesNotExistsAndPipeSeparator_ReturnsNull()
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

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Null);
            Assert.That(_database!.Diagnostics.LastReadFilePath, Is.EqualTo(blobFilePath));

            _testLogger!.AssertLogExceptions([], typeof(PhotoManager.Infrastructure.Database.Database));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void ReadBlob_ExceptionIsThrown_LogsItAndThrowsException()
    {
        string blobName = Guid.NewGuid() + ".bin";
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath,
            _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);

        string logMessage = $"Error while trying to read blob {blobName}. LastReadFilePath: {blobFilePath}";
        const string exceptionMessage = "Blob read error";
        IOException expectedException = new(exceptionMessage);

        try
        {
            IBlobStorage blobStorageMock = Substitute.For<IBlobStorage>();
            blobStorageMock.ReadFromBinaryFile(Arg.Any<string>()).Throws(expectedException);

            PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(),
                blobStorageMock, new BackupStorage(), _testLogger!);

            database.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            IOException? exception = Assert.Throws<IOException>(() => database.ReadBlob(blobName));
            Assert.That(exception?.Message, Is.EqualTo(exceptionMessage));

            _testLogger!.AssertLogErrors([logMessage], typeof(PhotoManager.Infrastructure.Database.Database));
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
