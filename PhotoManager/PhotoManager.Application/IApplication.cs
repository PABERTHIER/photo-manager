using PhotoManager.Common.Imaging;
using PhotoManager.Domain;
using System.Reflection;

namespace PhotoManager.Application;

public interface IApplication
{
    Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken token = default);
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
    IImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation);
    bool FileExists(string fullPath);
    int GetTotalFilesCount();
    bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles);
    void DeleteAssets(Asset[] assets);
    SyncAssetsConfiguration GetSyncAssetsConfiguration();
    void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration);
    Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback);
}
