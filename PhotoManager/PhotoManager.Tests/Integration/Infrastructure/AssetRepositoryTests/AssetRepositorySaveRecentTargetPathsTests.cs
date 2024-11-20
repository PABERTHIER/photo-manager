using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositorySaveRecentTargetPathsTests
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
    public void SaveRecentTargetPaths_RecentTargetPaths_SaveRecentTargetPaths()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);
            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths[1]);

            Assert.IsTrue(_assetRepository.HasChanges());

            _assetRepository.SaveRecentTargetPaths([]);
            recentTargetPaths = _assetRepository.GetRecentTargetPaths();

            Assert.IsEmpty(recentTargetPaths);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void SaveRecentTargetPaths_ConcurrentAccess_RecentTargetPathsAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            List<string> recentTargetPathsToSave =
            [
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            ];

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave),
                () => _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave)
            );

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths[1]);

            Assert.IsTrue(_assetRepository.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(_dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
