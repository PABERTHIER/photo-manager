using PhotoManager.Tests.Integration;
using System.IO.Compression;

namespace PhotoManager.Tests;

public static class CatalogAssetsAsyncAsserts
{
    public const string CREATING_BACKUP_MESSAGE = "Creating catalog backup...";
    public const string UPDATING_BACKUP_MESSAGE = "Updating catalog backup...";

    public static void CheckBackupBefore(IAssetRepository testableAssetRepository, string backupFilePath)
    {
        Assert.That(File.Exists(backupFilePath), Is.False);
        Assert.That(testableAssetRepository.BackupExists(), Is.False);
    }

    public static void CheckBackupAfter(string databaseBackupPath, string backupFilePath)
    {
        string databaseBackupFilePath = Path.Combine(databaseBackupPath, Constants.DATABASE_FILE_NAME);

        Assert.That(File.Exists(backupFilePath), Is.True);
        ZipFile.ExtractToDirectory(backupFilePath, databaseBackupPath);

        Assert.That(File.Exists(databaseBackupFilePath), Is.True);
        File.Delete(databaseBackupFilePath);
    }

    public static void AssertAssetsPropertyValidityAndImageData(List<Asset> assets, List<Asset> expectedAssets,
        List<string> assetPaths, string folderPath, Folder folder)
    {
        foreach (Asset asset in assets)
        {
            Asset expectedAsset = expectedAssets.First(x => x.FileName == asset.FileName);
            string assetPath = assetPaths.First(x => x == asset.FullPath);

            AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
            Assert.That(asset.ImageData, Is.Null); // Set above, not in this method
        }
    }

    public static void AssertAssetsPropertyValidityAndImageData(List<Asset> assets, List<Asset> expectedAssets,
        List<string> assetPaths, List<string> folderPath, List<Folder> folders)
    {
        foreach (Asset asset in assets)
        {
            Asset expectedAsset = expectedAssets.First(x => x.FileName == asset.FileName);
            string assetPath = assetPaths.First(x => x == asset.FullPath);
            string expectedFolderPath = folderPath.First(x => x == expectedAsset.Folder.Path);
            Folder expectedFolder = folders.First(x => x.Path == expectedAsset.Folder.Path);

            AssertAssetPropertyValidity(asset, expectedAsset, assetPath, expectedFolderPath, expectedFolder);
            Assert.That(asset.ImageData, Is.Null); // Set above, not in this method
        }
    }

    public static void AssertAssetPropertyValidityAndImageData(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
    {
        AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        Assert.That(asset.ImageData, Is.Null); // Set above, not in this method
    }

    public static void AssertAssetsPropertyValidity(List<Asset> assets, List<Asset> expectedAssets,
        List<string> assetPaths, string folderPath, Folder folder)
    {
        foreach (Asset asset in assets)
        {
            Asset expectedAsset = expectedAssets.First(x => x.FileName == asset.FileName);
            string assetPath = assetPaths.First(x => x == asset.FullPath);

            AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        }
    }

    public static void AssertAssetsPropertyValidity(List<Asset> assets, List<Asset> expectedAssets,
        List<string> assetPaths, List<string> folderPaths, List<Folder> folders)
    {
        foreach (Asset asset in assets)
        {
            Asset expectedAsset = expectedAssets.First(x => x.FileName == asset.FileName);
            string assetPath = assetPaths.First(x => x == asset.FullPath);
            string folderPath = folderPaths.First(x => x == expectedAsset.Folder.Path);
            Folder folder = folders.First(x => x.Path == expectedAsset.Folder.Path);

            AssertAssetPropertyValidity(asset, expectedAsset, assetPath, folderPath, folder);
        }
    }

    public static void AssertAssetPropertyValidity(Asset asset, Asset expectedAsset, string assetPath,
        string folderPath, Folder folder)
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
        Assert.That(asset.FileProperties.Creation.Date,
            Is.EqualTo(actualDate)); // Because files are generated by tests (ThumbnailCreationDateTime and FileModificationDateTime have the same value)
        Assert.That(asset.FileProperties.Modification.Date, Is.EqualTo(expectedAsset.FileProperties.Modification.Date));
    }

    public static void CheckCatalogChangesInspectingFolder(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
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

    public static void CheckCatalogChangesFolderInspected(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string assetsDirectory, ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.FolderInspectionCompleted));
        Assert.That(catalogChange.Message,
            Is.EqualTo($"Folder inspection for {assetsDirectory}, subfolders included, has been completed."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesFolderAdded(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        int expectedFoldersCount, IReadOnlyCollection<Folder> folders, string assetsDirectory, ref int increment)
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

    public static void CheckCatalogChangesFolderDeleted(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        int expectedFoldersCount, int foldersCount, string assetsDirectory, ref int increment)
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
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath,
            assetsDirectory, folder);
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
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath,
            assetsDirectory, folder);
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
        string expectedStatusMessage = isCorrupted
            ? $"Image {expectedAsset.FullPath} deleted from catalog (corrupted)."
            : $"Image {expectedAsset.FullPath} deleted from catalog.";

        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Not.Null);
        AssertAssetPropertyValidityAndImageData(catalogChange.Asset!, expectedAsset, expectedAsset.FullPath,
            assetsDirectory, folder);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Has.Count.EqualTo(expectedAssets.Count));
        AssertCataloguedAssetsByPath(expectedAssets, catalogChange);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.AssetDeleted));
        Assert.That(catalogChange.Message, Is.EqualTo(expectedStatusMessage));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    private static void AssertCataloguedAssetsByPath(IReadOnlyList<Asset> expectedAssets,
        CatalogChangeCallbackEventArgs catalogChange)
    {
        foreach (Asset cataloguedAssetByPath in catalogChange.CataloguedAssetsByPath)
        {
            Asset expectedAsset = expectedAssets.First(x => x.FileName == cataloguedAssetByPath.FileName);
            AssertAssetPropertyValidityAndImageData(cataloguedAssetByPath, expectedAsset,
                expectedAsset.FullPath, expectedAsset.Folder.Path, expectedAsset.Folder);
        }
    }

    public static void CheckCatalogChangesBackup(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        string expectedMessage, ref int increment)
    {
        CatalogChangeReason catalogChangeReason = string.Equals(expectedMessage, CREATING_BACKUP_MESSAGE)
            ? CatalogChangeReason.BackupCreationStarted
            : CatalogChangeReason.BackupUpdateStarted;

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

    public static void CheckCatalogChangesNoBackupChanges(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        ref int increment)
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

    public static void CheckCatalogChangesCancelled(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        ref int increment)
    {
        CatalogChangeCallbackEventArgs catalogChange = catalogChanges[increment];
        Assert.That(catalogChange.Asset, Is.Null);
        Assert.That(catalogChange.Folder, Is.Null);
        Assert.That(catalogChange.CataloguedAssetsByPath, Is.Empty);
        Assert.That(catalogChange.Reason, Is.EqualTo(CatalogChangeReason.CatalogProcessCancelled));
        Assert.That(catalogChange.Message, Is.EqualTo("The catalog process has been cancelled."));
        Assert.That(catalogChange.Exception, Is.Null);
        increment++;
    }

    public static void CheckCatalogChangesEnd(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        ref int increment)
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

    public static void CheckCatalogChangesException(IReadOnlyList<CatalogChangeCallbackEventArgs> catalogChanges,
        Exception exceptionExpected, ref int increment)
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

    public static void RemoveDatabaseBackup(string backupFilePath)
    {
        // Delete ZIP file in backup
        File.Delete(backupFilePath);
        Assert.That(File.Exists(backupFilePath), Is.False);
    }
}
