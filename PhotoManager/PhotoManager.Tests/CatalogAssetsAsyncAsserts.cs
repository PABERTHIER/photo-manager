using System.IO.Compression;
using Tables = PhotoManager.Tests.Integration.Constants.Tables;

namespace PhotoManager.Tests;

public static class CatalogAssetsAsyncAsserts
{
    public const string CREATING_BACKUP_MESSAGE = "Creating catalog backup...";
    public const string UPDATING_BACKUP_MESSAGE = "Updating catalog backup...";

    public static void CheckBlobsAndTablesBeforeSaveCatalog(string blobsPath, string tablesPath)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.That(blobFiles, Is.Empty);
        Assert.That(tableFiles, Is.Empty);

        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.ASSETS_DB)), Is.False);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.FOLDERS_DB)), Is.False);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.False);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.RECENT_TARGET_PATHS_DB)), Is.False);
    }

    public static void CheckBlobsAndTablesAfterSaveCatalog(
        IBlobStorage blobStorage,
        IDatabase database,
        IUserConfigurationService userConfigurationService,
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.That(blobFiles, Has.Length.EqualTo(foldersContainingAssets.Count));

        foreach (Folder folder in foldersContainingAssets)
        {
            string blobFileName = $"{folder.Id}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            Assert.That(File.Exists(blobFilePath), Is.True);

            List<Asset> assetsFromRepositoryByFolder = [.. assetsFromRepository.Where(x => x.FolderId == folder.Id)];

            Dictionary<string, byte[]>? dataRead = blobStorage.ReadFromBinaryFile(blobFilePath);
            Assert.That(dataRead, Is.Not.Null);
            Assert.That(dataRead!, Has.Count.EqualTo(assetsFromRepositoryByFolder.Count));

            for (int i = 0; i < dataRead.Count; i++)
            {
                Assert.That(dataRead.ContainsKey(assetsFromRepositoryByFolder[i].FileName), Is.True);
                Assert.That(assetNameToByteSizeMapping.ContainsKey(assetsFromRepositoryByFolder[i].FileName), Is.True);
                Assert.That(dataRead[assetsFromRepositoryByFolder[i].FileName], Has.Length.EqualTo(assetNameToByteSizeMapping[assetsFromRepositoryByFolder[i].FileName]));
            }
        }

        Assert.That(tableFiles, Has.Length.EqualTo(4));
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.ASSETS_DB)), Is.True);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.FOLDERS_DB)), Is.True);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
        Assert.That(File.Exists(Path.Combine(tablesPath, Tables.RECENT_TARGET_PATHS_DB)), Is.True);

        List<Asset> assetsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.That(assetsFromDatabase, Has.Count.EqualTo(assetsFromRepository.Count));

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        foreach (Asset assetFromDatabase in assetsFromDatabase)
        {
            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == assetFromDatabase.FileName && a.FolderId == assetFromDatabase.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            AssertAssetFromDatabaseValidity(assetFromDatabase, expectedAsset, expectedFolder.Id);
        }

        Assert.That(foldersFromDatabase, Has.Count.EqualTo(folders.Count));

        Dictionary<Guid, Folder> foldersById = folders.ToDictionary(f => f.Id, f => f);
        Dictionary<Guid, Folder> foldersFromDatabaseById = foldersFromDatabase.ToDictionary(f => f.Id, f => f);

        foreach ((Guid folderId, Folder expectedFolder) in foldersById)
        {
            Folder actualFolder = foldersFromDatabaseById[folderId];

            Assert.That(actualFolder.Id, Is.EqualTo(expectedFolder.Id));
            Assert.That(actualFolder.Path, Is.EqualTo(expectedFolder.Path));
        }

        Assert.That(syncAssetsDirectoriesDefinitionsFromDatabase, Is.Empty);
        Assert.That(recentTargetPathsFromDatabase, Is.Empty);
    }

    public static void CheckBlobsAndTablesAfterSaveCatalogEmpty(
        IDatabase database,
        IUserConfigurationService userConfigurationService,
        string blobsPath,
        string tablesPath,
        bool hasEmptyTables,
        bool hasOneFolder,
        Folder folder)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.That(blobFiles, Is.Empty);

        if (hasEmptyTables)
        {
            Assert.That(tableFiles, Has.Length.EqualTo(4));
            Assert.That(File.Exists(Path.Combine(tablesPath, Tables.ASSETS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(tablesPath, Tables.FOLDERS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(tablesPath, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB)), Is.True);
            Assert.That(File.Exists(Path.Combine(tablesPath, Tables.RECENT_TARGET_PATHS_DB)), Is.True);
        }
        else
        {
            Assert.That(tableFiles, Is.Empty);
        }

        List<Asset> assetsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.That(assetsFromDatabase, Is.Empty);

        if (hasOneFolder)
        {
            Assert.That(foldersFromDatabase, Has.Count.EqualTo(1));
            Assert.That(foldersFromDatabase[0].Id, Is.EqualTo(folder.Id));
            Assert.That(foldersFromDatabase[0].Path, Is.EqualTo(folder.Path));
        }
        else
        {
            Assert.That(foldersFromDatabase, Is.Empty);
        }

        Assert.That(syncAssetsDirectoriesDefinitionsFromDatabase, Is.Empty);
        Assert.That(recentTargetPathsFromDatabase, Is.Empty);
    }

    public static void CheckBackupBefore(IAssetRepository testableAssetRepository, string backupFilePath)
    {
        Assert.That(File.Exists(backupFilePath), Is.False);
        Assert.That(testableAssetRepository.BackupExists(), Is.False);
    }

    public static void CheckBackupAfter(
        IBlobStorage blobStorage,
        IDatabase database,
        IUserConfigurationService userConfigurationService,
        string databasePath,
        string databaseBackupPath,
        string backupFilePath,
        string blobsPath,
        string tablesPath,
        IReadOnlyCollection<Folder> folders,
        IReadOnlyCollection<Folder> foldersContainingAssets,
        IReadOnlyCollection<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        Dictionary<string, int> assetNameToByteSizeMapping)
    {
        string backupBlobsDirectory = Path.Combine(databaseBackupPath, userConfigurationService.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(databaseBackupPath, userConfigurationService.StorageSettings.FoldersNameSettings.Tables);

        Assert.That(Directory.Exists(backupBlobsDirectory), Is.False);
        Assert.That(Directory.Exists(backupTablesDirectory), Is.False);

        ZipFile.ExtractToDirectory(backupFilePath, databaseBackupPath);
        Assert.That(File.Exists(backupFilePath), Is.True);

        Assert.That(Directory.Exists(backupBlobsDirectory), Is.True);
        Assert.That(Directory.Exists(backupTablesDirectory), Is.True);

        string[] sourceDirectories = Directory.GetDirectories(databasePath);
        string[] backupDirectories = Directory.GetDirectories(databaseBackupPath);

        Assert.That(backupDirectories, Has.Length.EqualTo(sourceDirectories.Length));

        Assert.That(blobsPath, Is.EqualTo(sourceDirectories[0]));
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.That(blobs, Has.Length.EqualTo(foldersContainingAssets.Count));

        Assert.That(tablesPath, Is.EqualTo(sourceDirectories[1]));
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.That(tables, Has.Length.EqualTo(4));

        Assert.That(backupBlobsDirectory, Is.EqualTo(backupDirectories[0]));
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.That(blobsBackup, Has.Length.EqualTo(foldersContainingAssets.Count));

        Assert.That(backupTablesDirectory, Is.EqualTo(backupDirectories[1]));
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.That(tablesBackup, Has.Length.EqualTo(4));

        CheckBlobsAndTablesAfterSaveCatalog(
            blobStorage,
            database,
            userConfigurationService,
            backupBlobsDirectory,
            backupTablesDirectory,
            folders,
            foldersContainingAssets,
            assetsFromRepository,
            folderToAssetsMapping,
            assetNameToByteSizeMapping);

        Directory.Delete(backupBlobsDirectory, true);
        Directory.Delete(backupTablesDirectory, true);

        Assert.That(Directory.Exists(backupBlobsDirectory), Is.False);
        Assert.That(Directory.Exists(backupTablesDirectory), Is.False);
    }

    public static void CheckDefaultEmptyBackup(
        IDatabase database,
        IUserConfigurationService userConfigurationService,
        string databasePath,
        string databaseBackupPath,
        string backupFilePath,
        string blobsPath,
        string tablesPath,
        bool hasEmptyTables,
        bool hasOneFolder,
        Folder folder)
    {
        int expectedTablesCount = hasEmptyTables ? 4 : 0;

        string backupBlobsDirectory = Path.Combine(databaseBackupPath, userConfigurationService.StorageSettings.FoldersNameSettings.Blobs);
        string backupTablesDirectory = Path.Combine(databaseBackupPath, userConfigurationService.StorageSettings.FoldersNameSettings.Tables);

        Assert.That(Directory.Exists(backupBlobsDirectory), Is.False);
        Assert.That(Directory.Exists(backupTablesDirectory), Is.False);

        ZipFile.ExtractToDirectory(backupFilePath, databaseBackupPath);
        File.Delete(backupFilePath);
        Assert.That(File.Exists(backupFilePath), Is.False);

        Assert.That(Directory.Exists(backupBlobsDirectory), Is.True);
        Assert.That(Directory.Exists(backupTablesDirectory), Is.True);

        string[] sourceDirectories = Directory.GetDirectories(databasePath);
        string[] backupDirectories = Directory.GetDirectories(databaseBackupPath);

        Assert.That(backupDirectories, Has.Length.EqualTo(sourceDirectories.Length));

        Assert.That(blobsPath, Is.EqualTo(sourceDirectories[0]));
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.That(blobs, Is.Empty);

        Assert.That(tablesPath, Is.EqualTo(sourceDirectories[1]));
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.That(tables, Has.Length.EqualTo(expectedTablesCount));

        Assert.That(backupBlobsDirectory, Is.EqualTo(backupDirectories[0]));
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.That(blobsBackup, Is.Empty);

        Assert.That(backupTablesDirectory, Is.EqualTo(backupDirectories[1]));
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.That(tablesBackup, Has.Length.EqualTo(expectedTablesCount));

        CheckBlobsAndTablesAfterSaveCatalogEmpty(database, userConfigurationService, backupBlobsDirectory, backupTablesDirectory, hasEmptyTables, hasOneFolder, folder);
    }

    public static void AssertAssetPropertyValidityAndImageData(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Null); // Set above, not in this method
    }

    public static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath,
        Folder folder)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.That(asset.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(asset.FolderId, Is.EqualTo(folder.Id));
        Assert.That(asset.Folder, Is.EqualTo(folder));
        Assert.That(asset.FileProperties.Size, Is.EqualTo(expectedAsset.FileProperties.Size));
        Assert.That(asset.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(asset.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(asset.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(asset.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(asset.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(asset.ThumbnailCreationDateTime.Date, Is.EqualTo(actualDate));
        Assert.That(asset.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(asset.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(asset.Metadata.Corrupted.Message, Is.EqualTo(expectedAsset.Metadata.Corrupted.Message));
        Assert.That(asset.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(asset.Metadata.Rotated.Message, Is.EqualTo(expectedAsset.Metadata.Rotated.Message));
        Assert.That(asset.FullPath, Is.EqualTo(assetPath));
        Assert.That(asset.Folder.Path, Is.EqualTo(folderPath));
        Assert.That(asset.FileProperties.Creation.Date, Is.EqualTo(actualDate)); // Because files are generated by tests (ThumbnailCreationDateTime and FileModificationDateTime have the same value)
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    public static void AssertThumbnailsValidity(
        IReadOnlyList<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        IReadOnlyList<Folder> folders,
        Dictionary<string, Dictionary<string, byte[]>> thumbnails,
        IReadOnlyList<int> assetsImageByteSize)
    {
        Assert.That(thumbnails, Has.Count.EqualTo(folders.Count));

        int thumbnailsTotalCount = 0;

        for (int i = 0; i < thumbnails.Count; i++)
        {
            Assert.That(thumbnails.ContainsKey(folders[i].Path), Is.True);
            thumbnailsTotalCount += thumbnails[folders[i].Path].Count;
        }

        Assert.That(thumbnailsTotalCount, Is.EqualTo(assetsFromRepository.Count));

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        for (int i = 0; i < assetsFromRepository.Count; i++)
        {
            Asset currentAsset = assetsFromRepository[i];

            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == currentAsset.FileName && a.FolderId == currentAsset.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            Assert.That(thumbnails[expectedFolder.Path].ContainsKey(currentAsset.FileName), Is.True);

            byte[] assetImageByteSize = thumbnails[expectedFolder.Path][currentAsset.FileName];

            Assert.That(assetImageByteSize, Is.Not.Null);
            Assert.That(assetImageByteSize, Has.Length.EqualTo(assetsImageByteSize[i]));
        }
    }

    private static void AssertAssetFromDatabaseValidity(Asset assetFromDatabase, Asset expectedAsset, Guid folderId)
    {
        DateTime actualDate = DateTime.Now.Date;
        DateTime minDate = DateTime.MinValue.Date;

        Assert.That(assetFromDatabase.FileName, Is.EqualTo(expectedAsset.FileName));
        Assert.That(assetFromDatabase.FolderId, Is.EqualTo(folderId));
        Assert.That(assetFromDatabase.Folder.Path, Is.EqualTo(string.Empty));  // Not saved in Db, loaded at the runtime
        Assert.That(assetFromDatabase.FileProperties.Size, Is.EqualTo(0)); // Not saved in Db, loaded at the runtime
        Assert.That(assetFromDatabase.Pixel.Asset.Width, Is.EqualTo(expectedAsset.Pixel.Asset.Width));
        Assert.That(assetFromDatabase.Pixel.Asset.Height, Is.EqualTo(expectedAsset.Pixel.Asset.Height));
        Assert.That(assetFromDatabase.Pixel.Thumbnail.Width, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Width));
        Assert.That(assetFromDatabase.Pixel.Thumbnail.Height, Is.EqualTo(expectedAsset.Pixel.Thumbnail.Height));
        Assert.That(assetFromDatabase.ImageRotation, Is.EqualTo(expectedAsset.ImageRotation));
        Assert.That(assetFromDatabase.ThumbnailCreationDateTime.Date, Is.EqualTo(actualDate));
        Assert.That(assetFromDatabase.Hash, Is.EqualTo(expectedAsset.Hash));
        Assert.That(assetFromDatabase.Metadata.Corrupted.IsTrue, Is.EqualTo(expectedAsset.Metadata.Corrupted.IsTrue));
        Assert.That(assetFromDatabase.Metadata.Corrupted.Message, Is.EqualTo(string.Empty));
        Assert.That(assetFromDatabase.Metadata.Rotated.IsTrue, Is.EqualTo(expectedAsset.Metadata.Rotated.IsTrue));
        Assert.That(assetFromDatabase.Metadata.Rotated.Message, Is.EqualTo(string.Empty));
        Assert.That(assetFromDatabase.FullPath, Is.EqualTo(expectedAsset.FileName)); // Folder is not saved in Db, loaded at the runtime
        Assert.That(assetFromDatabase.ImageData, Is.Null); // Not saved in Db, loaded at the runtime
        Assert.That(assetFromDatabase.FileProperties.Creation.Date, Is.EqualTo(minDate)); // Not saved in Db, loaded at the runtime
        Assert.That(assetFromDatabase.FileProperties.Modification.Date, Is.EqualTo(minDate)); // Not saved in Db, loaded at the runtime
    }

    public static void CheckCatalogChangesInspectingFolder(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(folders, Has.Count.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder, Is.EqualTo(folders.First(x => x.Id == catalogChange.Folder!.Id)));
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderInspectionInProgress));
        Assert.That(catalogChange.Message, Is.EqualTo($"Inspecting folder {assetsDirectory}."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesFolderInspected(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderInspectionCompleted));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder inspection for {assetsDirectory}, subfolders included, has been completed."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesFolderAdded(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(folders, Has.Count.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder, Is.EqualTo(folders.First(x => x.Id == catalogChange.Folder!.Id)));
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder {assetsDirectory} added to catalog."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(foldersCount, Is.EqualTo(expectedFoldersCount));
        Assert.That(catalogChange.Folder, Is.Not.Null);
        Assert.That(catalogChange.Folder!.Path, Is.EqualTo(assetsDirectory));
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderDeleted));
        Assert.That(catalogChange.Message, Is.EqualTo($"Folder {assetsDirectory} deleted from catalog."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesAssetAdded(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Not.Null);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAsset.FullPath} added to catalog."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesAssetNotCreated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        IReadOnlyList<Asset> expectedAssets,
        string expectedAssetPath,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        int cataloguedAssetsByPathCount = catalogChange.CataloguedAssetsByPath.Count;
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(cataloguedAssetsByPathCount, Is.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetNotCreated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAssetPath} not added to catalog (corrupted)."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesAssetUpdated(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Not.Null);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetUpdated));
        Assert.That(catalogChange.Message, Is.EqualTo($"Image {expectedAsset.FullPath} updated in catalog."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesAssetDeleted(
        IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory,
        IReadOnlyList<Asset> expectedAssets,
        Asset expectedAsset,
        Folder folder,
        bool isCorrupted,
        ref int increment)
    {
        string expectedStatusMessage = isCorrupted ? $"Image {expectedAsset.FullPath} deleted from catalog (corrupted)." : $"Image {expectedAsset.FullPath} deleted from catalog.";

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Not.Null);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetDeleted));
        Assert.That(catalogChange.Message, Is.EqualTo(expectedStatusMessage));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    private static void AssertCataloguedAssetsByPath(IReadOnlyList<Asset> expectedAssets, CatalogChangeCallbackEventArgs catalogChange)
    {
        for (int i = 0; i < catalogChange.CataloguedAssetsByPath.Count; i++)
        {
            Asset currentExpectedAsset = expectedAssets[i];
            AssertAssetPropertyValidityAndImageData(catalogChange.CataloguedAssetsByPath[i], currentExpectedAsset, currentExpectedAsset.FullPath, currentExpectedAsset.Folder.Path, currentExpectedAsset.Folder);
        }
    }

    public static void CheckCatalogChangesBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string expectedMessage, ref int increment)
    {
        CatalogChangeReason catalogChangeReason = string.Equals(expectedMessage, CREATING_BACKUP_MESSAGE) ? CatalogChangeReason.BackupCreationStarted : CatalogChangeReason.BackupUpdateStarted;

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(catalogChangeReason));
        Assert.That(catalogChange.Message, Is.EqualTo(expectedMessage));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.BackupCompleted));
        Assert.That(catalogChange.Message, Is.EqualTo("Backup completed successfully."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesNoBackupChanges(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.NoBackupChangesDetected));
        Assert.That(catalogChange.Message, Is.EqualTo("No changes made to the backup."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessEnded));
        Assert.That(catalogChange.Message, Is.EqualTo("The catalog process has ended."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesException(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, Exception exceptionExpected, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessFailed));
        Assert.That(catalogChange.Message, Is.EqualTo("The catalog process has failed."));
        Assert.That(catalogChange.Exception, Is.Not.Null);
        Assert.That(catalogChange.Exception!.Message, Is.EqualTo(exceptionExpected.Message));
        Assert.That(catalogChange.Exception.GetType(), Is.EqualTo(exceptionExpected.GetType()));
        increment++;
    }

    public static void RemoveDatabaseBackup(
        List<Folder> folders,
        string blobsPath,
        string tablesPath,
        string backupFilePath)
    {

        // Delete all blobs in Blobs directory
        foreach (Folder folder in folders)
        {
            string blobFileName = $"{folder.Id}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            File.Delete(blobFilePath);
            Assert.That(File.Exists(blobFilePath), Is.False);
        }

        // Delete all tables in Tables directory
        string assetsTablePath = Path.Combine(tablesPath, Tables.ASSETS_DB);
        string foldersTablePath = Path.Combine(tablesPath, Tables.FOLDERS_DB);
        string syncAssetsDirectoriesDefinitionsTablePath = Path.Combine(tablesPath, Tables.SYNC_ASSETS_DIRECTORIES_DEFINITIONS_DB);
        string recentTargetPathsTablePath = Path.Combine(tablesPath, Tables.RECENT_TARGET_PATHS_DB);

        File.Delete(assetsTablePath);
        File.Delete(foldersTablePath);
        File.Delete(syncAssetsDirectoriesDefinitionsTablePath);
        File.Delete(recentTargetPathsTablePath);

        Assert.That(File.Exists(assetsTablePath), Is.False);
        Assert.That(File.Exists(foldersTablePath), Is.False);
        Assert.That(File.Exists(syncAssetsDirectoriesDefinitionsTablePath), Is.False);
        Assert.That(File.Exists(recentTargetPathsTablePath), Is.False);

        // Delete ZIP file in backup
        File.Delete(backupFilePath);
        Assert.That(File.Exists(backupFilePath), Is.False);
    }
}
