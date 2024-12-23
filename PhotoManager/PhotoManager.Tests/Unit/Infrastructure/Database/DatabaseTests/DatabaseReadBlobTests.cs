namespace PhotoManager.Tests.Unit.Infrastructure.Database.DatabaseTests;

[TestFixture]
public class DatabaseReadBlobTests
{
    private string? _dataDirectory;

    private PhotoManager.Infrastructure.Database.Database? _database;
    private UserConfigurationService? _userConfigurationService;

    private const char SEMICOLON_SEPARATOR = ';';

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
    public void ReadBlob_FileExistsAndSemicolonSeparator_ReturnsBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", [1, 2, 3]},
            { "Image2.png", [4, 5, 6]}
        };

        try
        {
            _database!.Initialize(
                directoryPath,
                SEMICOLON_SEPARATOR,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables,
                _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs);

            _database!.WriteBlob(blobToWrite, blobName);
            Assert.That(File.Exists(blobFilePath), Is.True);

            Dictionary<string, byte[]>? blob = _database!.ReadBlob(blobName);

            Assert.That(blob, Is.Not.Null);
            Assert.That(blob, Is.InstanceOf<Dictionary<string, byte[]>?>());
            Assert.That(_database!.Diagnostics.LastReadFilePath, Is.EqualTo(blobFilePath));
            Assert.That(blob!, Has.Count.EqualTo(blobToWrite.Count));
            Assert.That(blob.ContainsKey("Image1.jpg"), Is.True);
            Assert.That(blob["Image1.jpg"], Is.EqualTo(blobToWrite["Image1.jpg"]).AsCollection);
            Assert.That(blob["Image2.png"], Is.EqualTo(blobToWrite["Image2.png"]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadBlob_FileExistsAndPipeSeparator_ReturnsBlob()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);
        Dictionary<string, byte[]> blobToWrite = new()
        {
            { "Image1.jpg", [1, 2, 3]},
            { "Image2.png", [4, 5, 6]}
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
            Assert.That(blob.ContainsKey("Image1.jpg"), Is.True);
            Assert.That(blob["Image1.jpg"], Is.EqualTo(blobToWrite["Image1.jpg"]).AsCollection);
            Assert.That(blob["Image2.png"], Is.EqualTo(blobToWrite["Image2.png"]).AsCollection);
            Assert.That(blobToWrite, Is.EquivalentTo(blob));
            Assert.That(blob, Is.EqualTo(blobToWrite));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }

    [Test]
    public void ReadBlob_FileDoesNotExistsAndPipeSeparator_ReturnsNull()
    {
        string blobName = Guid.NewGuid() + ".bin"; // The blobName is always like this: Folder.Id + ".bin"
        string directoryPath = Path.Combine(_dataDirectory!, "DatabaseTests");
        string blobFilePath = Path.Combine(directoryPath, _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs, blobName);

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
        }
        finally
        {
            Directory.Delete(directoryPath, true);
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests_Backups"), true);
        }
    }
}
