using Directories = PhotoManager.Tests.Unit.Constants.Directories;
using FileNames = PhotoManager.Tests.Unit.Constants.FileNames;

namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseWriteBlobTests
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

        _userConfigurationService = new(configurationRootMock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
    }

    [Test]
    public void WriteBlob_BlobToWriteIsCorrectAndSemicolonSeparator_WritesBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { FileNames.IMAGE1_JPG, [1, 2, 3]},
            { FileNames.IMAGE_2_PNG, [4, 5, 6]}
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
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(blobFilePath));
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.EqualTo(blobToWrite));

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
            Assert.That(blob.ContainsKey(FileNames.IMAGE1_JPG), Is.True);
            Assert.That(blob[FileNames.IMAGE1_JPG], Is.EqualTo(blobToWrite[FileNames.IMAGE1_JPG]).AsCollection);
            Assert.That(blob[FileNames.IMAGE_2_PNG], Is.EqualTo(blobToWrite[FileNames.IMAGE_2_PNG]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteBlob_BlobToWriteIsCorrectAndPipeSeparator_WritesBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
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
            Assert.That(File.Exists(blobFilePath), Is.True);
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(blobFilePath));
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.EqualTo(blobToWrite));

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
            Assert.That(blob.ContainsKey(FileNames.IMAGE1_JPG), Is.True);
            Assert.That(blob[FileNames.IMAGE1_JPG], Is.EqualTo(blobToWrite[FileNames.IMAGE1_JPG]).AsCollection);
            Assert.That(blob[FileNames.IMAGE_2_PNG], Is.EqualTo(blobToWrite[FileNames.IMAGE_2_PNG]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteBlob_BlobToWriteIsNullAndPipeSeparator_ThrowsNullReferenceException()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]>? blobToWrite = null;

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _database!.WriteBlob(blobToWrite!, blobName));

            Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
            Assert.That(File.Exists(blobFilePath), Is.True);
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(blobFilePath));
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.EqualTo(blobToWrite));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }

    [Test]
    public void WriteBlob_BlobToWriteIsEmptyAndPipeSeparator_WritesNothingInTheFile()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS);
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = [];

        try
        {
            _database!.Initialize(
                directoryPath,
                _userConfigurationService!.StorageSettings.Separator,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);
            Assert.That(File.Exists(blobFilePath), Is.True);
            Assert.That(_database!.Diagnostics.LastWriteFilePath, Is.EqualTo(blobFilePath));
            Assert.That(_database!.Diagnostics.LastWriteFileRaw, Is.EqualTo(blobToWrite));

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, Directories.DATABASE_TESTS_BACKUPS), true);
        }
    }
}
