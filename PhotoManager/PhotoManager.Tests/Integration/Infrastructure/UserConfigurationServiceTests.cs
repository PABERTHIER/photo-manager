using PhotoManager.UI;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private string? _assetsDirectory;
    private string? _databaseDirectory;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _assetsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        _databaseDirectory = Path.Combine(_assetsDirectory, Directories.DATABASE_TESTS);
        string configFilePath = Path.Combine(_assetsDirectory, "appsettings.json");

        IConfigurationBuilder builder =
            new ConfigurationBuilder().AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        _userConfigurationService = configuration.CreateUserConfigurationService();
    }

    [Test]
    public void Constructor_NoPersistedSettings_SeedsConfigurationTableFromJson()
    {
        TestLogger<SqlitePersistenceContext> testLogger = new();
        SqlitePersistenceContext sqlitePersistenceContext = CreatePersistenceContext(testLogger);
        sqlitePersistenceContext.Initialize(_databaseDirectory!);

        try
        {
            IConfigurationRoot configurationRootMock = CreateConfiguration(catalogBatchSize: "321", themeMode: "Dark");

            UserConfigurationService userConfigurationService =
                new(configurationRootMock, sqlitePersistenceContext);

            Assert.That(userConfigurationService.AssetSettings.AnalyseVideos, Is.False);
            Assert.That(userConfigurationService.AssetSettings.CorruptedMessage, Is.EqualTo("The asset is corrupted"));
            Assert.That(userConfigurationService.AssetSettings.RotatedMessage,
                Is.EqualTo("The asset has been rotated"));
            Assert.That(userConfigurationService.AssetSettings.CatalogBatchSize, Is.EqualTo(321));
            Assert.That(userConfigurationService.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(5));
            Assert.That(userConfigurationService.AssetSettings.CorruptedImageOrientation, Is.EqualTo(10000));
            Assert.That(userConfigurationService.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
            Assert.That(userConfigurationService.AssetSettings.DetectThumbnails, Is.False);
            Assert.That(userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes, Is.False);
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(150));
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(200));

            Assert.That(userConfigurationService.HashSettings.PHashThreshold, Is.EqualTo(10));
            Assert.That(userConfigurationService.HashSettings.UsingDHash, Is.False);
            Assert.That(userConfigurationService.HashSettings.UsingMD5Hash, Is.False);
            Assert.That(userConfigurationService.HashSettings.UsingPHash, Is.False);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(
                PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
            Assert.That(userConfigurationService.PathSettings.FirstFrameVideosPath,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath(
                    $"E:\\Workspace\\PhotoManager\\TestAssets\\{Directories.OUTPUT_VIDEO_FIRST_FRAME}")));

            Assert.That(userConfigurationService.ProjectSettings.Name, Is.EqualTo("PhotoManager"));
            Assert.That(userConfigurationService.ProjectSettings.Owner, Is.EqualTo("Toto"));

            Assert.That(userConfigurationService.StorageSettings.BackupsToKeep, Is.EqualTo(2));
            Assert.That(userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));

            Assert.That(userConfigurationService.UiSettings.ThemeMode, Is.EqualTo("Dark"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
                Is.EqualTo("The asset is corrupted"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
                Is.EqualTo("The asset has been rotated"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
                Is.EqualTo("321"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
                Is.EqualTo("5"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
                Is.EqualTo("10000"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
                Is.EqualTo("1"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
                Is.EqualTo(bool.FalseString));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
                Is.EqualTo("150"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
                Is.EqualTo("200"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
                Is.EqualTo("10"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
                Is.EqualTo(bool.FalseString));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
                Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));

            // The Project section is intentionally never persisted: it stays in appsettings.json.
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_NAME), Is.Null);
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_OWNER), Is.Null);

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
                Is.EqualTo("2"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(
                    UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
                Is.EqualTo("5"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
                Is.EqualTo("Dark"));

            testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
        }
        finally
        {
            sqlitePersistenceContext.Dispose();
            TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        }
    }

    [Test]
    public void Constructor_PersistedSettingsExist_UsesDatabaseValuesAndKeepsProjectFromJson()
    {
        TestLogger<SqlitePersistenceContext> testLogger = new();
        SqlitePersistenceContext sqlitePersistenceContext = CreatePersistenceContext(testLogger);
        sqlitePersistenceContext.Initialize(_databaseDirectory!);

        try
        {
            IConfigurationRoot initialConfiguration = CreateConfiguration(catalogBatchSize: "321", themeMode: "Light");
            UserConfigurationService initialUserConfigurationService =
                new(initialConfiguration, sqlitePersistenceContext);
            EditableUserConfiguration persistedConfiguration = CreateEditableConfiguration();

            initialUserConfigurationService.SaveEditableConfiguration(persistedConfiguration);

            IConfigurationRoot changedJsonConfiguration = CreateConfiguration(catalogBatchSize: "999",
                projectName: "JsonProject", projectOwner: "JsonOwner", themeMode: "System");

            UserConfigurationService userConfigurationService =
                new(changedJsonConfiguration, sqlitePersistenceContext);

            Assert.That(userConfigurationService.AssetSettings.AnalyseVideos, Is.True);
            Assert.That(userConfigurationService.AssetSettings.CorruptedMessage, Is.EqualTo("Corrupted"));
            Assert.That(userConfigurationService.AssetSettings.RotatedMessage, Is.EqualTo("Rotated"));
            Assert.That(userConfigurationService.AssetSettings.CatalogBatchSize, Is.EqualTo(42));
            Assert.That(userConfigurationService.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(3));
            Assert.That(userConfigurationService.AssetSettings.CorruptedImageOrientation, Is.EqualTo(999));
            Assert.That(userConfigurationService.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
            Assert.That(userConfigurationService.AssetSettings.DetectThumbnails, Is.True);
            Assert.That(userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes, Is.True);
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(320));
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(640));

            Assert.That(userConfigurationService.HashSettings.PHashThreshold, Is.EqualTo(6));
            Assert.That(userConfigurationService.HashSettings.UsingDHash, Is.True);
            Assert.That(userConfigurationService.HashSettings.UsingMD5Hash, Is.True);
            Assert.That(userConfigurationService.HashSettings.UsingPHash, Is.True);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
            Assert.That(userConfigurationService.PathSettings.FirstFrameVideosPath,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Frames")));

            Assert.That(userConfigurationService.ProjectSettings.Name, Is.EqualTo("JsonProject"));
            Assert.That(userConfigurationService.ProjectSettings.Owner, Is.EqualTo("JsonOwner"));

            Assert.That(userConfigurationService.StorageSettings.BackupsToKeep, Is.EqualTo(4));
            Assert.That(userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(12));

            Assert.That(userConfigurationService.UiSettings.ThemeMode, Is.EqualTo("Dark"));

            testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
        }
        finally
        {
            sqlitePersistenceContext.Dispose();
            TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        }
    }

    [Test]
    public void Constructor_PersistedSettingsPartiallyStored_ThrowsForMissingRequiredKey()
    {
        TestLogger<SqlitePersistenceContext> testLogger = new();
        SqlitePersistenceContext sqlitePersistenceContext = CreatePersistenceContext(testLogger);
        sqlitePersistenceContext.Initialize(_databaseDirectory!);

        try
        {
            IConfigurationRoot configurationRootMock = CreateConfiguration();
            // A populated table means "persisted settings exist", so the service reads from it; a missing required
            // key is therefore an error. The atomic SetValues used on seed/save makes this state unreachable in
            // practice, but the read path must still fail fast if the table is ever partially populated.
            sqlitePersistenceContext.Configuration.SetValue(UserConfigurationKeys.THEME_MODE, "Dark");

            InvalidOperationException? exception = null;
            try
            {
                _ = new UserConfigurationService(configurationRootMock, sqlitePersistenceContext);
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }

            Assert.That(exception?.Message,
                Is.EqualTo($"Persisted configuration key '{UserConfigurationKeys.ANALYSE_VIDEOS}' is missing."));

            testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
        }
        finally
        {
            sqlitePersistenceContext.Dispose();
            TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        }
    }

    [Test]
    public void GetAboutInformation_WithValidAssembly_ReturnsAboutInformation()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(App).Assembly);

        Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(string.IsNullOrWhiteSpace(aboutInformation.Version), Is.False);
        Assert.That(aboutInformation.Version, Does.StartWith("v"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void GetAboutInformation_WithDifferentAssembly_ReturnsDifferentProduct()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(int).Assembly);

        Assert.That(aboutInformation.Product, Is.Not.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Product, Is.EqualTo("Microsoft® .NET"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void GetAboutInformation_WithAssemblyWithoutProductAttribute_ReturnsDefaultProduct()
    {
        AssemblyName assemblyName = new("TestAssemblyWithoutProductAttribute");
        AssemblyBuilder assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(assemblyBuilder);

        Assert.That(aboutInformation.Product, Is.EqualTo("PhotoManager"));
        Assert.That(aboutInformation.Author, Is.EqualTo("Toto"));
        Assert.That(aboutInformation.Version, Is.EqualTo(Constants.VERSION));
    }

    [Test]
    public void AnalyseVideos_CorrectValue_ReturnsAnalyseVideosValue()
    {
        bool analyseVideos = _userConfigurationService!.AssetSettings.AnalyseVideos;

        Assert.That(analyseVideos, Is.False);
    }

    [Test]
    public void CorruptedMessage_CorrectValue_ReturnsCorruptedMessageValue()
    {
        string corruptedMessage = _userConfigurationService!.AssetSettings.CorruptedMessage;

        Assert.That(corruptedMessage, Is.Not.Null);
        Assert.That(corruptedMessage, Is.EqualTo("The asset is corrupted"));
    }

    [Test]
    public void RotatedMessage_CorrectValue_ReturnsRotatedMessageValue()
    {
        string rotatedMessage = _userConfigurationService!.AssetSettings.RotatedMessage;

        Assert.That(rotatedMessage, Is.Not.Null);
        Assert.That(rotatedMessage, Is.EqualTo("The asset has been rotated"));
    }

    [Test]
    public void CatalogBatchSize_CorrectValue_ReturnsCatalogBatchSizeValue()
    {
        int catalogBatchSize = _userConfigurationService!.AssetSettings.CatalogBatchSize;

        Assert.That(catalogBatchSize, Is.EqualTo(100000));
    }

    [Test]
    public void CatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue()
    {
        ushort catalogCooldownMinutes = _userConfigurationService!.AssetSettings.CatalogCooldownMinutes;

        Assert.That(catalogCooldownMinutes, Is.EqualTo(2));
    }

    [Test]
    public void CorruptedImageOrientation_CorrectValue_ReturnsCorruptedImageOrientationValue()
    {
        ushort corruptedImageOrientation = _userConfigurationService!.AssetSettings.CorruptedImageOrientation;

        Assert.That(corruptedImageOrientation, Is.EqualTo(10000));
    }

    [Test]
    public void DefaultExifOrientation_CorrectValue_ReturnsDefaultExifOrientationValue()
    {
        ushort defaultExifOrientation = _userConfigurationService!.AssetSettings.DefaultExifOrientation;

        Assert.That(defaultExifOrientation, Is.EqualTo(1));
    }

    [Test]
    public void DetectThumbnails_CorrectValue_ReturnsDetectThumbnailsValue()
    {
        bool detectThumbnails = _userConfigurationService!.AssetSettings.DetectThumbnails;

        Assert.That(detectThumbnails, Is.False);
    }

    [Test]
    public void SyncAssetsEveryXMinutes_CorrectValue_ReturnsSyncAssetsEveryXMinutesValue()
    {
        bool syncAssetsEveryXMinutes = _userConfigurationService!.AssetSettings.SyncAssetsEveryXMinutes;

        Assert.That(syncAssetsEveryXMinutes, Is.False);
    }

    [Test]
    public void ThumbnailMaxHeight_CorrectValue_ReturnsThumbnailMaxHeightValue()
    {
        int thumbnailMaxHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        Assert.That(thumbnailMaxHeight, Is.EqualTo(150));
    }

    [Test]
    public void ThumbnailMaxWidth_CorrectValue_ReturnsThumbnailMaxWidthValue()
    {
        int thumbnailMaxWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;

        Assert.That(thumbnailMaxWidth, Is.EqualTo(200));
    }

    [Test]
    public void PHashThreshold_CorrectValue_ReturnsPHashThresholdValue()
    {
        ushort pHashThreshold = _userConfigurationService!.HashSettings.PHashThreshold;

        Assert.That(pHashThreshold, Is.EqualTo(10));
    }

    [Test]
    public void UsingDHash_CorrectValue_ReturnsUsingDHashValue()
    {
        bool usingDHash = _userConfigurationService!.HashSettings.UsingDHash;

        Assert.That(usingDHash, Is.False);
    }

    [Test]
    public void UsingMD5Hash_CorrectValue_ReturnsUsingMD5HashValue()
    {
        bool usingMD5Hash = _userConfigurationService!.HashSettings.UsingMD5Hash;

        Assert.That(usingMD5Hash, Is.False);
    }

    [Test]
    public void UsingPHash_CorrectValue_ReturnsUsingPHashValue()
    {
        bool usingPHash = _userConfigurationService!.HashSettings.UsingPHash;

        Assert.That(usingPHash, Is.False);
    }

    [Test]
    public void AssetsDirectory_CorrectValue_ReturnsAssetsDirectoryValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;

        Assert.That(assetsDirectory, Is.Not.Null);
        Assert.That(assetsDirectory,
            Is.EqualTo(PathHelper.ToResolvedConfigPath("E:\\Workspace\\PhotoManager\\TestAssets")));
    }

    [Test]
    public void ExemptedFolderPath_CorrectValue_ReturnsExemptedFolderPathValue()
    {
        string exemptedFolderPath = _userConfigurationService!.PathSettings.ExemptedFolderPath;

        Assert.That(exemptedFolderPath, Is.Not.Null);
        Assert.That(exemptedFolderPath,
            Is.EqualTo(PathHelper.ToResolvedConfigPath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
    }

    [Test]
    public void FirstFrameVideosPath_CorrectValue_ReturnsFirstFrameVideosPathValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;
        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Assert.That(assetsDirectory, Is.Not.Null);
        Assert.That(firstFrameVideosPath, Is.Not.Null);

        Assert.That(firstFrameVideosPath,
            Is.EqualTo(Path.Combine(assetsDirectory, Directories.OUTPUT_VIDEO_FIRST_FRAME)));
        Assert.That(firstFrameVideosPath,
            Is.EqualTo(Path.Combine(PathHelper.ToResolvedConfigPath("E:\\Workspace\\PhotoManager\\TestAssets"),
                Directories.OUTPUT_VIDEO_FIRST_FRAME)));
    }

    [Test]
    public void ProjectName_CorrectValue_ReturnsProjectNameValue()
    {
        string projectName = _userConfigurationService!.ProjectSettings.Name;

        Assert.That(projectName, Is.Not.Null);
        Assert.That(projectName, Is.EqualTo("PhotoManager"));
    }

    [Test]
    public void ProjectOwner_CorrectValue_ReturnsProjectOwnerValue()
    {
        string projectOwner = _userConfigurationService!.ProjectSettings.Owner;

        Assert.That(projectOwner, Is.Not.Null);
        Assert.That(projectOwner, Is.EqualTo("Toto"));
    }

    [Test]
    public void BackupsToKeep_CorrectValue_ReturnsBackupsToKeepValue()
    {
        ushort backupsToKeep = _userConfigurationService!.StorageSettings.BackupsToKeep;

        Assert.That(backupsToKeep, Is.EqualTo(2));
    }

    [Test]
    public void ThumbnailsDictionaryEntriesToKeep_CorrectValue_ReturnsThumbnailsDictionaryEntriesToKeepValue()
    {
        ushort thumbnailsDictionaryEntriesToKeep =
            _userConfigurationService!.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        Assert.That(thumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));
    }

    [Test]
    public void GetRootCatalogFolderPaths_CorrectPath_ReturnsArrayOfPaths()
    {
        string[] paths = _userConfigurationService!.GetRootCatalogFolderPaths();

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Is.EqualTo(PathHelper.ToResolvedConfigPath("E:\\Workspace\\PhotoManager\\TestAssets")));
    }

    [Test]
    public void SaveEditableConfiguration_PersistenceAvailable_PersistsUpdatedSettings()
    {
        TestLogger<SqlitePersistenceContext> testLogger = new();
        SqlitePersistenceContext sqlitePersistenceContext = CreatePersistenceContext(testLogger);
        sqlitePersistenceContext.Initialize(_databaseDirectory!);

        try
        {
            IConfigurationRoot configurationRootMock = CreateConfiguration();
            UserConfigurationService userConfigurationService = new(configurationRootMock, sqlitePersistenceContext);
            EditableUserConfiguration configuration = CreateEditableConfiguration();

            Assert.That(userConfigurationService.AssetSettings.AnalyseVideos, Is.False);
            Assert.That(userConfigurationService.AssetSettings.CorruptedMessage, Is.EqualTo("The asset is corrupted"));
            Assert.That(userConfigurationService.AssetSettings.RotatedMessage,
                Is.EqualTo("The asset has been rotated"));
            Assert.That(userConfigurationService.AssetSettings.CatalogBatchSize, Is.EqualTo(100));
            Assert.That(userConfigurationService.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(5));
            Assert.That(userConfigurationService.AssetSettings.CorruptedImageOrientation, Is.EqualTo(10000));
            Assert.That(userConfigurationService.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
            Assert.That(userConfigurationService.AssetSettings.DetectThumbnails, Is.False);
            Assert.That(userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes, Is.False);
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(150));
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(200));

            Assert.That(userConfigurationService.HashSettings.PHashThreshold, Is.EqualTo(10));
            Assert.That(userConfigurationService.HashSettings.UsingDHash, Is.False);
            Assert.That(userConfigurationService.HashSettings.UsingMD5Hash, Is.False);
            Assert.That(userConfigurationService.HashSettings.UsingPHash, Is.False);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(
                PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
            Assert.That(userConfigurationService.PathSettings.FirstFrameVideosPath,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath(
                    $"E:\\Workspace\\PhotoManager\\TestAssets\\{Directories.OUTPUT_VIDEO_FIRST_FRAME}")));

            Assert.That(userConfigurationService.ProjectSettings.Name, Is.EqualTo("PhotoManager"));
            Assert.That(userConfigurationService.ProjectSettings.Owner, Is.EqualTo("Toto"));

            Assert.That(userConfigurationService.StorageSettings.BackupsToKeep, Is.EqualTo(2));
            Assert.That(userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));

            Assert.That(userConfigurationService.UiSettings.ThemeMode, Is.EqualTo("Light"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
                Is.EqualTo("The asset is corrupted"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
                Is.EqualTo("The asset has been rotated"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
                Is.EqualTo("100"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
                Is.EqualTo("5"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
                Is.EqualTo("10000"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
                Is.EqualTo("1"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
                Is.EqualTo(bool.FalseString));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
                Is.EqualTo("150"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
                Is.EqualTo("200"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
                Is.EqualTo("10"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
                Is.EqualTo(bool.FalseString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
                Is.EqualTo(bool.FalseString));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets")));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("E:\\Workspace\\PhotoManager\\TestAssets\\Exempted")));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
                Is.EqualTo(Directories.OUTPUT_VIDEO_FIRST_FRAME));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_NAME), Is.Null);
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_OWNER), Is.Null);

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
                Is.EqualTo("2"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
                Is.EqualTo("5"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
                Is.EqualTo("Light"));

            userConfigurationService.SaveEditableConfiguration(configuration);

            Assert.That(userConfigurationService.AssetSettings.AnalyseVideos, Is.True);
            Assert.That(userConfigurationService.AssetSettings.CorruptedMessage, Is.EqualTo("Corrupted"));
            Assert.That(userConfigurationService.AssetSettings.RotatedMessage, Is.EqualTo("Rotated"));
            Assert.That(userConfigurationService.AssetSettings.CatalogBatchSize, Is.EqualTo(42));
            Assert.That(userConfigurationService.AssetSettings.CatalogCooldownMinutes, Is.EqualTo(3));
            Assert.That(userConfigurationService.AssetSettings.CorruptedImageOrientation, Is.EqualTo(999));
            Assert.That(userConfigurationService.AssetSettings.DefaultExifOrientation, Is.EqualTo(1));
            Assert.That(userConfigurationService.AssetSettings.DetectThumbnails, Is.True);
            Assert.That(userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes, Is.True);
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxHeight, Is.EqualTo(320));
            Assert.That(userConfigurationService.AssetSettings.ThumbnailMaxWidth, Is.EqualTo(640));

            Assert.That(userConfigurationService.HashSettings.PHashThreshold, Is.EqualTo(6));
            Assert.That(userConfigurationService.HashSettings.UsingDHash, Is.True);
            Assert.That(userConfigurationService.HashSettings.UsingMD5Hash, Is.True);
            Assert.That(userConfigurationService.HashSettings.UsingPHash, Is.True);

            Assert.That(userConfigurationService.PathSettings.AssetsDirectory,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
            Assert.That(userConfigurationService.PathSettings.ExemptedFolderPath, Is.EqualTo(
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
            Assert.That(userConfigurationService.PathSettings.FirstFrameVideosPath,
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Frames")));

            Assert.That(userConfigurationService.ProjectSettings.Name, Is.EqualTo("PhotoManager"));
            Assert.That(userConfigurationService.ProjectSettings.Owner, Is.EqualTo("Toto"));

            Assert.That(userConfigurationService.StorageSettings.BackupsToKeep, Is.EqualTo(4));
            Assert.That(userConfigurationService.StorageSettings.ThumbnailsDictionaryEntriesToKeep, Is.EqualTo(12));

            Assert.That(userConfigurationService.UiSettings.ThemeMode, Is.EqualTo("Dark"));

            Assert.That(userConfigurationService.GetEditableConfiguration(), Is.SameAs(configuration));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ANALYSE_VIDEOS),
                Is.EqualTo(bool.TrueString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_CORRUPTED_MESSAGE),
                Is.EqualTo("Corrupted"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSET_ROTATED_MESSAGE),
                Is.EqualTo("Rotated"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE),
                Is.EqualTo("42"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CATALOG_COOLDOWN_MINUTES),
                Is.EqualTo("3"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.CORRUPTED_IMAGE_ORIENTATION),
                Is.EqualTo("999"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DEFAULT_EXIF_ORIENTATION),
                Is.EqualTo("1"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.DETECT_THUMBNAILS),
                Is.EqualTo(bool.TrueString));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.SYNC_ASSETS_EVERY_X_MINUTES),
                Is.EqualTo(bool.TrueString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_HEIGHT),
                Is.EqualTo("320"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAIL_MAX_WIDTH),
                Is.EqualTo("640"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PHASH_THRESHOLD),
                Is.EqualTo("6"));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_DHASH),
                Is.EqualTo(bool.TrueString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_MD5_HASH),
                Is.EqualTo(bool.TrueString));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.USING_PHASH),
                Is.EqualTo(bool.TrueString));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.ASSETS_DIRECTORY),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets")));
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.EXEMPTED_FOLDER_PATH),
                Is.EqualTo(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted")));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.FIRST_FRAME_VIDEOS_FOLDER_NAME),
                Is.EqualTo("Frames"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_NAME), Is.Null);
            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.PROJECT_OWNER), Is.Null);

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.BACKUPS_TO_KEEP),
                Is.EqualTo("4"));
            Assert.That(
                sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THUMBNAILS_DICTIONARY_ENTRIES_TO_KEEP),
                Is.EqualTo("12"));

            Assert.That(sqlitePersistenceContext.Configuration.GetValue(UserConfigurationKeys.THEME_MODE),
                Is.EqualTo("Dark"));

            testLogger.AssertLogExceptions([], typeof(SqlitePersistenceContext));
        }
        finally
        {
            sqlitePersistenceContext.Dispose();
            TearDownHelper.DeleteTempDbDirectories(_databaseDirectory!);
        }
    }

    private static SqlitePersistenceContext CreatePersistenceContext(TestLogger<SqlitePersistenceContext> logger)
    {
        SqliteConnectionFactory factory = new(new TestLogger<SqliteConnectionFactory>());
        SqliteBackupService backupService = new(factory);

        return new(factory, backupService, logger);
    }

    private static IConfigurationRoot CreateConfiguration(
        string catalogBatchSize = "100",
        string projectName = "PhotoManager",
        string projectOwner = "Toto",
        string themeMode = "Light")
    {
        IConfigurationRoot configurationRootMock = Substitute.For<IConfigurationRoot>();
        configurationRootMock.GetDefaultMockConfig();
        configurationRootMock
            .MockGetValue(UserConfigurationKeys.CATALOG_BATCH_SIZE, catalogBatchSize)
            .MockGetValue(UserConfigurationKeys.PROJECT_NAME, projectName)
            .MockGetValue(UserConfigurationKeys.PROJECT_OWNER, projectOwner)
            .MockGetValue(UserConfigurationKeys.THEME_MODE, themeMode);

        return configurationRootMock;
    }

    private static EditableUserConfiguration CreateEditableConfiguration()
    {
        return new(
            new(true, "Corrupted", "Rotated", 42, 3, 999, 1, true, true, 320, 640),
            new(6, true, true, true),
            new(PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets"),
                PathHelper.ToPlatformAbsolutePath("C:\\PhotoManager\\Assets\\Exempted"),
                "Frames"),
            new(4, 12),
            new("Dark"));
    }
}
