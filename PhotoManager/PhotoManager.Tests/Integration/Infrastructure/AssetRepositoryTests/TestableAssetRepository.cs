using Microsoft.Extensions.Logging;
using PhotoManager.Domain.Interfaces.Persistence;

namespace PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;

public class TestableAssetRepository(
    IPathProviderService pathProviderService,
    IImageProcessingService imageProcessingService,
    IImageMetadataService imageMetadataService,
    IUserConfigurationService userConfigurationService,
    IPersistenceContext persistenceContext,
    ILogger<AssetRepository> logger)
    : AssetRepository(
        pathProviderService,
        imageProcessingService,
        imageMetadataService,
        userConfigurationService,
        persistenceContext,
        logger)
{
    public new IObservable<System.Reactive.Unit> AssetsUpdated => base.AssetsUpdated;

    public new Asset[] GetAssetsByPath(string directory) => base.GetAssetsByPath(directory);
    public new void AddAsset(Asset asset, byte[] thumbnailData) => base.AddAsset(asset, thumbnailData);
    public new Folder AddFolder(string path) => base.AddFolder(path);
    public new bool FolderExists(string path) => base.FolderExists(path);
    public new Folder[] GetFolders() => base.GetFolders();
    public new HashSet<string> GetFoldersPath() => base.GetFoldersPath();
    public new Folder[] GetSubFolders(Folder parentFolder) => base.GetSubFolders(parentFolder);
    public new Folder? GetFolderByPath(string path) => base.GetFolderByPath(path);
    public new bool BackupExists() => base.BackupExists();
    public new void WriteBackup() => base.WriteBackup();
    public new List<Asset> GetCataloguedAssets()
    {
        List<Asset> assets = [.. base.GetCataloguedAssets()];
        SortAssets(assets);
        return assets;
    }

    public new List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        List<Asset> assets = [.. base.GetCataloguedAssetsByPath(directory)];
        SortAssets(assets);
        return assets;
    }

    public new bool IsAssetCatalogued(string directoryName, string fileName) => base.IsAssetCatalogued(
        directoryName, fileName);

    public new Asset? DeleteAsset(string directory, string deletedFileName) => base.DeleteAsset(directory,
        deletedFileName);

    public new IReadOnlyList<Asset> DeleteAssets(string directory, IReadOnlyList<string> deletedFileNames) =>
        base.DeleteAssets(directory, deletedFileNames);

    public new void DeleteFolder(Folder folder) => base.DeleteFolder(folder);

    public new IImageData? LoadThumbnail(string directoryName, string fileName, int width, int height) =>
        base.LoadThumbnail(directoryName, fileName, width, height);

    public new SyncAssetsConfiguration GetSyncAssetsConfiguration() => base.GetSyncAssetsConfiguration();

    public new void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration) =>
        base.SaveSyncAssetsConfiguration(syncAssetsConfiguration);

    public new List<string> GetRecentTargetPaths() => [.. base.GetRecentTargetPaths()];

    public void SaveRecentTargetPaths(List<string> recentTargetPaths) => base.SaveRecentTargetPaths(
        [.. recentTargetPaths]);

    public new void UpdateTargetPathToRecent(Folder destinationFolder) => base.UpdateTargetPathToRecent(
        destinationFolder);

    public new int GetAssetsCounter() => base.GetAssetsCounter();

    public new string? GetStoredAssetsDirectory() => base.GetStoredAssetsDirectory();

    public new void StoreAssetsDirectory(string path) => base.StoreAssetsDirectory(path);

    public new void Vacuum() => base.Vacuum();

    private static void SortAssets(List<Asset> assets)
    {
        assets.Sort(static (a, b) =>
        {
            int compare = string.Compare(a.FileName, b.FileName, StringComparison.Ordinal);

            return compare != 0 ? compare : string.Compare(a.Folder.Path, b.Folder.Path, StringComparison.Ordinal);
        });
    }
}
