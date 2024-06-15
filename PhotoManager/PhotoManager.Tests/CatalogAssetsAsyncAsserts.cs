using System.IO.Compression;

namespace PhotoManager.Tests;

public static class CatalogAssetsAsyncAsserts
{
    public const string CREATING_BACKUP_MESSAGE = "Creating catalog backup...";
    public const string UPDATING_BACKUP_MESSAGE = "Updating catalog backup...";

    public static void CheckBlobsAndTablesBeforeSaveCatalog(string blobsPath, string tablesPath)
    {
        string[] blobFiles = Directory.GetFiles(blobsPath);
        string[] tableFiles = Directory.GetFiles(tablesPath);

        Assert.IsEmpty(blobFiles);
        Assert.IsEmpty(tableFiles);

        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsFalse(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
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

        Assert.AreEqual(foldersContainingAssets.Count, blobFiles.Length);

        foreach (Folder folder in foldersContainingAssets)
        {
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            Assert.IsTrue(File.Exists(blobFilePath));

            List<Asset> assetsFromRepositoryByFolder = assetsFromRepository.Where(x => x.FolderId == folder.FolderId).ToList();

            Dictionary<string, byte[]>? dataRead = blobStorage.ReadFromBinaryFile(blobFilePath);
            Assert.IsNotNull(dataRead);
            Assert.AreEqual(assetsFromRepositoryByFolder.Count, dataRead!.Count);

            for (int i = 0; i < dataRead.Count; i++)
            {
                Assert.IsTrue(dataRead.ContainsKey(assetsFromRepositoryByFolder[i].FileName));
                Assert.IsTrue(assetNameToByteSizeMapping.ContainsKey(assetsFromRepositoryByFolder[i].FileName));
                Assert.AreEqual(assetNameToByteSizeMapping[assetsFromRepositoryByFolder[i].FileName], dataRead[assetsFromRepositoryByFolder[i].FileName].Length);
            }
        }

        Assert.AreEqual(4, tableFiles.Length);
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
        Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));

        List<Asset> assetsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.AreEqual(assetsFromRepository.Count, assetsFromDatabase.Count);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        foreach (Asset assetFromDatabase in assetsFromDatabase)
        {
            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == assetFromDatabase.FileName && a.FolderId == assetFromDatabase.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            AssertAssetFromDatabaseValidity(assetFromDatabase, expectedAsset, expectedFolder.FolderId);
        }

        Assert.AreEqual(folders.Count, foldersFromDatabase.Count);

        Dictionary<Guid, Folder> foldersById = folders.ToDictionary(f => f.FolderId, f => f);
        Dictionary<Guid, Folder> foldersFromDatabaseById = foldersFromDatabase.ToDictionary(f => f.FolderId, f => f);

        foreach ((Guid folderId, Folder? expectedFolder) in foldersById)
        {
            Folder actualFolder = foldersFromDatabaseById[folderId];

            Assert.AreEqual(expectedFolder.FolderId, actualFolder.FolderId);
            Assert.AreEqual(expectedFolder.Path, actualFolder.Path);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
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

        Assert.IsEmpty(blobFiles);

        if (hasEmptyTables)
        {
            Assert.AreEqual(4, tableFiles.Length);
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "assets.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "folders.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db")));
            Assert.IsTrue(File.Exists(Path.Combine(tablesPath, "recenttargetpaths.db")));
        }
        else
        {
            Assert.IsEmpty(tableFiles);
        }

        List<Asset> assetsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.AssetsTableName, AssetConfigs.ReadFunc);
        List<Folder> foldersFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.FoldersTableName, FolderConfigs.ReadFunc);
        List<SyncAssetsDirectoriesDefinition> syncAssetsDirectoriesDefinitionsFromDatabase =
            database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.SyncAssetsDirectoriesDefinitionsTableName, SyncAssetsDirectoriesDefinitionConfigs.ReadFunc);
        List<string> recentTargetPathsFromDatabase = database.ReadObjectList(userConfigurationService.StorageSettings.TablesSettings.RecentTargetPathsTableName, RecentPathsConfigs.ReadFunc);

        Assert.IsEmpty(assetsFromDatabase);

        if (hasOneFolder)
        {
            Assert.AreEqual(1, foldersFromDatabase.Count);
            Assert.AreEqual(folder.FolderId, foldersFromDatabase[0].FolderId);
            Assert.AreEqual(folder.Path, foldersFromDatabase[0].Path);
        }
        else
        {
            Assert.IsEmpty(foldersFromDatabase);
        }

        Assert.IsEmpty(syncAssetsDirectoriesDefinitionsFromDatabase);
        Assert.IsEmpty(recentTargetPathsFromDatabase);
    }

    public static void CheckBackupBefore(IAssetRepository testableAssetRepository, string backupFilePath)
    {
        Assert.IsFalse(File.Exists(backupFilePath));
        Assert.IsFalse(testableAssetRepository.BackupExists());
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

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, databaseBackupPath);
        Assert.IsTrue(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(databasePath);
        string[] backupDirectories = Directory.GetDirectories(databaseBackupPath);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(foldersContainingAssets.Count, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(4, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(foldersContainingAssets.Count, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(4, tablesBackup.Length);

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

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));
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

        Assert.IsFalse(Directory.Exists(backupBlobsDirectory));
        Assert.IsFalse(Directory.Exists(backupTablesDirectory));

        ZipFile.ExtractToDirectory(backupFilePath, databaseBackupPath);
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));

        Assert.IsTrue(Directory.Exists(backupBlobsDirectory));
        Assert.IsTrue(Directory.Exists(backupTablesDirectory));

        string[] sourceDirectories = Directory.GetDirectories(databasePath);
        string[] backupDirectories = Directory.GetDirectories(databaseBackupPath);

        Assert.AreEqual(sourceDirectories.Length, backupDirectories.Length);

        Assert.AreEqual(sourceDirectories[0], blobsPath);
        string[] blobs = Directory.GetFiles(blobsPath);
        Assert.AreEqual(0, blobs.Length);

        Assert.AreEqual(sourceDirectories[1], tablesPath);
        string[] tables = Directory.GetFiles(tablesPath);
        Assert.AreEqual(expectedTablesCount, tables.Length);

        Assert.AreEqual(backupDirectories[0], backupBlobsDirectory);
        string[] blobsBackup = Directory.GetFiles(backupBlobsDirectory);
        Assert.AreEqual(0, blobsBackup.Length);

        Assert.AreEqual(backupDirectories[1], backupTablesDirectory);
        string[] tablesBackup = Directory.GetFiles(backupTablesDirectory);
        Assert.AreEqual(expectedTablesCount, tablesBackup.Length);

        CheckBlobsAndTablesAfterSaveCatalogEmpty(database, userConfigurationService, backupBlobsDirectory, backupTablesDirectory, hasEmptyTables, hasOneFolder, folder);
    }

    public static void AssertAssetPropertyValidityAndImageData(Asset asset, Asset expectedAsset, string assetPath, string folderPath, Folder folder)
    {
        AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.IsNull(asset.ImageData); // Set above, not in this method
    }

    public static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath, string folderPath,
        Folder folder)
    {
        DateTime actualDate = DateTime.Now.Date;

        Assert.AreEqual(expectedAsset.FileName, asset.FileName);
        Assert.AreEqual(folder.FolderId, asset.FolderId);
        Assert.AreEqual(folder, asset.Folder);
        Assert.AreEqual(expectedAsset.FileSize, asset.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, asset.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, asset.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, asset.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, asset.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, asset.ImageRotation);
        Assert.AreEqual(actualDate, asset.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, asset.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, asset.IsAssetCorrupted);
        Assert.AreEqual(expectedAsset.AssetCorruptedMessage, asset.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, asset.IsAssetRotated);
        Assert.AreEqual(expectedAsset.AssetRotatedMessage, asset.AssetRotatedMessage);
        Assert.AreEqual(assetPath, asset.FullPath);
        Assert.AreEqual(folderPath, asset.Folder.Path);
        Assert.AreEqual(actualDate, asset.FileCreationDateTime.Date); // Because files are generated by tests (thumbnailCreationDateTime is then the FileModificationDateTime value)  
        Assert.AreEqual(expectedAsset.ThumbnailCreationDateTime.Date, asset.FileModificationDateTime.Date);  // Unlike in AssetCreationServiceTests it is set here
    }

    public static void AssertThumbnailsValidity(
        IReadOnlyList<Asset> assetsFromRepository,
        Dictionary<Folder, List<Asset>> folderToAssetsMapping,
        IReadOnlyList<Folder> folders,
        Dictionary<string, Dictionary<string, byte[]>> thumbnails,
        IReadOnlyList<int> assetsImageByteSize)
    {
        Assert.AreEqual(folders.Count, thumbnails.Count);

        int thumbnailsTotalCount = 0;

        for (int i = 0; i < thumbnails.Count; i++)
        {
            Assert.IsTrue(thumbnails.ContainsKey(folders[i].Path));
            thumbnailsTotalCount += thumbnails[folders[i].Path].Count;
        }

        Assert.AreEqual(assetsFromRepository.Count, thumbnailsTotalCount);

        Dictionary<Asset, Folder> assetToFolderMapping = folderToAssetsMapping.SelectMany(kv =>
            kv.Value.Select(a => (Asset: a, Folder: kv.Key))).ToDictionary(x => x.Asset, x => x.Folder);

        for (int i = 0; i < assetsFromRepository.Count; i++)
        {
            Asset currentAsset = assetsFromRepository[i];

            Asset expectedAsset = assetToFolderMapping.Keys.First(a => a.FileName == currentAsset.FileName && a.FolderId == currentAsset.FolderId);
            Folder expectedFolder = assetToFolderMapping[expectedAsset];

            Assert.IsTrue(thumbnails[expectedFolder.Path].ContainsKey(currentAsset.FileName));

            byte[] assetImageByteSize = thumbnails[expectedFolder.Path][currentAsset.FileName];

            Assert.IsNotNull(assetImageByteSize);
            Assert.AreEqual(assetsImageByteSize[i], assetImageByteSize.Length);
        }
    }

    private static void AssertAssetFromDatabaseValidity(Asset assetFromDatabase, Asset expectedAsset, Guid folderId)
    {
        DateTime actualDate = DateTime.Now.Date;
        DateTime minDate = DateTime.MinValue.Date;

        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FileName);
        Assert.AreEqual(folderId, assetFromDatabase.FolderId);
        Assert.IsNull(assetFromDatabase.Folder);  // Not saved in Db, loaded at the runtime
        Assert.AreEqual(expectedAsset.FileSize, assetFromDatabase.FileSize);
        Assert.AreEqual(expectedAsset.PixelWidth, assetFromDatabase.PixelWidth);
        Assert.AreEqual(expectedAsset.PixelHeight, assetFromDatabase.PixelHeight);
        Assert.AreEqual(expectedAsset.ThumbnailPixelWidth, assetFromDatabase.ThumbnailPixelWidth);
        Assert.AreEqual(expectedAsset.ThumbnailPixelHeight, assetFromDatabase.ThumbnailPixelHeight);
        Assert.AreEqual(expectedAsset.ImageRotation, assetFromDatabase.ImageRotation);
        Assert.AreEqual(actualDate, assetFromDatabase.ThumbnailCreationDateTime.Date);
        Assert.AreEqual(expectedAsset.Hash, assetFromDatabase.Hash);
        Assert.AreEqual(expectedAsset.IsAssetCorrupted, assetFromDatabase.IsAssetCorrupted);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetCorruptedMessage);
        Assert.AreEqual(expectedAsset.IsAssetRotated, assetFromDatabase.IsAssetRotated);
        Assert.AreEqual(string.Empty, assetFromDatabase.AssetRotatedMessage);
        Assert.AreEqual(expectedAsset.FileName, assetFromDatabase.FullPath); // Folder is not saved in Db, loaded at the runtime
        Assert.IsNull(assetFromDatabase.ImageData); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileCreationDateTime.Date); // Not saved in Db, loaded at the runtime
        Assert.AreEqual(minDate, assetFromDatabase.FileModificationDateTime.Date); // Not saved in Db, loaded at the runtime
    }

    public static void CheckCatalogChangesInspectingFolder(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.FolderInspectionInProgress, catalogChange.Reason);
        Assert.AreEqual($"Inspecting folder {assetsDirectory}.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesFolderInspected(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.FolderInspectionCompleted, catalogChange.Reason);
        Assert.AreEqual($"Folder inspection for {assetsDirectory}, subfolders included, has been completed.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesFolderAdded(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, folders.Count);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(folders.First(x => x.FolderId == catalogChange.Folder!.FolderId), catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.FolderCreated, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.AreEqual(expectedFoldersCount, foldersCount);
        Assert.IsNotNull(catalogChange.Folder);
        Assert.AreEqual(assetsDirectory, catalogChange.Folder!.Path);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.FolderDeleted, catalogChange.Reason);
        Assert.AreEqual($"Folder {assetsDirectory} deleted from catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
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
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.AreEqual(ReasonEnum.AssetCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} added to catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
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
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, cataloguedAssetsByPathCount);
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.AreEqual(ReasonEnum.AssetNotCreated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAssetPath} not added to catalog (corrupted).", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
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
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.AreEqual(ReasonEnum.AssetUpdated, catalogChange.Reason);
        Assert.AreEqual($"Image {expectedAsset.FullPath} updated in catalog.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
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
        Assert.IsNotNull(catalogChange.Asset);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath, assetsDirectory, folder);
        Assert.IsNull(catalogChange.Folder);
        Assert.AreEqual(expectedAssets.Count, catalogChange.CataloguedAssetsByPath.Count);
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.AreEqual(ReasonEnum.AssetDeleted, catalogChange.Reason);
        Assert.AreEqual(expectedStatusMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
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
        ReasonEnum reason = string.Equals(expectedMessage, CREATING_BACKUP_MESSAGE) ? ReasonEnum.BackupCreationStarted : ReasonEnum.BackupUpdateStarted;

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(reason, catalogChange.Reason);
        Assert.AreEqual(expectedMessage, catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;

        catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.BackupCompleted, catalogChange.Reason);
        Assert.AreEqual("Backup completed successfully.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesNoBackupChanges(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.NoBackupChangesDetected, catalogChange.Reason);
        Assert.AreEqual("No changes made to the backup.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.CatalogProcessEnded, catalogChange.Reason);
        Assert.AreEqual("The catalog process has ended.", catalogChange.Message);
        Assert.IsNull(catalogChange.Exception);
        increment++;
    }

    public static void CheckCatalogChangesException(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges, Exception exceptionExpected, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.IsNull(catalogChange.Asset);
        Assert.IsNull(catalogChange.Folder);
        Assert.IsEmpty(catalogChange.CataloguedAssetsByPath);
        Assert.AreEqual(ReasonEnum.CatalogProcessFailed, catalogChange.Reason);
        Assert.AreEqual("The catalog process has failed.", catalogChange.Message);
        Assert.IsNotNull(catalogChange.Exception);
        Assert.AreEqual(exceptionExpected.Message, catalogChange.Exception!.Message);
        Assert.AreEqual(exceptionExpected.GetType(), catalogChange.Exception.GetType());
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
            string blobFileName = $"{folder.FolderId}.bin";
            string blobFilePath = Path.Combine(blobsPath, blobFileName);

            File.Delete(blobFilePath);
            Assert.IsFalse(File.Exists(blobFilePath));
        }

        // Delete all tables in Tables directory
        string assetsTablePath = Path.Combine(tablesPath, "assets.db");
        string foldersTablePath = Path.Combine(tablesPath, "folders.db");
        string syncAssetsDirectoriesDefinitionsTablePath = Path.Combine(tablesPath, "syncassetsdirectoriesdefinitions.db");
        string recentTargetPathsTablePath = Path.Combine(tablesPath, "recenttargetpaths.db");

        File.Delete(assetsTablePath);
        File.Delete(foldersTablePath);
        File.Delete(syncAssetsDirectoriesDefinitionsTablePath);
        File.Delete(recentTargetPathsTablePath);

        Assert.IsFalse(File.Exists(assetsTablePath));
        Assert.IsFalse(File.Exists(foldersTablePath));
        Assert.IsFalse(File.Exists(syncAssetsDirectoriesDefinitionsTablePath));
        Assert.IsFalse(File.Exists(recentTargetPathsTablePath));

        // Delete ZIP file in backup
        File.Delete(backupFilePath);
        Assert.IsFalse(File.Exists(backupFilePath));
    }
}