using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetFoldersPathTests
{
    private string? _dataDirectory;
    private string? _backupPath;
    private const string BACKUP_END_PATH = "DatabaseTests\\v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _backupPath = Path.Combine(_dataDirectory, BACKUP_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_backupPath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void GetFoldersPath_Folders_ReturnsCorrectFoldersPath()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            string folderPath1 = Path.Combine(_dataDirectory!, "TestFolder1");
            string folderPath2 = Path.Combine(_dataDirectory!, "TestFolder2");

            Folder addedFolder1 = _assetRepository!.AddFolder(folderPath1);
            Folder addedFolder2 = _assetRepository!.AddFolder(folderPath2);

            HashSet<string> foldersPath = _assetRepository!.GetFoldersPath();

            Assert.That(foldersPath, Has.Count.EqualTo(2));
            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(foldersPath.Contains(folderPath1), Is.True);
            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));
            Assert.That(foldersPath.Contains(folderPath2), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
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
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            HashSet<string> foldersPath = _assetRepository!.GetFoldersPath();

            Assert.That(foldersPath, Is.Empty);

            Assert.That(assetsUpdatedEvents, Is.Empty);
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
        List<Reactive.Unit> assetsUpdatedEvents = [];
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

            Assert.That(addedFolder1.Path, Is.EqualTo(folderPath1));
            Assert.That(addedFolder2.Path, Is.EqualTo(folderPath2));

            Assert.That(foldersPath1, Has.Count.EqualTo(2));
            Assert.That(foldersPath1.Contains(folderPath1), Is.True);
            Assert.That(foldersPath1.Contains(folderPath2), Is.True);

            Assert.That(foldersPath2, Has.Count.EqualTo(2));
            Assert.That(foldersPath2.Contains(folderPath1), Is.True);
            Assert.That(foldersPath2.Contains(folderPath2), Is.True);

            Assert.That(foldersPath3, Has.Count.EqualTo(2));
            Assert.That(foldersPath3.Contains(folderPath1), Is.True);
            Assert.That(foldersPath3.Contains(folderPath2), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
