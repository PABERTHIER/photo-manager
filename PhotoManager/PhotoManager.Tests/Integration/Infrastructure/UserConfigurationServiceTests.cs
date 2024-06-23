using PhotoManager.UI;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private string? dataDirectory;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
        string configFilePath = Path.Combine(dataDirectory, "appsettings.json");

        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        _userConfigurationService = new UserConfigurationService(configuration);
    }

    [Test]
    [Ignore("Tests ignored to prevent effects on the client computer")]
    [TestCase(WallpaperStyle.Fit, "6", "0")]
    [TestCase(WallpaperStyle.Stretch, "2", "0")]
    [TestCase(WallpaperStyle.Tile, "0", "1")]
    [TestCase(WallpaperStyle.Center, "0", "0")]
    [TestCase(WallpaperStyle.Span, "22", "0")]
    [TestCase(WallpaperStyle.Fill, "10", "0")]
    public void SetAsWallpaper_ValidStyleAndTile_RegistersExpectedValues(WallpaperStyle style, string expectedStyleValue, string expectedTileValue)
    {
        Folder folder = new() { Path = dataDirectory! };
        Asset asset = new() { Folder = folder, FileName = "NonExistentFile.jpg" }; // Not giving an existing file to prevent the wallpaper to be changed

        // Set up a StringWriter to capture console output
        StringWriter stringWriter = new();
        Console.SetOut(stringWriter);

        _userConfigurationService!.SetAsWallpaper(asset, style);

        // Get the captured console output
        string consoleOutput = stringWriter.ToString();

        // Assert that the expected error message was printed to the console
        Assert.IsTrue(consoleOutput.Contains($"Wallpaper set for style {expectedStyleValue} and tile {expectedTileValue}"));
    }

    [Test]
    public void GetAboutInformation_WithValidAssembly_ReturnsAboutInformation()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(App).Assembly);

        Assert.AreEqual("PhotoManager", aboutInformation.Product);
        Assert.AreEqual("Toto", aboutInformation.Author);
        Assert.IsFalse(string.IsNullOrWhiteSpace(aboutInformation.Version));
        Assert.That(aboutInformation.Version, Does.StartWith("v"));
        Assert.AreEqual("v1.0.0", aboutInformation.Version);
    }

    [Test]
    public void GetAboutInformation_WithInvalidAssembly_ReturnsDefaultProduct()
    {
        AboutInformation aboutInformation = _userConfigurationService!.GetAboutInformation(typeof(int).Assembly);

        Assert.AreNotEqual("PhotoManager", aboutInformation.Product);
        Assert.AreEqual("Toto", aboutInformation.Author);
        Assert.AreEqual("v1.0.0", aboutInformation.Version);
    }

    [Test]
    public void AnalyseVideos_CorrectValue_ReturnsAnalyseVideosValue()
    {
        bool analyseVideos = _userConfigurationService!.AssetSettings.AnalyseVideos;

        Assert.IsFalse(analyseVideos);
    }

    [Test]
    public void AssetCorruptedMessage_CorrectValue_ReturnsAssetCorruptedMessageValue()
    {
        string assetCorruptedMessage = _userConfigurationService!.AssetSettings.AssetCorruptedMessage;

        Assert.IsNotNull(assetCorruptedMessage);
        Assert.AreEqual("The asset is corrupted", assetCorruptedMessage);
    }

    [Test]
    public void AssetRotatedMessage_CorrectValue_ReturnsAssetRotatedMessageValue()
    {
        string assetRotatedMessage = _userConfigurationService!.AssetSettings.AssetRotatedMessage;

        Assert.IsNotNull(assetRotatedMessage);
        Assert.AreEqual("The asset has been rotated", assetRotatedMessage);
    }

    [Test]
    public void CatalogBatchSize_CorrectValue_ReturnsCatalogBatchSizeValue()
    {
        int catalogBatchSize = _userConfigurationService!.AssetSettings.CatalogBatchSize;

        Assert.AreEqual(100000, catalogBatchSize);
    }

    [Test]
    public void CatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue()
    {
        ushort catalogCooldownMinutes = _userConfigurationService!.AssetSettings.CatalogCooldownMinutes;

        Assert.AreEqual(2, catalogCooldownMinutes);
    }

    [Test]
    public void CorruptedImageOrientation_CorrectValue_ReturnsCorruptedImageOrientationValue()
    {
        ushort corruptedImageOrientation = _userConfigurationService!.AssetSettings.CorruptedImageOrientation;

        Assert.AreEqual(10000, corruptedImageOrientation);
    }

    [Test]
    public void DefaultExifOrientation_CorrectValue_ReturnsDefaultExifOrientationValue()
    {
        ushort defaultExifOrientation = _userConfigurationService!.AssetSettings.DefaultExifOrientation;

        Assert.AreEqual(1, defaultExifOrientation);
    }

    [Test]
    public void DetectThumbnails_CorrectValue_ReturnsDetectThumbnailsValue()
    {
        bool detectThumbnails = _userConfigurationService!.AssetSettings.DetectThumbnails;

        Assert.IsFalse(detectThumbnails);
    }

    [Test]
    public void SyncAssetsEveryXMinutes_CorrectValue_ReturnsSyncAssetsEveryXMinutesValue()
    {
        bool syncAssetsEveryXMinutes = _userConfigurationService!.AssetSettings.SyncAssetsEveryXMinutes;

        Assert.IsFalse(syncAssetsEveryXMinutes);
    }

    [Test]
    public void ThumbnailMaxHeight_CorrectValue_ReturnsThumbnailMaxHeightValue()
    {
        int thumbnailMaxHeight = _userConfigurationService!.AssetSettings.ThumbnailMaxHeight;

        Assert.AreEqual(150, thumbnailMaxHeight);
    }

    [Test]
    public void ThumbnailMaxWidth_CorrectValue_ReturnsThumbnailMaxWidthValue()
    {
        int thumbnailMaxWidth = _userConfigurationService!.AssetSettings.ThumbnailMaxWidth;

        Assert.AreEqual(200, thumbnailMaxWidth);
    }

    [Test]
    public void PHashThreshold_CorrectValue_ReturnsPHashThresholdValue()
    {
        ushort pHashThreshold = _userConfigurationService!.HashSettings.PHashThreshold;

        Assert.AreEqual(40, pHashThreshold);
    }

    [Test]
    public void UsingDHash_CorrectValue_ReturnsUsingDHashValue()
    {
        bool usingDHash = _userConfigurationService!.HashSettings.UsingDHash;

        Assert.IsFalse(usingDHash);
    }

    [Test]
    public void UsingMD5Hash_CorrectValue_ReturnsUsingMD5HashValue()
    {
        bool usingMD5Hash = _userConfigurationService!.HashSettings.UsingMD5Hash;

        Assert.IsFalse(usingMD5Hash);
    }

    [Test]
    public void UsingPHash_CorrectValue_ReturnsUsingPHashValue()
    {
        bool usingPHash = _userConfigurationService!.HashSettings.UsingPHash;

        Assert.IsFalse(usingPHash);
    }

    [Test]
    public void AssetsDirectory_CorrectValue_ReturnsAssetsDirectoryValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;

        Assert.IsNotNull(assetsDirectory);
        Assert.AreEqual("E:\\Workspace\\PhotoManager\\Test", assetsDirectory);
    }

    [Test]
    public void BackupPath_CorrectValue_ReturnsBackupPathValue()
    {
        string backupPath = _userConfigurationService!.PathSettings.BackupPath;

        Assert.IsNotNull(backupPath);
        Assert.AreEqual("E:\\Workspace\\PhotoManager\\Backup", backupPath);
    }

    [Test]
    public void ExemptedFolderPath_CorrectValue_ReturnsExemptedFolderPathValue()
    {
        string exemptedFolderPath = _userConfigurationService!.PathSettings.ExemptedFolderPath;

        Assert.IsNotNull(exemptedFolderPath);
        Assert.AreEqual("E:\\Workspace\\PhotoManager\\Test\\test1", exemptedFolderPath);
    }

    [Test]
    public void FfmpegPath_CorrectValue_ReturnsFfmpegPathValue()
    {
        string ffmpegPath = _userConfigurationService!.PathSettings.FfmpegPath;

        Assert.IsNotNull(ffmpegPath);
        Assert.AreEqual("E:\\ffmpeg\\bin\\ffmpeg.exe", ffmpegPath);
    }

    [Test]
    public void FirstFrameVideosPath_CorrectValue_ReturnsFirstFrameVideosPathValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;
        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Assert.IsNotNull(assetsDirectory);
        Assert.IsNotNull(firstFrameVideosPath);

        Assert.AreEqual(Path.Combine(assetsDirectory!, "OutputVideoFirstFrame"), firstFrameVideosPath);
        Assert.AreEqual(Path.Combine("E:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame"), firstFrameVideosPath);
    }

    [Test]
    public void ProjectName_CorrectValue_ReturnsProjectNameValue()
    {
        string projectName = _userConfigurationService!.ProjectSettings.Name;

        Assert.IsNotNull(projectName);
        Assert.AreEqual("PhotoManager", projectName);
    }

    [Test]
    public void ProjectOwner_CorrectValue_ReturnsProjectOwnerValue()
    {
        string projectOwner = _userConfigurationService!.ProjectSettings.Owner;

        Assert.IsNotNull(projectOwner);
        Assert.AreEqual("Toto", projectOwner);
    }

    [Test]
    public void BackupsToKeep_CorrectValue_ReturnsBackupsToKeepValue()
    {
        ushort backupsToKeep = _userConfigurationService!.StorageSettings.BackupsToKeep;

        Assert.AreEqual(2, backupsToKeep);
    }

    [Test]
    public void FoldersNameBlobs_CorrectValue_ReturnsFoldersNameBlobsValue()
    {
        string folderNameBlobs = _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs;

        Assert.IsNotNull(folderNameBlobs);
        Assert.AreEqual("Blobs", folderNameBlobs);
    }

    [Test]
    public void FoldersNameTables_CorrectValue_ReturnsFoldersNameTablesValue()
    {
        string foldersNameTables = _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables;

        Assert.IsNotNull(foldersNameTables);
        Assert.AreEqual("Tables", foldersNameTables);
    }

    [Test]
    public void Separator_CorrectValue_ReturnsSeparatorValue()
    {
        char separator = _userConfigurationService!.StorageSettings.Separator;

        Assert.IsNotNull(separator);
        Assert.AreEqual('|', separator);
    }

    [Test]
    public void StorageVersion_CorrectValue_ReturnsStorageVersionValue()
    {
        string storageVersion = _userConfigurationService!.StorageSettings.StorageVersion;

        Assert.IsNotNull(storageVersion);
        Assert.AreEqual("1.0", storageVersion);
    }

    [Test]
    public void TablesSettingsAssetsTableName_CorrectValue_ReturnsTablesSettingsAssetsTableNameValue()
    {
        string tablesSettingsAssetsTableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName;

        Assert.IsNotNull(tablesSettingsAssetsTableName);
        Assert.AreEqual("Assets", tablesSettingsAssetsTableName);
    }

    [Test]
    public void TablesSettingsFoldersTableName_CorrectValue_ReturnsTablesSettingsFoldersTableNameValue()
    {
        string tablesSettingsFoldersTableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName;

        Assert.IsNotNull(tablesSettingsFoldersTableName);
        Assert.AreEqual("Folders", tablesSettingsFoldersTableName);
    }

    [Test]
    public void TablesSettingsRecentTargetPathsTableName_CorrectValue_ReturnsTablesSettingsRecentTargetPathsTableNameValue()
    {
        string tablesSettingsRecentTargetPathsTableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName;

        Assert.IsNotNull(tablesSettingsRecentTargetPathsTableName);
        Assert.AreEqual("RecentTargetPaths", tablesSettingsRecentTargetPathsTableName);
    }

    [Test]
    public void TablesSettingsSyncAssetsDirectoriesDefinitionsTableName_CorrectValue_ReturnsTablesSettingsSyncAssetsDirectoriesDefinitionsTableNameValue()
    {
        string tablesSettingsSyncAssetsDirectoriesDefinitionsTableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName;

        Assert.IsNotNull(tablesSettingsSyncAssetsDirectoriesDefinitionsTableName);
        Assert.AreEqual("SyncAssetsDirectoriesDefinitions", tablesSettingsSyncAssetsDirectoriesDefinitionsTableName);
    }

    [Test]
    public void ThumbnailsDictionaryEntriesToKeep_CorrectValue_ReturnsThumbnailsDictionaryEntriesToKeepValue()
    {
        ushort thumbnailsDictionaryEntriesToKeep = _userConfigurationService!.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        Assert.AreEqual(5, thumbnailsDictionaryEntriesToKeep);
    }

    [Test]
    public void GetRootCatalogFolderPaths_CorrectPath_ReturnsArrayOfPaths()
    {
        string[] paths = _userConfigurationService!.GetRootCatalogFolderPaths();

        Assert.AreEqual(1, paths.Length);
        Assert.AreEqual("E:\\Workspace\\PhotoManager\\Test", paths[0]);
    }
}

