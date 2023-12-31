namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFolderByPathTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

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
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void GetFolderByPath_DifferentPaths_ReturnsCorrectFolder()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.FolderId, folderByPath2.FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolderByPath_SamePath_ReturnsFirstFolder()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath1);

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);
            Folder? folderByPath2 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath1, folderByPath2!.Path);
            Assert.AreNotEqual(addedFolder2.FolderId, folderByPath2.FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolderByPath_PathNotRegistered_ReturnsNull()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1);

            Assert.IsNull(folderByPath1);

        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolderByPath_PathIsNull_ReturnsNull()
    {
        try
        {
            string? folderPath1 = null;

            _assetRepository!.AddFolder(Path.Combine(dataDirectory!, "TestFolder1"));

            Folder? folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1!);

            Assert.IsNull(folderByPath1);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolderByPath_ConcurrentAccess_FoldersAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder? folderByPath1 = new() { Path = dataDirectory! };
            Folder? folderByPath2 = new() { Path = dataDirectory! };

            // Simulate concurrent access
            Parallel.Invoke(
                () => folderByPath1 = _assetRepository!.GetFolderByPath(folderPath1),
                () => folderByPath2 = _assetRepository!.GetFolderByPath(folderPath2)
            );

            Assert.IsNotNull(folderByPath1);
            Assert.IsNotNull(folderByPath2);

            Assert.AreEqual(folderPath1, folderByPath1!.Path);
            Assert.AreEqual(addedFolder1.FolderId, folderByPath1.FolderId);

            Assert.AreEqual(folderPath2, folderByPath2!.Path);
            Assert.AreEqual(addedFolder2.FolderId, folderByPath2.FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
