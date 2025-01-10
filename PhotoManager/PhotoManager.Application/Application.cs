using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Application;

public class Application(
    IAssetRepository assetRepository,
    ISyncAssetsService syncAssetsService,
    ICatalogAssetsService catalogAssetsService,
    IMoveAssetsService moveAssetsService,
    IFindDuplicatedAssetsService findDuplicatedAssetsService,
    IUserConfigurationService userConfigurationService,
    IStorageService storageService)
    : IApplication
{
    // Catalog
    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null) => await catalogAssetsService.CatalogAssetsAsync(callback, token);

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets() => findDuplicatedAssetsService.GetDuplicatedAssets();

    // AssetRepository
    public Asset[] GetAssetsByPath(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be null or empty.");
        }

        if (!assetRepository.FolderExists(directory))
        {
            assetRepository.AddFolder(directory);
        }

        return assetRepository.GetAssetsByPath(directory);
    }

    public Folder[] GetRootCatalogFolders()
    {
        string[] paths = userConfigurationService.GetRootCatalogFolderPaths();
        Folder?[] folders = new Folder[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            folders[i] = assetRepository.GetFolderByPath(paths[i]) ?? assetRepository.AddFolder(paths[i]);
        }

        return folders!;
    }

    public void LoadThumbnail(Asset asset)
    {
        asset.ImageData = assetRepository.LoadThumbnail(asset.Folder.Path, asset.FileName, asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
    }

    public Folder[] GetSubFolders(Folder parentFolder) => assetRepository.GetSubFolders(parentFolder);

    public List<string> GetRecentTargetPaths() => assetRepository.GetRecentTargetPaths();

    public int GetAssetsCounter() => assetRepository.GetAssetsCounter();

    // UserConfigurationService
    public string GetInitialFolderPath() => userConfigurationService.PathSettings.AssetsDirectory;

    public ushort GetCatalogCooldownMinutes() => userConfigurationService.AssetSettings.CatalogCooldownMinutes;

    public bool GetSyncAssetsEveryXMinutes() => userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes;

    public string GetExemptedFolderPath() => userConfigurationService.PathSettings.ExemptedFolderPath;

    public AboutInformation GetAboutInformation(Assembly assembly) => userConfigurationService.GetAboutInformation(assembly);

    // StorageService
    public BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation) => storageService.LoadBitmapImageFromPath(imagePath, rotation);
    public BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation) => storageService.LoadBitmapHeicImageFromPath(imagePath, rotation);
    public bool FileExists(string fullPath) => storageService.FileExists(fullPath);
    public int GetTotalFilesCount() => storageService.GetTotalFilesCount();

    // MoveAssetsService
    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles) => moveAssetsService.MoveAssets(assets, destinationFolder, preserveOriginalFiles);
    public void DeleteAssets(Asset[] assets) => moveAssetsService.DeleteAssets(assets);

    // Sync
    public SyncAssetsConfiguration GetSyncAssetsConfiguration() => assetRepository.GetSyncAssetsConfiguration();
    public void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration)
    {
        syncConfiguration.Validate().Normalize();
        assetRepository.SaveSyncAssetsConfiguration(syncConfiguration);
        assetRepository.SaveCatalog(null);
    }
    public async Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback) => await syncAssetsService.ExecuteAsync(callback);
}
