using Microsoft.Extensions.Configuration;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryHasChangesTests
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
    public void HasChanges_Initialization_ReturnFalse()
    {
        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void HasChanges_AfterChange_ReturnTrue()
    {
        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());

            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            Assert.IsTrue(_assetRepository!.HasChanges());
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }

    [Test]
    public void HasChanges_ConcurrentAccess_ChangesAreHandledSafely()
    {
        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());

            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            bool hasChanges1 = false;
            bool hasChanges2 = false;
            bool hasChanges3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => hasChanges1 = _assetRepository!.HasChanges(),
                () => hasChanges2 = _assetRepository!.HasChanges(),
                () => hasChanges3 = _assetRepository!.HasChanges()
            );

            Assert.IsTrue(hasChanges1);
            Assert.IsTrue(hasChanges2);
            Assert.IsTrue(hasChanges3);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
        }
    }
}
