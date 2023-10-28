using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryGetRecentTargetPathsTests
{
    private string? dataDirectory;
    private const string backupEndPath = "DatabaseTests\\v1.0";
    private string? backupPath;

    private IAssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageService;
    private Mock<IConfigurationRoot>? _configurationRoot;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        backupPath = Path.Combine(dataDirectory, backupEndPath);

        _configurationRoot = new Mock<IConfigurationRoot>();
        _configurationRoot
            .MockGetValue("appsettings:CatalogBatchSize", "100")
            .MockGetValue("appsettings:CatalogCooldownMinutes", "5")
            .MockGetValue("appsettings:BackupsToKeep", "2")
            .MockGetValue("appsettings:ThumbnailsDictionaryEntriesToKeep", "5");

        _storageService = new Mock<IStorageService>();
        _storageService!.Setup(x => x.ResolveDataDirectory(It.IsAny<double>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new(new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new(_configurationRoot!.Object);
        _assetRepository = new AssetRepository(database, _storageService!.Object, userConfigurationService);
    }

    [Test]
    public void GetRecentTargetPaths_RecentTargetPaths_ReturnsRecentTargetPaths()
    {
        try
        {
            List<string> recentTargetPathsToSave = new()
            {
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            };

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);
            List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetRecentTargetPaths_NoRecentTargetPaths_ReturnsEmptyList()
    {
        try
        {
            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.IsEmpty(recentTargetPaths);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void GetRecentTargetPaths_ConcurrentAccess_RecentTargetPathsAreHandledSafely()
    {
        try
        {
            List<string> recentTargetPathsToSave = new()
            {
                "D:\\Workspace\\PhotoManager\\Toto",
                "D:\\Workspace\\PhotoManager\\Tutu"
            };

            _assetRepository!.SaveRecentTargetPaths(recentTargetPathsToSave);

            List<string> recentTargetPaths1 = new();
            List<string> recentTargetPaths2 = new();
            List<string> recentTargetPaths3 = new();

            // Simulate concurrent access
            Parallel.Invoke(
                () => recentTargetPaths1 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths2 = _assetRepository.GetRecentTargetPaths(),
                () => recentTargetPaths3 = _assetRepository.GetRecentTargetPaths()
            );

            Assert.AreEqual(2, recentTargetPaths1.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths1[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths1[1]);

            Assert.AreEqual(2, recentTargetPaths2.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths2[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths2[1]);

            Assert.AreEqual(2, recentTargetPaths3.Count);
            Assert.AreEqual(recentTargetPathsToSave[0], recentTargetPaths3[0]);
            Assert.AreEqual(recentTargetPathsToSave[1], recentTargetPaths3[1]);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
