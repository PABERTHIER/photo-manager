namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersTests
{
    private string? _dataDirectory;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";
    private string? _backupPath;

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(_backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void GetFolders_Folders_ReturnsCorrectFolders()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders = _assetRepository!.GetFolders();

            Assert.AreEqual(2, folders.Length);
            Assert.AreEqual(folderPath1, folders[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders[0].FolderId);
            Assert.AreEqual(folderPath2, folders[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders[1].FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolders_NoFolders_ReturnsEmptyArray()
    {
        try
        {
            Folder[] folders = _assetRepository!.GetFolders();

            Assert.IsEmpty(folders);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetFolders_ConcurrentAccess_FoldersAreHandledSafely()
    {
        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            Folder[] folders1 = [];
            Folder[] folders2 = [];
            Folder[] folders3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => folders1 = _assetRepository!.GetFolders(),
                () => folders2 = _assetRepository!.GetFolders(),
                () => folders3 = _assetRepository!.GetFolders()
            );

            Assert.AreEqual(2, folders1.Length);
            Assert.AreEqual(folderPath1, folders1[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders1[0].FolderId);
            Assert.AreEqual(folderPath2, folders1[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders1[1].FolderId);

            Assert.AreEqual(2, folders2.Length);
            Assert.AreEqual(folderPath1, folders2[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders2[0].FolderId);
            Assert.AreEqual(folderPath2, folders2[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders2[1].FolderId);

            Assert.AreEqual(2, folders3.Length);
            Assert.AreEqual(folderPath1, folders3[0].Path);
            Assert.AreEqual(addedFolder1.FolderId, folders3[0].FolderId);
            Assert.AreEqual(folderPath2, folders3[1].Path);
            Assert.AreEqual(addedFolder2.FolderId, folders3[1].FolderId);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
        }
    }
}
