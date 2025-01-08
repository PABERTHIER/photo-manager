using PhotoManager.Domain;
using PhotoManager.Domain.Interfaces;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PhotoManager.Application;

public class Application : IApplication
{
    private readonly IAssetRepository _assetRepository;
    private readonly ISyncAssetsService _syncAssetsService;
    private readonly ICatalogAssetsService _catalogAssetsService;
    private readonly IMoveAssetsService _moveAssetsService;
    private readonly IFindDuplicatedAssetsService _findDuplicatedAssetsService;
    private readonly IUserConfigurationService _userConfigurationService;
    private readonly IStorageService _storageService;

    public Application(
        IAssetRepository assetRepository,
        ISyncAssetsService syncAssetsService,
        ICatalogAssetsService catalogAssetsService,
        IMoveAssetsService moveAssetsService,
        IFindDuplicatedAssetsService findDuplicatedAssetsService,
        IUserConfigurationService userConfigurationService,
        IStorageService storageService)
    {
        _assetRepository = assetRepository;
        _syncAssetsService = syncAssetsService;
        _catalogAssetsService = catalogAssetsService;
        _moveAssetsService = moveAssetsService;
        _findDuplicatedAssetsService = findDuplicatedAssetsService;
        _userConfigurationService = userConfigurationService;
        _storageService = storageService;
    }

    // Catalog
    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null) => await _catalogAssetsService.CatalogAssetsAsync(callback, token);

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets() => _findDuplicatedAssetsService.GetDuplicatedAssets();

    // AssetRepository
    public Asset[] GetAssetsByPath(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be null or empty.");
        }

        if (!_assetRepository.FolderExists(directory))
        {
            _assetRepository.AddFolder(directory);
        }

        return _assetRepository.GetAssetsByPath(directory);
    }

    public Folder[] GetRootCatalogFolders()
    {
        string[] paths = _userConfigurationService.GetRootCatalogFolderPaths();
        Folder[] folders = new Folder[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            folders[i] = _assetRepository.GetFolderByPath(paths[i]);

            if (folders[i] == null)
            {
                folders[i] = _assetRepository.AddFolder(paths[i]);
            }
        }

        return folders;
    }

    public void LoadThumbnail(Asset asset)
    {
        asset.ImageData = _assetRepository.LoadThumbnail(asset.Folder.Path, asset.FileName, asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
    }

    public Folder[] GetSubFolders(Folder parentFolder) => _assetRepository.GetSubFolders(parentFolder);

    public List<string> GetRecentTargetPaths() => _assetRepository.GetRecentTargetPaths();

    public int GetAssetsCounter() => _assetRepository.GetAssetsCounter();

    // UserConfigurationService
    public string GetInitialFolderPath() => _userConfigurationService.PathSettings.AssetsDirectory;

    public ushort GetCatalogCooldownMinutes() => _userConfigurationService.AssetSettings.CatalogCooldownMinutes;

    public bool GetSyncAssetsEveryXMinutes() => _userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes;

    public string GetExemptedFolderPath() => _userConfigurationService.PathSettings.ExemptedFolderPath;

    public AboutInformation GetAboutInformation(Assembly assembly) => _userConfigurationService.GetAboutInformation(assembly);

    // StorageService
    public BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation) => _storageService.LoadBitmapImageFromPath(imagePath, rotation);
    public BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation) => _storageService.LoadBitmapHeicImageFromPath(imagePath, rotation);
    public bool FileExists(string fullPath) => _storageService.FileExists(fullPath);
    public int GetTotalFilesCount() => _storageService.GetTotalFilesCount();

    // MoveAssetsService
    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles) => _moveAssetsService.MoveAssets(assets, destinationFolder, preserveOriginalFiles);
    public void DeleteAssets(Asset[] assets) => _moveAssetsService.DeleteAssets(assets);

    // Sync
    public SyncAssetsConfiguration GetSyncAssetsConfiguration() => _assetRepository.GetSyncAssetsConfiguration();
    public void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration)
    {
        syncConfiguration.Validate().Normalize();
        _assetRepository.SaveSyncAssetsConfiguration(syncConfiguration);
        _assetRepository.SaveCatalog(null);
    }
    public async Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback) => await _syncAssetsService.ExecuteAsync(callback);

    // Wallpaper
    public void SetAsWallpaper(Asset asset, WallpaperStyle style)
    {
        if (asset != null)
        {
            _userConfigurationService.SetAsWallpaper(asset, style);
        }
    }
}
