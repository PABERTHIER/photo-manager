using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

[TestFixture]
public class AssetRepositoryVacuumTests
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
    public void Vacuum_EmptyDatabase_CompletesWithoutError()
    {
        Assert.DoesNotThrow(() => _assetRepository!.Vacuum());

        _testLogger!.AssertLogExceptions([], typeof(AssetRepository));
    }

    [Test]
    public void Vacuum_Disposed_ThrowsObjectDisposedExceptionAndLogsError()
    {
        _assetRepository!.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _assetRepository!.Vacuum());

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
