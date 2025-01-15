using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryBackupExistsTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;
    private const string DATABASE_END_PATH = "v1.0";

    private AssetRepository? _assetRepository;
    private Mock<IStorageService>? _storageServiceMock;
    private Mock<IConfigurationRoot>? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        _databaseDirectory = Path.Combine(_dataDirectory, "DatabaseTests");
        _databasePath = Path.Combine(_databaseDirectory, DATABASE_END_PATH);

        _configurationRootMock = new Mock<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _storageServiceMock = new Mock<IStorageService>();
        _storageServiceMock!.Setup(x => x.ResolveDataDirectory(It.IsAny<string>())).Returns(_databasePath);
    }

    [SetUp]
    public void SetUp()
    {
        PhotoManager.Infrastructure.Database.Database database = new (new ObjectListStorage(), new BlobStorage(), new BackupStorage());
        UserConfigurationService userConfigurationService = new (_configurationRootMock!.Object);
        _assetRepository = new (database, _storageServiceMock!.Object, userConfigurationService);
    }

    [Test]
    public void BackupExists_BackupExists_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            _assetRepository!.WriteBackup();

            Assert.That(_assetRepository!.BackupExists(), Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void BackupExists_BackupDoesNotExist_ReturnsTrue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            Assert.That(_assetRepository!.BackupExists(), Is.False);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void BackupExists_ConcurrentAccess_CheckExistenceIsHandledSafely()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            _assetRepository!.WriteBackup();

            bool backupExists1 = false;
            bool backupExists2 = false;
            bool backupExists3 = false;

            // Simulate concurrent access
            Parallel.Invoke(
                () => backupExists1 = _assetRepository!.BackupExists(),
                () => backupExists2 = _assetRepository!.BackupExists(),
                () => backupExists3 = _assetRepository!.BackupExists()
            );

            Assert.That(backupExists1, Is.True);
            Assert.That(backupExists2, Is.True);
            Assert.That(backupExists3, Is.True);

            Assert.That(assetsUpdatedEvents, Is.Empty);
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
            assetsUpdatedSubscription.Dispose();
        }
    }
}
