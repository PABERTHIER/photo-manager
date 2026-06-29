using PhotoManager.Common.Imaging;
using PhotoManager.Domain.Interfaces;
using PhotoManager.Domain.UserConfigurationSettings;
using System.Reflection;

namespace PhotoManager.Application;

public class Application(
    IAssetRepository assetRepository,
    ISyncAssetsService syncAssetsService,
    ICatalogAssetsService catalogAssetsService,
    IMoveAssetsService moveAssetsService,
    IFindDuplicatedAssetsService findDuplicatedAssetsService,
    IUserConfigurationService userConfigurationService,
    IFileOperationsService fileOperationsService,
    IImageProcessingService imageProcessingService,
    IAssetConversionService assetConversionService)
    : IApplication
{
    // Catalog
    public async Task CatalogAssetsAsync(CatalogChangeCallback callback, CancellationToken token = default) =>
        await catalogAssetsService.CatalogAssetsAsync(callback, token);

    /// <summary>
    /// Detects duplicated assets in the catalog.
    /// </summary>
    /// <returns>An array of duplicated sets of assets (corresponding to the same image),
    /// where each item is an array of duplicated assets.</returns>
    public Asset[][] GetDuplicatedAssets() => findDuplicatedAssetsService.GetDuplicatedAssets();

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
        IImageData? imageData = assetRepository.LoadThumbnail(asset.Folder.Path, asset.FileName,
            asset.Pixel.Thumbnail.Width, asset.Pixel.Thumbnail.Height);
        asset.ImageData?.Dispose();
        asset.ImageData = imageData;
    }

    public Folder[] GetSubFolders(Folder parentFolder) => assetRepository.GetSubFolders(parentFolder);

    public List<string> GetRecentTargetPaths()
    {
        string[] recentTargetPaths = assetRepository.GetRecentTargetPaths();
        List<string> result = new(recentTargetPaths.Length);

        for (int i = 0; i < recentTargetPaths.Length; i++)
        {
            result.Add(recentTargetPaths[i]);
        }

        return result;
    }

    public int GetAssetsCounter() => assetRepository.GetAssetsCounter();

    // UserConfigurationService
    public string GetInitialFolderPath() => userConfigurationService.PathSettings.AssetsDirectory;

    public ushort GetCatalogCooldownMinutes() => userConfigurationService.AssetSettings.CatalogCooldownMinutes;

    public bool GetSyncAssetsEveryXMinutes() => userConfigurationService.AssetSettings.SyncAssetsEveryXMinutes;

    public string GetExemptedFolderPath() => userConfigurationService.PathSettings.ExemptedFolderPath;

    public AboutInformation GetAboutInformation(Assembly assembly) =>
        userConfigurationService.GetAboutInformation(assembly);

    public EditableUserConfiguration GetEditableConfiguration() =>
        userConfigurationService.GetEditableConfiguration();

    public void SaveEditableConfiguration(EditableUserConfiguration configuration) =>
        userConfigurationService.SaveEditableConfiguration(configuration);

    // ImageProcessingService
    public IImageData LoadBitmapImageFromPath(string imagePath, ImageRotation rotation) =>
        imageProcessingService.LoadBitmapImageFromPath(imagePath, rotation);

    // FileOperationsService
    public bool FileExists(string fullPath) => fileOperationsService.FileExists(fullPath);

    public int GetTotalFilesCount() => fileOperationsService.GetTotalFilesCount();

    // AssetConversionService
    public async Task<AssetConversionResult> ConvertAssetsAsync(AssetConversionRequest request,
        AssetConversionProgressCallback callback, CancellationToken token = default) =>
        await assetConversionService.ConvertAssetsAsync(request, callback, token);

    // MoveAssetsService
    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles) =>
        moveAssetsService.MoveAssets(assets, destinationFolder, preserveOriginalFiles);

    public void DeleteAssets(Asset[] assets) => moveAssetsService.DeleteAssets(assets);

    // Sync
    public SyncAssetsConfiguration GetSyncAssetsConfiguration() => assetRepository.GetSyncAssetsConfiguration();

    public void SetSyncAssetsConfiguration(SyncAssetsConfiguration syncConfiguration)
    {
        syncConfiguration.Validate().Normalize();
        assetRepository.SaveSyncAssetsConfiguration(syncConfiguration);
    }

    public async Task<List<SyncAssetsResult>> SyncAssetsAsync(ProcessStatusChangedCallback callback) =>
        await syncAssetsService.ExecuteAsync(callback);
}
