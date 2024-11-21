using PhotoManager.UI;

namespace PhotoManager.Tests.Unit.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Mock<IConfigurationRoot> configurationRootMock = new();
        configurationRootMock.GetDefaultMockConfig();

        _userConfigurationService = new UserConfigurationService(configurationRootMock.Object);
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

        Assert.AreEqual(100, catalogBatchSize);
    }

    [Test]
    public void CatalogCooldownMinutes_CorrectValue_ReturnsCatalogCooldownMinutesValue()
    {
        ushort catalogCooldownMinutes = _userConfigurationService!.AssetSettings.CatalogCooldownMinutes;

        Assert.AreEqual(5, catalogCooldownMinutes);
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
        Assert.AreEqual("C:\\Path", assetsDirectory);
    }

    [Test]
    public void BackupPath_CorrectValue_ReturnsBackupPathValue()
    {
        string backupPath = _userConfigurationService!.PathSettings.BackupPath;

        Assert.IsNotNull(backupPath);
        Assert.AreEqual("C:\\Path\\To\\Backup", backupPath);
    }

    [Test]
    public void ExemptedFolderPath_CorrectValue_ReturnsExemptedFolderPathValue()
    {
        string exemptedFolderPath = _userConfigurationService!.PathSettings.ExemptedFolderPath;

        Assert.IsNotNull(exemptedFolderPath);
        Assert.AreEqual("C:\\Path\\To\\FolderExempted", exemptedFolderPath);
    }

    [Test]
    public void FirstFrameVideosPath_CorrectValue_ReturnsFirstFrameVideosPathValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;
        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Assert.IsNotNull(assetsDirectory);
        Assert.IsNotNull(firstFrameVideosPath);

        Assert.AreEqual(Path.Combine(assetsDirectory, "OutputVideoFirstFrame"), firstFrameVideosPath);
        Assert.AreEqual(Path.Combine("C:\\Path\\OutputVideoFirstFrame"), firstFrameVideosPath);
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
        Assert.AreEqual("C:\\Path", paths[0]);
    }
}
