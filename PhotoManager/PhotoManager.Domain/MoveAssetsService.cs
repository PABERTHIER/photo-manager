namespace PhotoManager.Domain;

public class MoveAssetsService : IMoveAssetsService
{
    //private const int RECENT_TARGET_PATHS_MAX_COUNT = 20; // MoveAssetsService should not have this responsability
    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;
    private readonly ICatalogAssetsService _catalogAssetsService;

    public MoveAssetsService(
        IAssetRepository assetRepository,
        IStorageService storageService,
        ICatalogAssetsService catalogAssetsService)
    {
        _assetRepository = assetRepository;
        _storageService = storageService;
        _catalogAssetsService = catalogAssetsService;
    }

    public bool MoveAssets(Asset[] assets, Folder destinationFolder, bool preserveOriginalFiles)
    {
        #region Parameters validation

        // TODO: Refacto and optimize it

        // if (assets == null || assets.Length == 0)
        // {
        //     throw new ArgumentNullException(nameof(assets), "Assets cannot be null or empty.");
        // }

        // if (destinationFolder == null)
        // {
        //     throw new ArgumentNullException(nameof(destinationFolder), "destinationFolder cannot be null.");
        // }

        // if (assets.Any(asset => asset == null || asset.Folder == null)) // To refacto it, passing a delegate ?
        // {
        //     throw new ArgumentNullException(nameof(assets), "An asset or its folder cannot be null.");
        // }

        if (assets == null || assets.Length == 0)
        {
            throw new ArgumentNullException(nameof(assets), "Assets cannot be null.");
        }

        if (destinationFolder == null)
        {
            throw new ArgumentNullException(nameof(destinationFolder), "destinationFolder cannot be null.");
        }

        foreach (Asset asset in assets)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "asset cannot be null.");
            }

            if (asset.Folder == null)
            {
                throw new ArgumentNullException(nameof(asset.Folder), "asset.Folder cannot be null.");
            }
        }

        #endregion

        bool result = false;
        Folder? folder = _assetRepository.GetFolderByPath(destinationFolder.Path); // If the folder is null, it means is not present in the catalog

        // TODO: IF THE DESTINATION FOLDER IS NEW, THE FOLDER NAVIGATION CONTROL SHOULD DISPLAY IT WHEN THE USER GOES BACK TO THE MAIN WINDOW.
        bool isDestinationFolderInCatalog = folder != null;

        if (isDestinationFolderInCatalog)
        {
            destinationFolder = folder!;
        }

        foreach (Asset asset in assets)
        {
            string sourceFilePath = asset.FullPath;
            string destinationFilePath = Path.Combine(destinationFolder.Path, asset.FileName);

            result = CopyAsset(sourceFilePath, destinationFilePath);

            if (!result)
            {
                break;
            }

            if (!isDestinationFolderInCatalog)
            {
                destinationFolder = _assetRepository.AddFolder(destinationFolder.Path);
                isDestinationFolderInCatalog = true;

            }

            _catalogAssetsService.CreateAsset(destinationFolder.Path, asset.FileName);
        }

        if (result)
        {
            if (!preserveOriginalFiles && !IsInSameDirectory(assets[0], destinationFolder))
            {
                DeleteAssets(assets, false);
            }

            UpdateTargetPathToRecent(destinationFolder);
            _assetRepository.SaveCatalog(destinationFolder);
        }

        return result;
    }

    public void DeleteAssets(Asset[] assets, bool saveCatalog)
    {
        #region Parameters validation

        // TODO: Refacto and optimize it
        if (assets == null || assets.Length == 0)
        {
            throw new ArgumentNullException(nameof(assets), "Assets cannot be null.");
        }

        foreach (Asset asset in assets)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "Asset cannot be null.");
            }

            if (asset.Folder == null)
            {
                throw new ArgumentNullException(nameof(asset), "Asset.Folder cannot be null.");
            }

            if (!_storageService.FileExists(asset, asset.Folder))
            {
                throw new ArgumentException("File does not exist: " + asset.FullPath);
            }
        }

        #endregion

        foreach (Asset asset in assets)
        {
            _assetRepository.DeleteAsset(asset.Folder.Path, asset.FileName);
            _storageService.DeleteFile(asset.Folder.Path, asset.FileName);
        }

        if (saveCatalog)
        {
            _assetRepository.SaveCatalog(assets[0].Folder);
        }
    }

    public bool CopyAsset(string sourceFilePath, string destinationFilePath)
    {
        if (_storageService.FileExists(destinationFilePath))
        {
            // TODO: Log the file already exists in the destination
            return _storageService.FileExists(sourceFilePath);
        }

        if (string.IsNullOrWhiteSpace(destinationFilePath))
        {
            // TODO: Log the destinationFolderPath is null or empty
            return false;
        }

        string destinationFolderPath = new FileInfo(destinationFilePath).Directory!.FullName;

        if (!Directory.Exists(destinationFolderPath))
        {
            _storageService.CreateDirectory(destinationFolderPath);
        }

        // TODO: try catch to handle errors
        File.Copy(sourceFilePath, destinationFilePath);

        return _storageService.FileExists(sourceFilePath) && _storageService.FileExists(destinationFilePath);
    }

    private static bool IsInSameDirectory(Asset asset, Folder destinationFolder)
    {
        return asset.Folder.Path.Equals(destinationFolder.Path);
    }

    private void UpdateTargetPathToRecent(Folder destinationFolder)
    {
        List<string> recentTargetPaths = _assetRepository.GetRecentTargetPaths();

        if (recentTargetPaths.Contains(destinationFolder.Path))
        {
            recentTargetPaths.Remove(destinationFolder.Path);
        }

        recentTargetPaths.Insert(0, destinationFolder.Path);

        //recentTargetPaths = recentTargetPaths.Take(RECENT_TARGET_PATHS_MAX_COUNT).ToList(); // TODO: MoveAssetsService should not have this responsability
        _assetRepository.SaveRecentTargetPaths(recentTargetPaths);
    }
}
