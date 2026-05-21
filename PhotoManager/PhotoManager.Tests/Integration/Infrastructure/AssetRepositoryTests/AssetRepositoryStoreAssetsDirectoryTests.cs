using Directories = PhotoManager.Tests.Integration.Constants.Directories;
using Reactive = System.Reactive;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryStoreAssetsDirectoryTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private AssetRepository? _assetRepository;
    private TestLogger<AssetRepository>? _testLogger;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDatabaseDirectory().Returns(_databaseDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        _testLogger = new();
        _assetRepository = CreateNewAssetRepository();
    }

    [TearDown]
    public void TearDown()
    {
        _assetRepository?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        _testLogger!.LoggingAssertTearDown();
    }

    [Test]
    public void StoreAssetsDirectory_ValidPath_StoredAndRetrievable()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string path = @"D:\Photos\2024";

            _assetRepository!.StoreAssetsDirectory(path);
            string? result = _assetRepository.GetStoredAssetsDirectory();

            Assert.That(result, Is.EqualTo(path));
            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void StoreAssetsDirectory_CalledTwice_OverwritesPreviousValue()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string firstPath = @"D:\Photos\2024";
            const string secondPath = @"E:\Images\Holiday";

            _assetRepository!.StoreAssetsDirectory(firstPath);
            _assetRepository!.StoreAssetsDirectory(secondPath);
            string? result = _assetRepository.GetStoredAssetsDirectory();

            Assert.That(result, Is.EqualTo(secondPath));
            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void StoreAssetsDirectory_PersistsAcrossRepositoryInstances()
    {
        List<Reactive.Unit> assetsUpdatedEvents = [];
        IDisposable assetsUpdatedSubscription = _assetRepository!.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

        try
        {
            const string path = @"D:\Photos\2024";

            _assetRepository!.StoreAssetsDirectory(path);

            // Simulate an app restart by disposing the repository and creating a new one
            _assetRepository!.Dispose();
            assetsUpdatedSubscription.Dispose();

            _assetRepository = CreateNewAssetRepository();
            assetsUpdatedEvents = [];
            assetsUpdatedSubscription = _assetRepository.AssetsUpdated.Subscribe(assetsUpdatedEvents.Add);

            string? result = _assetRepository.GetStoredAssetsDirectory();

            Assert.That(result, Is.EqualTo(path));
            Assert.That(assetsUpdatedEvents, Is.Empty);

            _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
        }
        finally
        {
            assetsUpdatedSubscription.Dispose();
        }
    }

    [Test]
    public void StoreAssetsDirectory_Disposed_ThrowsObjectDisposedExceptionAndLogsError()
    {
        _assetRepository!.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _assetRepository!.StoreAssetsDirectory(@"D:\Photos\2024"));

        _testLogger!.AssertLogExceptions([new ObjectDisposedException(nameof(AssetRepository))],
            typeof(AssetRepository));
    }

    private AssetRepository CreateNewAssetRepository()
    {
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(sqliteConnectionFactory, sqliteBackupService,
            new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());

        return new(_pathProviderServiceMock!, imageProcessingService, imageMetadataService, userConfigurationService,
            sqlitePersistenceContext, _testLogger!);
    }
}
