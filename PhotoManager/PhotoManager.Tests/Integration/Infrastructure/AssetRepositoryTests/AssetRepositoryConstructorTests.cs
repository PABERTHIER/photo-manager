using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class AssetRepositoryConstructorTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private IPathProviderService? _pathProviderServiceMock;
    private IConfigurationRoot? _configurationRootMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);

        _configurationRootMock = Substitute.For<IConfigurationRoot>();
        _configurationRootMock.GetDefaultMockConfig();

        _pathProviderServiceMock = Substitute.For<IPathProviderService>();
        _pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);
    }

    [TearDown]
    public void TearDown()
    {
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    // TODO: Enable it when the migration is done + fix the content
    [Test]
    [Ignore("Enable this test when using OptimizedAssetRepository implementation instead")]
    public void Constructor_ReadCatalogThrowsException_LogsItAndThrowsException()
    {
        // Create a corrupted .db file with invalid GUID that will cause ReadCatalog to fail

        TestLogger<AssetRepository> testLogger = new();
        SqliteConnectionFactory sqliteConnectionFactory = new();
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        UserConfigurationService userConfigurationService = new(_configurationRootMock!);
        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());

        using (Assert.EnterMultipleScope())
        {
            ArgumentException? exception = Assert.Throws<ArgumentException>(() =>
                new AssetRepository(_pathProviderServiceMock!, imageProcessingService, imageMetadataService,
                    userConfigurationService, sqlitePersistenceContext, testLogger));

            Assert.That(exception?.Message, Does.Contain("Error while trying to read data table"));

            Exception expectedException = new(exception?.Message);
            testLogger.AssertLogExceptions([expectedException], typeof(AssetRepository));
        }
    }
}
