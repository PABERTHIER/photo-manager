using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryHasChangesTests
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
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(backupPath);
    }

    [SetUp]
    public void Setup()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new AssetRepository(database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void HasChanges_Initialization_ReturnFalse()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void HasChanges_AfterChange_ReturnTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.IsFalse(_assetRepository!.HasChanges());

            string folderPath1 = Path.Combine(dataDirectory!, "TestFolder1");

            _assetRepository!.AddFolder(folderPath1);

            Assert.IsTrue(_assetRepository!.HasChanges());

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void HasChanges_ConcurrentAccess_ChangesAreHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = new();
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

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

            Assert.IsEmpty(assetsUpdatedEvents);
        }
        finally
        {
            Directory.Delete(Path.Combine(dataDirectory!, "DatabaseTests"), true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
