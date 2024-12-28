using PhotoManager.Domain;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Application;

public interface IApplication
{
    Asset[] GetAssetsByPath(string directory);
    int GetAssetsCounter();
    int GetTotalFilesCount();
    void LoadThumbnail(Asset asset);
    SyncAssetsConfiguration GetSyncAssetsConfiguration();
    void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration);
    Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback);
    Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null);
    void SetAsWallpaper(Asset asset, WallpaperStyle style);
    List<List<Asset>> GetDuplicatedAssets();
    void DeleteAssets(Asset[] assets);
    AboutInformation GetAboutInformation(Assembly assembly);
    Folder[] GetSubFolders(Folder parentFolder, bool includeHidden);
    string GetInitialFolder();
    ushort GetCatalogCooldownMinutes();
    bool GetSyncAssetsEveryXMinutes();
    string GetExemptedFolderPath();
    bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles);
    BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation);
    BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation);
    bool FileExists(string fullPath);
    List<string> GetRecentTargetPaths();
    Folder[] GetRootCatalogFolders();
}
