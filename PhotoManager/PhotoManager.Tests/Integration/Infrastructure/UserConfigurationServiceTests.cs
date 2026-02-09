using PhotoManager.UI;
using System.Reflection;
using System.Reflection.Emit;
using Directories = PhotoManager.Tests.Integration.Constants.Directories;

namespace PhotoManager.Tests.Integration.Infrastructure;

[TestFixture]
public class UserConfigurationServiceTests
{
    private string? _dataDirectory;
    private UserConfigurationService? _userConfigurationService;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Directories.TEST_FILES);
        string configFilePath = Path.Combine(_dataDirectory, "appsettings.json");

        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        _userConfigurationService = new UserConfigurationService(configuration);
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
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

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
        Assert.That(assetsDirectory, Is.EqualTo("E:\\Workspace\\PhotoManager\\Test"));
    }

    [Test]
    public void BackupPath_CorrectValue_ReturnsBackupPathValue()
    {
        string backupPath = _userConfigurationService!.PathSettings.BackupPath;

        Assert.That(backupPath, Is.Not.Null);
        Assert.That(backupPath, Is.EqualTo("E:\\Workspace\\PhotoManager\\Backup"));
    }

    [Test]
    public void ExemptedFolderPath_CorrectValue_ReturnsExemptedFolderPathValue()
    {
        string exemptedFolderPath = _userConfigurationService!.PathSettings.ExemptedFolderPath;

        Assert.That(exemptedFolderPath, Is.Not.Null);
        Assert.That(exemptedFolderPath, Is.EqualTo("E:\\Workspace\\PhotoManager\\Test\\test1"));
    }

    [Test]
    public void FirstFrameVideosPath_CorrectValue_ReturnsFirstFrameVideosPathValue()
    {
        string assetsDirectory = _userConfigurationService!.PathSettings.AssetsDirectory;
        string firstFrameVideosPath = _userConfigurationService!.PathSettings.FirstFrameVideosPath;

        Assert.That(assetsDirectory, Is.Not.Null);
        Assert.That(firstFrameVideosPath, Is.Not.Null);

        Assert.That(firstFrameVideosPath, Is.EqualTo(Path.Combine(assetsDirectory, Directories.OUTPUT_VIDEO_FIRST_FRAME)));
        Assert.That(firstFrameVideosPath, Is.EqualTo(Path.Combine("E:\\Workspace\\PhotoManager\\Test\\OutputVideoFirstFrame")));
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
    public void FoldersNameBlobs_CorrectValue_ReturnsFoldersNameBlobsValue()
    {
        string folderNameBlobs = _userConfigurationService!.StorageSettings.FoldersNameSettings.Blobs;

        Assert.That(folderNameBlobs, Is.Not.Null);
        Assert.That(folderNameBlobs, Is.EqualTo(Directories.BLOBS));
    }

    [Test]
    public void FoldersNameTables_CorrectValue_ReturnsFoldersNameTablesValue()
    {
        string foldersNameTables = _userConfigurationService!.StorageSettings.FoldersNameSettings.Tables;

        Assert.That(foldersNameTables, Is.Not.Null);
        Assert.That(foldersNameTables, Is.EqualTo(Directories.TABLES));
    }

    [Test]
    public void Separator_CorrectValue_ReturnsSeparatorValue()
    {
        char separator = _userConfigurationService!.StorageSettings.Separator;

        Assert.That(separator, Is.EqualTo('|'));
    }

    [Test]
    public void StorageVersion_CorrectValue_ReturnsStorageVersionValue()
    {
        string storageVersion = _userConfigurationService!.StorageSettings.StorageVersion;

        Assert.That(storageVersion, Is.Not.Null);
        Assert.That(storageVersion, Is.EqualTo("1.0"));
    }

    [Test]
    public void TablesSettingsAssetsTableName_CorrectValue_ReturnsTablesSettingsAssetsTableNameValue()
    {
        string tablesSettingsAssetsTableName = _userConfigurationService!.StorageSettings.TablesSettings.AssetsTableName;

        Assert.That(tablesSettingsAssetsTableName, Is.Not.Null);
        Assert.That(tablesSettingsAssetsTableName, Is.EqualTo("Assets"));
    }

    [Test]
    public void TablesSettingsFoldersTableName_CorrectValue_ReturnsTablesSettingsFoldersTableNameValue()
    {
        string tablesSettingsFoldersTableName = _userConfigurationService!.StorageSettings.TablesSettings.FoldersTableName;

        Assert.That(tablesSettingsFoldersTableName, Is.Not.Null);
        Assert.That(tablesSettingsFoldersTableName, Is.EqualTo("Folders"));
    }

    [Test]
    public void TablesSettingsRecentTargetPathsTableName_CorrectValue_ReturnsTablesSettingsRecentTargetPathsTableNameValue()
    {
        string tablesSettingsRecentTargetPathsTableName = _userConfigurationService!.StorageSettings.TablesSettings.RecentTargetPathsTableName;

        Assert.That(tablesSettingsRecentTargetPathsTableName, Is.Not.Null);
        Assert.That(tablesSettingsRecentTargetPathsTableName, Is.EqualTo("RecentTargetPaths"));
    }

    [Test]
    public void TablesSettingsSyncAssetsDirectoriesDefinitionsTableName_CorrectValue_ReturnsTablesSettingsSyncAssetsDirectoriesDefinitionsTableNameValue()
    {
        string tablesSettingsSyncAssetsDirectoriesDefinitionsTableName = _userConfigurationService!.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName;

        Assert.That(tablesSettingsSyncAssetsDirectoriesDefinitionsTableName, Is.Not.Null);
        Assert.That(tablesSettingsSyncAssetsDirectoriesDefinitionsTableName, Is.EqualTo("SyncAssetsDirectoriesDefinitions"));
    }

    [Test]
    public void ThumbnailsDictionaryEntriesToKeep_CorrectValue_ReturnsThumbnailsDictionaryEntriesToKeepValue()
    {
        ushort thumbnailsDictionaryEntriesToKeep = _userConfigurationService!.StorageSettings.ThumbnailsDictionaryEntriesToKeep;

        Assert.That(thumbnailsDictionaryEntriesToKeep, Is.EqualTo(5));
    }

    [Test]
    public void GetRootCatalogFolderPaths_CorrectPath_ReturnsArrayOfPaths()
    {
        string[] paths = _userConfigurationService!.GetRootCatalogFolderPaths();

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0], Is.EqualTo("E:\\Workspace\\PhotoManager\\Test"));
    }
}

