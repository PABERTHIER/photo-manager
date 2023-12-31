namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryFolderHasThumbnailsTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;
    private PhotoManager.Infrastructure.Database.Database? _database;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        _database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(_database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void FolderHasThumbnails_ThumbnailsExist_ReturnsTrue()
    {
        try
        {
            Folder folder = new() { FolderId = Guid.NewGuid(), Path = dataDirectory! };
            _database!.WriteBlob(new Dictionary<string, byte[]>(), folder.ThumbnailsFilename);

            bool folderHasThumbnails = _assetRepository!.FolderHasThumbnails(folder);

            Assert.IsTrue(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void FolderHasThumbnails_ThumbnailsDoNotExist_ReturnsFalse()
    {
        try
        {
            Folder folder = new() { FolderId = Guid.NewGuid(), Path = dataDirectory! };

            bool folderHasThumbnails = _assetRepository!.FolderHasThumbnails(folder);

            Assert.IsFalse(folderHasThumbnails);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void FolderHasThumbnails_FolderIsNull_ThrowsNullReferenceException()
    {
        try
        {
            Folder? folder = null;

            NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => _assetRepository!.FolderHasThumbnails(folder!));

            Assert.AreEqual("Object reference not set to an instance of an object.", exception?.Message);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
