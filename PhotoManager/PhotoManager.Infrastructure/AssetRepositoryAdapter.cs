using System.Reactive;

namespace PhotoManager.Infrastructure;

/// <summary>
/// Migration adapter that implements <see cref="IAssetRepository"/> by forwarding
/// all calls to <see cref="OptimizedAssetRepository"/>. Use this as a drop-in
/// replacement for the legacy <see cref="AssetRepository"/> during phase 1:
/// swap the DI registration and leave all callers unchanged.
/// <para>
/// Phase 2: remove this adapter and let callers depend on
/// <see cref="IOptimizedAssetRepository"/>.
/// </para>
/// </summary>
public sealed class AssetRepositoryAdapter(IOptimizedAssetRepository inner) : IAssetRepository, IDisposable
{
    public IObservable<Unit> AssetsUpdated => inner.AssetsUpdated;

    public Asset[] GetAssetsByPath(string directory)
    {
        return inner.GetAssetsByPath(directory);
    }

    public void AddAsset(Asset asset, byte[] thumbnailData)
    {
        inner.AddAsset(asset, thumbnailData);
    }

    public Folder AddFolder(string path)
    {
        return inner.AddFolder(path);
    }

    public bool FolderExists(string path)
    {
        return inner.FolderExists(path);
    }

    public Folder[] GetFolders()
    {
        return inner.GetFolders();
    }

    public HashSet<string> GetFoldersPath()
    {
        return inner.GetFoldersPath();
    }

    public Folder[] GetSubFolders(Folder parentFolder)
    {
        return inner.GetSubFolders(parentFolder);
    }

    public Folder? GetFolderByPath(string path)
    {
        return inner.GetFolderByPath(path);
    }

    public bool BackupExists()
    {
        return inner.BackupExists();
    }

    public void WriteBackup()
    {
        inner.WriteBackup();
    }

    public List<Asset> GetCataloguedAssets()
    {
        return [.. inner.GetCataloguedAssets()];
    }

    public List<Asset> GetCataloguedAssetsByPath(string directory)
    {
        return [.. inner.GetCataloguedAssetsByPath(directory)];
    }

    public bool IsAssetCatalogued(string directoryName, string fileName)
    {
        return inner.IsAssetCatalogued(directoryName, fileName);
    }

    public Asset? DeleteAsset(string directory, string deletedFileName)
    {
        return inner.DeleteAsset(directory, deletedFileName);
    }

    public void DeleteFolder(Folder folder)
    {
        inner.DeleteFolder(folder);
    }

    public BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height)
    {
        return inner.LoadThumbnail(directoryName, fileName, width, height);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        return inner.GetSyncAssetsConfiguration();
    }

    public void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration)
    {
        inner.SaveSyncAssetsConfiguration(syncAssetsConfiguration);
    }

    public List<string> GetRecentTargetPaths()
    {
        return [.. inner.GetRecentTargetPaths()];
    }

    public void SaveRecentTargetPaths(List<string> recentTargetPaths)
    {
        inner.SaveRecentTargetPaths([.. recentTargetPaths]);
    }

    public void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        inner.UpdateTargetPathToRecent(destinationFolder);
    }

    public int GetAssetsCounter()
    {
        return inner.GetAssetsCounter();
    }

    public void Dispose()
    {
        inner.Dispose();
    }
}
