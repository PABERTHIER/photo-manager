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
        ISyncAssetsService syncAssetsService,
        ICatalogAssetsService catalogAssetsService,
        IMoveAssetsService moveAssetsService,
        IFindDuplicatedAssetsService findDuplicatedAssetsService,
        IAssetRepository assetRepository,
        IUserConfigurationService userConfigurationService,
        IStorageService storageService)
    {
        _syncAssetsService = syncAssetsService;
        _catalogAssetsService = catalogAssetsService;
        _moveAssetsService = moveAssetsService;
        _findDuplicatedAssetsService = findDuplicatedAssetsService;
        _assetRepository = assetRepository;
        _userConfigurationService = userConfigurationService;
        _storageService = storageService;
    }

    public Asset[] GetAssets(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be null or empty.", directory);
        }

        if (!_assetRepository.FolderExists(directory))
        {
            _assetRepository.AddFolder(directory);
        }

        return _assetRepository.GetAssetsByPath(directory);
    }

    public int GetAssetsCounter()
    {
        return _assetRepository.GetAssetsCounter();
    }

    public int GetTotalFilesNumber()
    {
        return _catalogAssetsService.GetTotalFilesNumber();
    }

    public void LoadThumbnail(Asset asset)
    {
        asset.ImageData = _assetRepository.LoadThumbnail(asset.Folder.Path, asset.FileName, asset.ThumbnailPixelWidth, asset.ThumbnailPixelHeight);
    }

    public SyncAssetsConfiguration GetSyncAssetsConfiguration()
    {
        return _assetRepository.GetSyncAssetsConfiguration();
    }

    public void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration)
    {
        syncConfiguration.Validate();
        syncConfiguration.Normalize();
        _assetRepository.SaveSyncAssetsConfiguration(syncConfiguration);
        _assetRepository.SaveCatalog(null);
    }

    public async Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback) => await _syncAssetsService.ExecuteAsync(callback);

    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken? token = null) => await _catalogAssetsService.CatalogAssetsAsync(callback, token);

    public void SetAsWallpaper(Asset asset, WallpaperStyle style)
    {
        if (asset != null)
        {
            _userConfigurationService.SetAsWallpaper(asset, style);
        }
    }

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>A list of duplicated sets of assets (corresponding to the same image),
    /// where each item is a list of duplicated assets.</returns>
    public List<List<Asset>> GetDuplicatedAssets() => _findDuplicatedAssetsService.GetDuplicatedAssets();

    public void DeleteAssets(Asset[] assets, bool saveCatalog) => _moveAssetsService.DeleteAssets(assets, saveCatalog);

    public AboutInformation GetAboutInformation(Assembly assembly) => _userConfigurationService.GetAboutInformation(assembly);

    public Folder[] GetSubFolders(Folder parentFolder, bool includeHidden) => _assetRepository.GetSubFolders(parentFolder, includeHidden);

    public string GetInitialFolder() => _userConfigurationService.GetPicturesDirectory();

    public int GetCatalogCooldownMinutes() => _userConfigurationService.GetCatalogCooldownMinutes();

    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles) => _moveAssetsService.MoveAssets(assets, destinationFolder, preserveOriginalFiles);

    public BitmapImage LoadBitmapImageFromPath(string imagePath, Rotation rotation) => _storageService.LoadBitmapImageFromPath(imagePath, rotation);
    public BitmapImage LoadBitmapHeicImageFromPath(string imagePath, Rotation rotation) => _storageService.LoadBitmapHeicImageFromPath(imagePath, rotation);

    public bool FileExists(string fullPath) => _storageService.FileExists(fullPath);

    public List<string> GetRecentTargetPaths() => _assetRepository.GetRecentTargetPaths();

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
}
