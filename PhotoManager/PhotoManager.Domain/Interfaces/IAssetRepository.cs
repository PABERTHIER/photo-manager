using System.Reactive;

namespace PhotoManager.Domain.Interfaces;

/// <summary>
/// High-performance asset repository backed by SQLite with immediate persistence.
/// All return types are concrete (arrays, HashSet) for maximum throughput
/// and zero virtual-dispatch overhead.
/// </summary>
public interface IAssetRepository
{
    // ----------------------------------------------------------------- Events

    /// <summary>
    /// Emits after any asset mutation (add / delete) completes and all
    /// internal state has been committed to SQLite.
    /// </summary>
    IObservable<Unit> AssetsUpdated { get; }

    // ----------------------------------------------------------------- Folders

    Folder AddFolder(string path);
    bool FolderExists(string path);
    Folder[] GetFolders();
    HashSet<string> GetFoldersPath();
    Folder[] GetSubFolders(Folder parentFolder);
    Folder? GetFolderByPath(string path);
    void DeleteFolder(Folder folder);

    // ----------------------------------------------------------------- Assets

    Asset[] GetAssetsByPath(string directory);
    bool AddAsset(Asset asset, byte[] thumbnailData);
    Asset? DeleteAsset(string directory, string deletedFileName);
    Asset[] GetCataloguedAssets();
    Asset[] GetCataloguedAssetsByPath(string directory);
    bool IsAssetCatalogued(string directoryName, string fileName);
    int GetAssetsCounter();

    // -------------------------------------------------------------- Thumbnails

    BitmapImage? LoadThumbnail(string directoryName, string fileName, int width, int height);

    // ----------------------------------------------------------------- Backups

    bool BackupExists();
    void WriteBackup();

    // ----------------------------------------------------------------- Config

    SyncAssetsConfiguration GetSyncAssetsConfiguration();
    void SaveSyncAssetsConfiguration(SyncAssetsConfiguration syncAssetsConfiguration);

    string[] GetRecentTargetPaths();
    void SaveRecentTargetPaths(string[] recentTargetPaths);
    void UpdateTargetPathToRecent(Folder destinationFolder);

    // ----------------------------------------------------------- Stored state

    string? GetStoredAssetsDirectory();
    void StoreAssetsDirectory(string path);

    // --------------------------------------------------------------- Vacuum

    void Vacuum();
}
