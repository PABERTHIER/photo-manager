using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetCatalogCooldownMinutesTests
{
    private string? _dataDirectory;
    private string? _databaseDirectory;
    private string? _databasePath;

    private PhotoManager.Application.Application? _application;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_dataDirectory, Directories.DATABASE_TESTS);
        _databasePath = Path.Combine(_databaseDirectory, Constants.DATABASE_END_PATH);
    }

    private void ConfigureApplication(string assetsDirectory, ushort catalogCooldownMinutes)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES,
            catalogCooldownMinutes.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        SqliteConnectionFactory sqliteConnectionFactory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService sqliteBackupService = new(sqliteConnectionFactory);
        SqlitePersistenceContext sqlitePersistenceContext = new(
            sqliteConnectionFactory, sqliteBackupService, new TestLogger<SqlitePersistenceContext>());
        TestableAssetRepository testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        AssetCreationService assetCreationService = new(testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, userConfigurationService,
            new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogAssetsService catalogAssetsService = new(testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, userConfigurationService, assetsComparator,
            new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(testableAssetRepository, fileOperationsService,
            userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        _application = new(testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, userConfigurationService, fileOperationsService, imageProcessingService);
    }

    [Test]
    [TestCase((ushort)0)]
    [TestCase((ushort)2)]
    [TestCase((ushort)15)]
    public void GetCatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue(
        ushort expectedCatalogCooldownMinutes)
    {
        ConfigureApplication(_dataDirectory!, expectedCatalogCooldownMinutes);

        try
        {
            ushort catalogCooldownMinutes = _application!.GetCatalogCooldownMinutes();

            Assert.That(catalogCooldownMinutes, Is.EqualTo(expectedCatalogCooldownMinutes));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
