using PhotoManager.Domain;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Application;

public interface IApplication
{
    Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null);
    List<List<Asset>> GetDuplicatedAssets();
    Asset[] GetAssetsByPath(string directory);
    Folder[] GetRootCatalogFolders();
    void LoadThumbnail(Asset asset);
    Folder[] GetSubFolders(Folder parentFolder);
    List<string> GetRecentTargetPaths();
    int GetAssetsCounter();
    string GetInitialFolderPath();
    ushort GetCatalogCooldownMinutes();
    bool GetSyncAssetsEveryXMinutes();
    string GetExemptedFolderPath();
    AboutInformation GetAboutInformation(Assembly assembly);
    BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation);
    BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation);
    bool FileExists(string fullPath);
    int GetTotalFilesCount();
    bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles);
    void DeleteAssets(Asset[] assets);
    SyncAssetsConfiguration GetSyncAssetsConfiguration();
    void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration);
    Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback);
}
