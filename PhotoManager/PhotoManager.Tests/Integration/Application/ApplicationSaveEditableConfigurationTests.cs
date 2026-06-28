using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Application;

[TestFixture]
public class ApplicationSaveEditableConfigurationTests
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

    private void ConfigureApplication(IConfigurationRoot configurationRootMock)
    {
        _sqlitePersistenceContext = PersistenceContextTestHelper.CreateInitializedContext(_databaseDirectory!);
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
    public void SaveEditableConfiguration_ValidConfiguration_PersistsConfigurationAndUpdatesEditableState()
    {
        ConfigureApplication(ConfigurationFactory.CreateMockConfiguration());
        EditableUserConfiguration configuration = ConfigurationFactory.CreateEditableConfiguration();

        // The application starts on the JSON-seeded defaults, both in memory and in the SQLite Configuration table.
        EditableUserConfiguration applicationEditableConfiguration = _application!.GetEditableConfiguration();

        Assert.That(applicationEditableConfiguration.AssetSettings.AnalyseVideos, Is.False);
        Assert.That(applicationEditableConfiguration.AssetSettings.CorruptedMessage,
            Is.EqualTo("The asset is corrupted"));
        Assert.That(applicationEditableConfiguration.AssetSettings.RotatedMessage,
            Is.EqualTo("The asset has been rotated"));
        Assert.That(applicationEditableConfiguration.AssetSettings.CatalogBatchSize, Is.EqualTo(100));
        Assert.That(applicationEditableConfiguration.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(5));
        Assert.That(applicationEditableConfiguration.AssetSettings.CorruptedImageOrientation, Is.EqualTo(10000));
        Assert.That(applicationEditableConfiguration.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
        Assert.That(applicationEditableConfiguration.AssetSettings.DetectThumbnails, Is.False);
        Assert.That(applicationEditableConfiguration.AssetSettings.SyncAssetsEveryXMinutes, Is.False);
        Assert.That(applicationEditableConfiguration.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(150));
        Assert.That(applicationEditableConfiguration.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(200));

        Assert.That(applicationEditableConfiguration.HashSettings.PHashThreshold, Is.EqualTo(10));
        Assert.That(applicationEditableConfiguration.HashSettings.UsingDHash, Is.False);
        Assert.That(applicationEditableConfiguration.HashSettings.UsingMD5Hash, Is.False);
        Assert.That(applicationEditableConfiguration.HashSettings.UsingPHash, Is.False);

        Assert.That(applicationEditableConfiguration.PathSettings.AssetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
        Assert.That(applicationEditableConfiguration.PathSettings.ExemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
        Assert.That(applicationEditableConfiguration.PathSettings.FirstFrameVideosFolderName,
            Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));

        Assert.That(applicationEditableConfiguration.PipelineSettings.ReadConcurrency, Is.EqualTo(3));
        Assert.That(applicationEditableConfiguration.PipelineSettings.ProcessConcurrency, Is.EqualTo(2));
        Assert.That(applicationEditableConfiguration.PipelineSettings.ReadBufferSize, Is.EqualTo(8));
        Assert.That(applicationEditableConfiguration.PipelineSettings.ProcessBufferSize, Is.EqualTo(16));
        Assert.That(applicationEditableConfiguration.PipelineSettings.PersistBufferSize, Is.EqualTo(24));
        Assert.That(applicationEditableConfiguration.PipelineSettings.PersistBatchSize, Is.EqualTo(20));

        Assert.That(applicationEditableConfiguration.StorageSettings.BackupsToKeep, Is.EqualTo(2));
        Assert.That(applicationEditableConfiguration.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));

        Assert.That(applicationEditableConfiguration.UiSettings.ThemeMode, Is.EqualTo("Light"));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
            Is.EqualTo(bool.FalseString));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
            Is.EqualTo("The asset is corrupted"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
            Is.EqualTo("The asset has been rotated"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
            Is.EqualTo("100"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
            Is.EqualTo("5"));
        Assert.That(
            _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
            Is.EqualTo("10000"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
            Is.EqualTo("1"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
            Is.EqualTo(bool.FalseString));
        Assert.That(
            _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
            Is.EqualTo(bool.FalseString));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
            Is.EqualTo("150"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
            Is.EqualTo("200"));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
            Is.EqualTo("10"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
            Is.EqualTo(bool.FalseString));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
            Is.EqualTo(bool.FalseString));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
            Is.EqualTo(bool.FalseString));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
        Assert.That(
            _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
            Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.READ_CONCURRENCY),
            Is.EqualTo("3"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.PROCESS_CONCURRENCY),
            Is.EqualTo("2"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.READ_BUFFER_SIZE),
            Is.EqualTo("8"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.PROCESS_BUFFER_SIZE),
            Is.EqualTo("16"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.PERSIST_BUFFER_SIZE),
            Is.EqualTo("24"));
        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.PERSIST_BATCH_SIZE),
            Is.EqualTo("20"));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
            Is.EqualTo("2"));
        Assert.That(
            _sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
            Is.EqualTo("5"));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
            Is.EqualTo("Light"));

        _application.SaveEditableConfiguration(configuration);

        // The in-memory editable configuration is replaced by the very instance that was saved.
        Assert.That(_application.GetEditableConfiguration(), Is.SameAs(configuration));
        Assert.That(_userConfigurationService!.GetEditableConfiguration(), Is.SameAs(configuration));

        // The whole editable configuration is now persisted in SQLite (the Project section is never stored).
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
            Is.EqualTo("Corrupted"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
            Is.EqualTo("Rotated"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
            Is.EqualTo("42"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
            Is.EqualTo("3"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
            Is.EqualTo("999"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
            Is.EqualTo("1"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
            Is.EqualTo("320"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
            Is.EqualTo("640"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
            Is.EqualTo("6"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
            Is.EqualTo(bool.TrueString));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
        Assert.That(
            _sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
            Is.EqualTo("Frames"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.READ_CONCURRENCY),
            Is.EqualTo("5"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROCESS_CONCURRENCY),
            Is.EqualTo("1"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.READ_BUFFER_SIZE),
            Is.EqualTo("10"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROCESS_BUFFER_SIZE),
            Is.EqualTo("20"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PERSIST_BUFFER_SIZE),
            Is.EqualTo("30"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PERSIST_BATCH_SIZE),
            Is.EqualTo("25"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
            Is.EqualTo("4"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(
                UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
            Is.EqualTo("12"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
            Is.EqualTo("Dark"));

        // The Project section is intentionally never persisted: it always comes from appsettings.json.
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_NAME), Is.Null);
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_OWNER), Is.Null);
    }

    [Test]
    public void SaveEditableConfiguration_ValidConfigurationAndReadBackByANewService_PersistsConfiguration()
    {
        ConfigureApplication(ConfigurationFactory.CreateMockConfiguration());
        EditableUserConfiguration configuration = ConfigurationFactory.CreateEditableConfiguration();

        _application!.SaveEditableConfiguration(configuration);

        // A brand-new service reading the same database observes the saved values: the save is durable and
        // re-readable beyond the live instance, rebuilt by value rather than handing back the saved instance.
        UserConfigurationService reloadedService = new(ConfigurationFactory.CreateMockConfiguration(),
            _sqlitePersistenceContext!);
        EditableUserConfiguration reloadedConfiguration = reloadedService.GetEditableConfiguration();

        Assert.That(reloadedConfiguration, Is.Not.SameAs(configuration));

        Assert.That(reloadedConfiguration.AssetSettings.AnalyseVideos, Is.True);
        Assert.That(reloadedConfiguration.AssetSettings.CorruptedMessage, Is.EqualTo("Corrupted"));
        Assert.That(reloadedConfiguration.AssetSettings.RotatedMessage, Is.EqualTo("Rotated"));
        Assert.That(reloadedConfiguration.AssetSettings.CatalogBatchSize, Is.EqualTo(42));
        Assert.That(reloadedConfiguration.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(3));
        Assert.That(reloadedConfiguration.AssetSettings.CorruptedImageOrientation, Is.EqualTo(999));
        Assert.That(reloadedConfiguration.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
        Assert.That(reloadedConfiguration.AssetSettings.DetectThumbnails, Is.True);
        Assert.That(reloadedConfiguration.AssetSettings.SyncAssetsEveryXMinutes, Is.True);
        Assert.That(reloadedConfiguration.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(320));
        Assert.That(reloadedConfiguration.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(640));

        Assert.That(reloadedConfiguration.HashSettings.PHashThreshold, Is.EqualTo(6));
        Assert.That(reloadedConfiguration.HashSettings.UsingDHash, Is.True);
        Assert.That(reloadedConfiguration.HashSettings.UsingMD5Hash, Is.True);
        Assert.That(reloadedConfiguration.HashSettings.UsingPHash, Is.True);

        Assert.That(reloadedConfiguration.PathSettings.AssetsDirectory,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
        Assert.That(reloadedConfiguration.PathSettings.ExemptedFolderPath,
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
        Assert.That(reloadedConfiguration.PathSettings.FirstFrameVideosFolderName, Is.EqualTo("Frames"));

        Assert.That(reloadedConfiguration.PipelineSettings.ReadConcurrency, Is.EqualTo(5));
        Assert.That(reloadedConfiguration.PipelineSettings.ProcessConcurrency, Is.EqualTo(1));
        Assert.That(reloadedConfiguration.PipelineSettings.ReadBufferSize, Is.EqualTo(10));
        Assert.That(reloadedConfiguration.PipelineSettings.ProcessBufferSize, Is.EqualTo(20));
        Assert.That(reloadedConfiguration.PipelineSettings.PersistBufferSize, Is.EqualTo(30));
        Assert.That(reloadedConfiguration.PipelineSettings.PersistBatchSize, Is.EqualTo(25));

        Assert.That(reloadedConfiguration.StorageSettings.BackupsToKeep, Is.EqualTo(4));
        Assert.That(reloadedConfiguration.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(12));

        Assert.That(reloadedConfiguration.UiSettings.ThemeMode, Is.EqualTo("Dark"));

        Assert.That(_sqlitePersistenceContext!.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
            Is.EqualTo("Corrupted"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
            Is.EqualTo("Rotated"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
            Is.EqualTo("42"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
            Is.EqualTo("3"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
            Is.EqualTo("999"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
            Is.EqualTo("1"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
            Is.EqualTo("320"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
            Is.EqualTo("640"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
            Is.EqualTo("6"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
            Is.EqualTo(bool.TrueString));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
            Is.EqualTo(bool.TrueString));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
            Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
        Assert.That(
            _sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
            Is.EqualTo("Frames"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.READ_CONCURRENCY),
            Is.EqualTo("5"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROCESS_CONCURRENCY),
            Is.EqualTo("1"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.READ_BUFFER_SIZE),
            Is.EqualTo("10"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROCESS_BUFFER_SIZE),
            Is.EqualTo("20"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PERSIST_BUFFER_SIZE),
            Is.EqualTo("30"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PERSIST_BATCH_SIZE),
            Is.EqualTo("25"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
            Is.EqualTo("4"));
        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(
                UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
            Is.EqualTo("12"));

        Assert.That(_sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
            Is.EqualTo("Dark"));
    }

}
