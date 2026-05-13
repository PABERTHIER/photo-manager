using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetSyncAssetsEveryXMinutesTests
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

    private void ConfigureApplication(string assetsDirectory, bool syncAssetsEveryXMinutes)
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock.MockGetValue(UserConfigurationKeys.ASSETS_DIRECTORY, assetsDirectory);
        configurationRootMock.MockGetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES,
            syncAssetsEveryXMinutes.ToString());

        UserConfigurationService userConfigurationService = new(configurationRootMock);

        IPathProviderService pathProviderServiceMock = Substitute.For<IPathProviderService>();
        pathProviderServiceMock.ResolveDataDirectory().Returns(_databasePath);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        TestableAssetRepository testableAssetRepository = new(pathProviderServiceMock, imageProcessingService,
            imageMetadataService, userConfigurationService, new TestLogger<AssetRepository>(),
            new TestLogger<SqlitePersistenceContext>(), new TestLogger<OptimizedAssetRepository>());
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
    [TestCase(true)]
    [TestCase(false)]
    public void GetSyncAssetsEveryXMinutes_CorrectValue_ReturnsSyncAssetsEveryXMinutesValue(
        bool expectedSyncAssetsEveryXMinutes)
    {
        ConfigureApplication(_dataDirectory!, expectedSyncAssetsEveryXMinutes);

        try
        {
            bool syncAssetsEveryXMinutes = _application!.GetSyncAssetsEveryXMinutes();

            Assert.That(syncAssetsEveryXMinutes, Is.EqualTo(expectedSyncAssetsEveryXMinutes));
        }
        finally
        {
            Directory.Delete(_databaseDirectory!, true);
        }
    }
}
