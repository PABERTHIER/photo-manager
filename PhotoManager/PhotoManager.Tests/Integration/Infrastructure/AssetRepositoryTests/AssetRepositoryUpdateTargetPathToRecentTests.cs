using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryUpdateTargetPathToRecentTests
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
    public void UpdateTargetPathToRecent_NewFolderAndExisting_UpdateRecentTargetPathsAndSave()
    {
        try
        {
            Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Path = "D:\\Workspace\\PhotoManager\\Titi" };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            _assetRepository!.UpdateTargetPathToRecent(folder3);
            _assetRepository!.UpdateTargetPathToRecent(folder2);

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(3, recentTargetPaths.Count);
            Assert.AreEqual(folder1.Path, recentTargetPaths[2]);
            Assert.AreEqual(folder2.Path, recentTargetPaths[0]);
            Assert.AreEqual(folder3.Path, recentTargetPaths[1]);

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_MaxCountHasBeenReached_UpdateRecentPathsAndSave()
    {
        try
        {
            for (int i = 0; i < 30; i++)
            {
                _assetRepository!.UpdateTargetPathToRecent(new Folder { Path = $"D:\\Workspace\\PhotoManager\\Folder{i}" });
            }

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(20, recentTargetPaths.Count);
            Assert.AreEqual("D:\\Workspace\\PhotoManager\\Folder29", recentTargetPaths[0]);
            Assert.AreEqual("D:\\Workspace\\PhotoManager\\Folder28", recentTargetPaths[1]);
            Assert.AreEqual("D:\\Workspace\\PhotoManager\\Folder10", recentTargetPaths[19]);

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_PathIsNull_UpdateRecentPathsAndSave()
    {
        try
        {
            string? nullPath = null;

            Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Path = nullPath! };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            _assetRepository!.UpdateTargetPathToRecent(folder3);

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(3, recentTargetPaths.Count);
            Assert.AreEqual(folder1.Path, recentTargetPaths[2]);
            Assert.AreEqual(folder2.Path, recentTargetPaths[1]);
            Assert.AreEqual(folder3.Path, recentTargetPaths[0]);

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_PathIsEmpty_UpdateRecentPathsAndSave()
    {
        try
        {
            string emptyPath = string.Empty;

            Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Path = emptyPath };

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            _assetRepository!.UpdateTargetPathToRecent(folder3);

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(3, recentTargetPaths.Count);
            Assert.AreEqual(folder1.Path, recentTargetPaths[2]);
            Assert.AreEqual(folder2.Path, recentTargetPaths[1]);
            Assert.AreEqual(folder3.Path, recentTargetPaths[0]);

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_FolderIsNull_ThrowsNullReferenceException()
    {
        try
        {
            Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder? folder3 = null;

            _assetRepository!.UpdateTargetPathToRecent(folder1);
            _assetRepository!.UpdateTargetPathToRecent(folder2);
            Assert.Throws<NullReferenceException>(() => _assetRepository!.UpdateTargetPathToRecent(folder3!));

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(2, recentTargetPaths.Count);
            Assert.AreEqual(folder1.Path, recentTargetPaths[1]);
            Assert.AreEqual(folder2.Path, recentTargetPaths[0]);

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void UpdateTargetPathToRecent_ConcurrentAccess_RecentTargetPathsAreHandledSafely()
    {
        try
        {
            Folder folder1 = new() { Path = "D:\\Workspace\\PhotoManager\\Toto" };
            Folder folder2 = new() { Path = "D:\\Workspace\\PhotoManager\\Tutu" };
            Folder folder3 = new() { Path = "D:\\Workspace\\PhotoManager\\Titi" };

            // Simulate concurrent access
            Parallel.Invoke(
                () => _assetRepository!.UpdateTargetPathToRecent(folder1),
                () => _assetRepository!.UpdateTargetPathToRecent(folder2),
                () => _assetRepository!.UpdateTargetPathToRecent(folder2),
                () => _assetRepository!.UpdateTargetPathToRecent(folder3)
            );

            List<string> recentTargetPaths = _assetRepository!.GetRecentTargetPaths();

            Assert.AreEqual(3, recentTargetPaths.Count);
            Assert.IsTrue(recentTargetPaths.Any(x => x == folder1.Path));
            Assert.IsTrue(recentTargetPaths.Any(x => x == folder2.Path));
            Assert.IsTrue(recentTargetPaths.Any(x => x == folder3.Path));

            Assert.IsTrue(_assetRepository.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
