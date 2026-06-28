using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationGetEditableConfigurationTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;

    private PhotoManager.Application.Application? _application;
    private TestableAssetRepository? _testableAssetRepository;
    private UserConfigurationService? _userConfigurationService;
    private SqlitePersistenceContext? _sqlitePersistenceContext;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
    }

    [TearDown]
    public void TearDown()
    {
        _testableAssetRepository?.Dispose();
        _sqlitePersistenceContext?.Dispose();
        TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
    }

    private void ConfigureApplication(IConfigurationRoot configurationRootMock,
        EditableUserConfiguration? persistedConfiguration = null)
    {
        _sqlitePersistenceContext = PersistenceContextTestHelper.CreateInitializedContext(_databaseDirectory!);

        if (persistedConfiguration is not null)
        {
            UserConfigurationService seedingService = new(ConfigurationFactory.CreateMockConfiguration(),
                _sqlitePersistenceContext);
            seedingService.SaveEditableConfiguration(persistedConfiguration);
        }

        _userConfigurationService = new(configurationRootMock, _sqlitePersistenceContext);

        ImageProcessingService imageProcessingService = new(new TestLogger<ImageProcessingService>());
        FileOperationsService fileOperationsService = new(_userConfigurationService,
            new TestLogger<FileOperationsService>());
        ImageMetadataService imageMetadataService = new(fileOperationsService, new TestLogger<ImageMetadataService>());
        _testableAssetRepository = new(imageProcessingService, imageMetadataService, _userConfigurationService,
            _sqlitePersistenceContext, new TestLogger<AssetRepository>());
        AssetHashCalculatorService assetHashCalculatorService = new(_userConfigurationService,
            new TestLogger<AssetHashCalculatorService>());
        ThumbnailGenerator thumbnailGenerator = new(imageProcessingService);
        AssetCreationService assetCreationService = new(_testableAssetRepository, fileOperationsService,
            imageProcessingService, imageMetadataService, assetHashCalculatorService, thumbnailGenerator,
            _userConfigurationService, new TestLogger<AssetCreationService>());
        AssetsComparator assetsComparator = new();
        CatalogFolderPipeline catalogFolderPipeline = new(fileOperationsService, assetCreationService,
            _testableAssetRepository);
        CatalogAssetsService catalogAssetsService = new(_testableAssetRepository, fileOperationsService,
            imageMetadataService, assetCreationService, _userConfigurationService, assetsComparator,
            catalogFolderPipeline, new TestLogger<CatalogAssetsService>());
        MoveAssetsService moveAssetsService = new(_testableAssetRepository, fileOperationsService, assetCreationService,
            new TestLogger<MoveAssetsService>());
        SyncAssetsService syncAssetsService = new(_testableAssetRepository, fileOperationsService, assetsComparator,
            moveAssetsService);
        FindDuplicatedAssetsService findDuplicatedAssetsService = new(_testableAssetRepository, fileOperationsService,
            _userConfigurationService, new TestLogger<FindDuplicatedAssetsService>());
        AssetConversionService assetConversionService = new(fileOperationsService, imageProcessingService,
            new TestLogger<AssetConversionService>());
        _application = new(_testableAssetRepository, syncAssetsService, catalogAssetsService, moveAssetsService,
            findDuplicatedAssetsService, _userConfigurationService, fileOperationsService, imageProcessingService,
            assetConversionService);
    }

    [Test]
    public void GetEditableConfiguration_NoPersistedSettings_ReturnsConfigurationSeededFromJson()
    {
        ConfigureApplication(ConfigurationFactory.CreateMockConfiguration());

        EditableUserConfiguration result = _application!.GetEditableConfiguration();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.SameAs(_userConfigurationService!.GetEditableConfiguration()));

        Assert.That(result.AssetSettings.AnalyseVideos, Is.False);
        Assert.That(result.AssetSettings.CorruptedMessage, Is.EqualTo("The asset is corrupted"));
        Assert.That(result.AssetSettings.RotatedMessage, Is.EqualTo("The asset has been rotated"));
        Assert.That(result.AssetSettings.CatalogBatchSize, Is.EqualTo(100));
        Assert.That(result.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(5));
        Assert.That(result.AssetSettings.CorruptedImageOrientation, Is.EqualTo(10000));
        Assert.That(result.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
        Assert.That(result.AssetSettings.DetectThumbnails, Is.False);
        Assert.That(result.AssetSettings.SyncAssetsEveryXMinutes, Is.False);
        Assert.That(result.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(150));
        Assert.That(result.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(200));

        Assert.That(result.HashSettings.PHashThreshold, Is.EqualTo(10));
        Assert.That(result.HashSettings.UsingDHash, Is.False);
        Assert.That(result.HashSettings.UsingMD5Hash, Is.False);
        Assert.That(result.HashSettings.UsingPHash, Is.False);

        Assert.That(result.PathSettings.AssetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
        Assert.That(result.PathSettings.ExemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
        Assert.That(result.PathSettings.FirstFrameVideosFolderName, Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));

        Assert.That(result.PipelineSettings.ReadConcurrency, Is.EqualTo(3));
        Assert.That(result.PipelineSettings.ProcessConcurrency, Is.EqualTo(2));
        Assert.That(result.PipelineSettings.ReadBufferSize, Is.EqualTo(8));
        Assert.That(result.PipelineSettings.ProcessBufferSize, Is.EqualTo(16));
        Assert.That(result.PipelineSettings.PersistBufferSize, Is.EqualTo(24));
        Assert.That(result.PipelineSettings.PersistBatchSize, Is.EqualTo(20));

        Assert.That(result.StorageSettings.BackupsToKeep, Is.EqualTo(2));
        Assert.That(result.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));

        Assert.That(result.UiSettings.ThemeMode, Is.EqualTo("Light"));
    }

    [Test]
    public void GetEditableConfiguration_PersistedSettingsExist_ReturnsPersistedConfiguration()
    {
        EditableUserConfiguration persistedConfiguration = ConfigurationFactory.CreateEditableConfiguration();

        ConfigureApplication(ConfigurationFactory.CreateMockConfiguration(catalogBatchSize: "999", themeMode: "System"),
            persistedConfiguration);

        EditableUserConfiguration result = _application!.GetEditableConfiguration();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.SameAs(persistedConfiguration));
        Assert.That(result, Is.SameAs(_userConfigurationService!.GetEditableConfiguration()));

        Assert.That(result.AssetSettings.AnalyseVideos, Is.True);
        Assert.That(result.AssetSettings.CorruptedMessage, Is.EqualTo("Corrupted"));
        Assert.That(result.AssetSettings.RotatedMessage, Is.EqualTo("Rotated"));
        Assert.That(result.AssetSettings.CatalogBatchSize, Is.EqualTo(42));
        Assert.That(result.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(3));
        Assert.That(result.AssetSettings.CorruptedImageOrientation, Is.EqualTo(999));
        Assert.That(result.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
        Assert.That(result.AssetSettings.DetectThumbnails, Is.True);
        Assert.That(result.AssetSettings.SyncAssetsEveryXMinutes, Is.True);
        Assert.That(result.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(320));
        Assert.That(result.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(640));

        Assert.That(result.HashSettings.PHashThreshold, Is.EqualTo(6));
        Assert.That(result.HashSettings.UsingDHash, Is.True);
        Assert.That(result.HashSettings.UsingMD5Hash, Is.True);
        Assert.That(result.HashSettings.UsingPHash, Is.True);

        Assert.That(result.PathSettings.AssetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
        Assert.That(result.PathSettings.ExemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
        Assert.That(result.PathSettings.FirstFrameVideosFolderName, Is.EqualTo("Frames"));

        Assert.That(result.PipelineSettings.ReadConcurrency, Is.EqualTo(5));
        Assert.That(result.PipelineSettings.ProcessConcurrency, Is.EqualTo(1));
        Assert.That(result.PipelineSettings.ReadBufferSize, Is.EqualTo(10));
        Assert.That(result.PipelineSettings.ProcessBufferSize, Is.EqualTo(20));
        Assert.That(result.PipelineSettings.PersistBufferSize, Is.EqualTo(30));
        Assert.That(result.PipelineSettings.PersistBatchSize, Is.EqualTo(25));

        Assert.That(result.StorageSettings.BackupsToKeep, Is.EqualTo(4));
        Assert.That(result.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(12));

        Assert.That(result.UiSettings.ThemeMode, Is.EqualTo("Dark"));
    }

}
