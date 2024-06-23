using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersPathTests
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
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void GetFoldersPath_Folders_ReturnsCorrectFoldersPath()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            HashSet<string> foldersPath = _assetRepository!.GetFoldersPath();

            Assert.AreEqual(2, foldersPath.Count);
            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.IsTrue(foldersPath.Contains(folderPath1));
            Assert.AreEqual(folderPath2, addedFolder2.Path);
            Assert.IsTrue(foldersPath.Contains(folderPath2));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFoldersPath_NoFolders_ReturnsEmptyHashSet()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            HashSet<string> foldersPath = _assetRepository!.GetFoldersPath();

            Assert.IsEmpty(foldersPath);

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void GetFoldersPath_ConcurrentAccess_FoldersPathAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            HashSet<string> foldersPath1 = [];
            HashSet<string> foldersPath2 = [];
            HashSet<string> foldersPath3 = [];

            // Simulate concurrent access
            Parallel.Invoke(
                () => foldersPath1 = _assetRepository!.GetFoldersPath(),
                () => foldersPath2 = _assetRepository!.GetFoldersPath(),
                () => foldersPath3 = _assetRepository!.GetFoldersPath()
            );

            Assert.AreEqual(folderPath1, addedFolder1.Path);
            Assert.AreEqual(folderPath2, addedFolder2.Path);

            Assert.AreEqual(2, foldersPath1.Count);
            Assert.IsTrue(foldersPath1.Contains(folderPath1));
            Assert.IsTrue(foldersPath1.Contains(folderPath2));

            Assert.AreEqual(2, foldersPath2.Count);
            Assert.IsTrue(foldersPath2.Contains(folderPath1));
            Assert.IsTrue(foldersPath2.Contains(folderPath2));

            Assert.AreEqual(2, foldersPath3.Count);
            Assert.IsTrue(foldersPath3.Contains(folderPath1));
            Assert.IsTrue(foldersPath3.Contains(folderPath2));

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
